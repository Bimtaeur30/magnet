using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Domain.Rotation;
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

        [Inject] private readonly BlockSpawnBootstrap _blockSpawnBootstrap;
        [Inject] private readonly PlacedBlocksView _placedBlocksView;

        private BoardSession _session;
        private BlockPlacementService _placementService;
        private readonly ClearReassemblyService _reassemblyService = new();
        private readonly BoardRotationService _rotationService = new();

        public BoardSession Session => _session;
        public BlockPlacementService PlacementService => _placementService;
        public BlockSpawnBootstrap BlockSpawn => _blockSpawnBootstrap;
        public bool IsTurnResolving { get; private set; }

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[BoardPlacementBootstrap] BoardConfigSO is not assigned.", this);
            Debug.Assert(placementConfig != null, "[BoardPlacementBootstrap] PlacementConfigSO is not assigned.", this);
            Debug.Assert(_blockSpawnBootstrap != null, "[BoardPlacementBootstrap] BlockSpawnBootstrap was not injected.", this);
            Debug.Assert(_placedBlocksView != null, "[BoardPlacementBootstrap] PlacedBlocksView was not injected.", this);

            _session = new BoardSession(boardConfig.BoardSize);
            _placementService = new BlockPlacementService(_session);
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

                magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockPlacedEvent.Init(
                    result.BlockId,
                    slotIndex,
                    shape.ShapeId,
                    result.FinalPivot,
                    result.CellPositions));

                await _placedBlocksView.PlayPlaceAsync(staging, result);

                ClearReassemblyResult reassembly = _reassemblyService.ResolveAllWaves(_session);
                RaiseReassemblyEvents(reassembly);
                await _placedBlocksView.PlayReassemblyAsync(reassembly);

                if (reassembly.HasCellsOutsideBounds)
                {
                    magnetGameChannel.RaiseEvent(SkinEvents.SkinUnlockCheckEvent.Init(SkinUnlockTypeEnum.Score, 0));
                    magnetGameChannel.RaiseEvent(MagnetGameEvents.GameOverEvent.Init(0));
                    return new TurnResolutionResult(result, reassembly, boardRotated: false);
                }

                if (placementConfig.PreRotationDelay > 0f)
                {
                    await UniTask.Delay(System.TimeSpan.FromSeconds(placementConfig.PreRotationDelay));
                }

                _rotationService.RotateClockwise(_session);
                magnetGameChannel.RaiseEvent(MagnetGameEvents.BoardRotatedEvent.Init(
                    BoardRotationService.DefaultDegreesClockwise));

                await _placedBlocksView.PlayRotateAsync();

                _blockSpawnBootstrap.Consume(slotIndex);

                return new TurnResolutionResult(result, reassembly, boardRotated: true);
            }
            finally
            {
                IsTurnResolving = false;
            }
        }

        private void RaiseReassemblyEvents(ClearReassemblyResult reassembly)
        {
            if (reassembly == null || !reassembly.HasAnyWave)
            {
                return;
            }

            for (int w = 0; w < reassembly.Waves.Count; w++)
            {
                ClearWave wave = reassembly.Waves[w];
                magnetGameChannel.RaiseEvent(MagnetGameEvents.SquareClearedEvent.Init(
                    wave.SquareSize,
                    scoreAwarded: wave.ScoreCells,
                    wave.DestroyedCells));

                if (wave.Relocations.Count == 0)
                {
                    continue;
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
}
