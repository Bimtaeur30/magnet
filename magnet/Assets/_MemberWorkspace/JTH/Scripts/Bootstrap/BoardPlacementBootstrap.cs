using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Domain.Rotation;
using JTH.Scripts.Domain.Score;
using JTH.Scripts.Domain.Turn;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Contracts.BlockShapes;
using PMS.Scripts.Events;
using PMS.Scripts.Skin;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    /// <summary>
    /// BoardSession·BlockPlacementService 생성, 부착 후 클리어 재조립 연쇄·회전·Consume까지 순차 처리한다.
    /// </summary>
    public sealed class BoardPlacementBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private EventChannelSO skinChannel;
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;
        [SerializeField] private ScoreConfigSO scoreConfig;

        [Inject] private readonly BlockSpawnBootstrap _blockSpawnBootstrap;
        [Inject] private readonly PlacedBlocksView _placedBlocksView;
        [Inject] private readonly BoardView _boardView;

        private BoardSession _session;
        private BlockPlacementService _placementService;
        private ScoreSession _scoreSession;
        private readonly ClearReassemblyService _reassemblyService = new();
        private readonly BoardRotationService _rotationService = new();

        public BoardSession Session => _session;
        public BlockPlacementService PlacementService => _placementService;
        public ScoreSession ScoreSession => _scoreSession;
        public BlockSpawnBootstrap BlockSpawn => _blockSpawnBootstrap;
        public bool IsTurnResolving { get; private set; }

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[BoardPlacementBootstrap] BoardConfigSO is not assigned.", this);
            Debug.Assert(placementConfig != null, "[BoardPlacementBootstrap] PlacementConfigSO is not assigned.", this);
            Debug.Assert(scoreConfig != null, "[BoardPlacementBootstrap] ScoreConfigSO is not assigned.", this);
            Debug.Assert(skinChannel != null, "[BoardPlacementBootstrap] Skin EventChannelSO is not assigned.", this);
            Debug.Assert(_blockSpawnBootstrap != null, "[BoardPlacementBootstrap] BlockSpawnBootstrap was not injected.", this);
            Debug.Assert(_placedBlocksView != null, "[BoardPlacementBootstrap] PlacedBlocksView was not injected.", this);
            Debug.Assert(_boardView != null, "[BoardPlacementBootstrap] BoardView was not injected.", this);

            _session = new BoardSession(boardConfig.BoardSize);
            _placementService = new BlockPlacementService(_session);
            _scoreSession = new ScoreSession(scoreConfig);
        }

        private void OnEnable()
        {
            magnetGameChannel.AddListener<TurnEndedEvent>(OnTurnEnded);
        }

        private void OnDisable()
        {
            magnetGameChannel.RemoveListener<TurnEndedEvent>(OnTurnEnded);
        }

        private void OnTurnEnded(TurnEndedEvent _)
        {
            _scoreSession?.NotifyTurnEnded();
        }

        /// <summary>
        /// Place → Clear재조립 연쇄 → Rotate 를 Domain → Raise → await FX 순으로 실행한다.
        /// </summary>
        public async UniTask<TurnResolutionResult> TryConfirmPlacement(
            IBlockShape shape,
            Vector2Int startPivot,
            int slotIndex,
            ShapeBlock staging)
        {
            IsTurnResolving = true;
            try
            {
                PlacementResult result = _placementService.TryPlace(shape, startPivot);
                if (!result.Success)
                {
                    if (staging != null)
                    {
                        staging.Clear();
                        Destroy(staging.gameObject);
                    }

                    return new TurnResolutionResult(result, ClearReassemblyResult.None, boardRotated: false);
                }

                _blockSpawnBootstrap.Consume(slotIndex);

                await _placedBlocksView.PlayPlaceAsync(staging, result);

                magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockPlacedEvent.Init(
                    result.BlockId,
                    slotIndex,
                    shape.ShapeId,
                    result.FinalPivot,
                    result.CellPositions));

                ClearReassemblyResult reassembly = _reassemblyService.ResolveAllWaves(
                    _session,
                    placementConfig.ClearReassemblyRule.CorridorHalfWidth);
                PlacementScoreResult scoreResult = ApplyPlacementScore(result, reassembly);
                magnetGameChannel.RaiseEvent(MagnetGameEvents.ScoreChangedEvent.Init(scoreResult.TotalScore));
                await PlayClearReassemblyWavesAsync(reassembly, scoreResult);

                if (reassembly.HasCellsOutsideBounds)
                {
                    int finalScore = _scoreSession.TotalScore;
                    RaiseScoreSkinUnlockCheck(finalScore);
                    magnetGameChannel.RaiseEvent(MagnetGameEvents.GameOverEvent.Init(finalScore));
                    return new TurnResolutionResult(result, reassembly, boardRotated: false);
                }

                if (placementConfig.Rotation.PreRotationDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(placementConfig.Rotation.PreRotationDelay));
                }

                _rotationService.RotateClockwise(_session);
                magnetGameChannel.RaiseEvent(MagnetGameEvents.BoardRotatedEvent.Init(
                    BoardRotationService.DefaultDegreesClockwise));

                await _placedBlocksView.PlayRotateAsync();


                return new TurnResolutionResult(result, reassembly, boardRotated: true);
            }
            finally
            {
                IsTurnResolving = false;
            }
        }

        private void RaiseScoreSkinUnlockCheck(int totalScore)
        {
            skinChannel.RaiseEvent(
                SkinEvents.SkinUnlockCheckEvent.Init(SkinUnlockTypeEnum.Score, totalScore));
        }

        private PlacementScoreResult ApplyPlacementScore(PlacementResult placement, ClearReassemblyResult reassembly)
        {
            int cellsPlaced = placement.CellPositions != null ? placement.CellPositions.Count : 0;
            if (reassembly == null || !reassembly.HasAnyWave)
            {
                return _scoreSession.ApplyPlacement(cellsPlaced, waveSquareSizes: null);
            }

            var waveSizes = new int[reassembly.Waves.Count];
            for (int i = 0; i < reassembly.Waves.Count; i++)
            {
                waveSizes[i] = reassembly.Waves[i].SquareSize;
            }

            return _scoreSession.ApplyPlacement(cellsPlaced, waveSizes);
        }

        private async UniTask PlayClearReassemblyWavesAsync(
            ClearReassemblyResult reassembly,
            PlacementScoreResult scoreResult)
        {
            if (reassembly == null || !reassembly.HasAnyWave)
            {
                return;
            }

            IReadOnlyList<int> waveScores = scoreResult.WaveScores;
            for (int w = 0; w < reassembly.Waves.Count; w++)
            {
                ClearWave wave = reassembly.Waves[w];
                int scoreAwarded = w < waveScores.Count ? waveScores[w] : 0;
                RaiseWaveEvents(wave, scoreAwarded);

                await PlayExplosionWaveAsync(wave);
                await _placedBlocksView.PlayWaveRelocationsAsync(wave);
            }
        }

        private async UniTask PlayExplosionWaveAsync(ClearWave wave)
        {
            UniTask borderPulse = ExplosionBorderPulseView.PlayAsync(
                _boardView,
                wave.SquareSize,
                boardConfig,
                placementConfig.ExplosionBorder);
            _placedBlocksView.DestroyWaveCellViews(wave.DestroyedCellIds);
            await borderPulse;
        }

        private void RaiseWaveEvents(ClearWave wave, int scoreAwarded)
        {
            magnetGameChannel.RaiseEvent(MagnetGameEvents.SquareClearedEvent.Init(
                wave.SquareSize,
                scoreAwarded,
                wave.DestroyedCells));

            if (wave.Relocations.Count == 0)
            {
                return;
            }

            var cellIds = new List<int>(wave.Relocations.Count);
            var fromCells = new List<Vector2Int>(wave.Relocations.Count);
            var toCells = new List<Vector2Int>(wave.Relocations.Count);
            for (int i = 0; i < wave.Relocations.Count; i++)
            {
                CellRelocation relocation = wave.Relocations[i];
                cellIds.Add(relocation.CellId);
                fromCells.Add(relocation.From);
                toCells.Add(relocation.To);
            }

            magnetGameChannel.RaiseEvent(MagnetGameEvents.CellsRelocatedEvent.Init(
                wave.SquareSize,
                cellIds,
                fromCells,
                toCells));
        }
    }
}
