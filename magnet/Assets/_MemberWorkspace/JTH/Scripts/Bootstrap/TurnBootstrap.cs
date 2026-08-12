using System.Collections.Generic;
using GameLib.EventChannelSystem;
using _Shared.Magnet.Core.SO.Skin;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Domain.Score;
using JTH.Scripts.Domain.Turn;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Core.Events;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    public sealed class TurnBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO enemyChannel;
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private EventChannelSO skinChannel;
        [SerializeField] private ScoreConfigSO scoreConfig;
        
        [Inject] private readonly GameBoard _gameBoard;
        [Inject] private readonly BlockSpawnBootstrap _blockSpawnBootstrap;
        
        private ScoreSession _scoreSession;
        private int _currentStage;

        private void Awake()
        {
            Debug.Assert(enemyChannel != null, "[TurnBootstrap] enemyChannel is not assigned.", this);
            Debug.Assert(inGameChannel != null, "[TurnBootstrap] inGameChannel is not assigned.", this);
            Debug.Assert(magnetGameChannel != null, "[TurnBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(skinChannel != null, "[TurnBootstrap] skinChannel is not assigned.", this);
            Debug.Assert(scoreConfig != null, "[TurnBootstrap] scoreConfig is not assigned.", this);
            Debug.Assert(_gameBoard != null, "[TurnBootstrap] GameBoard was not injected.", this);

            _scoreSession = new ScoreSession(scoreConfig);
        }

        private void OnEnable()
        {
            inGameChannel.AddListener<BlockPlacedEvent>(BlockPlacedHandler);
            enemyChannel.AddListener<StageClearEvent>(StageClearHandler);
        }

        private void OnDisable()
        {
            inGameChannel?.RemoveListener<BlockPlacedEvent>(BlockPlacedHandler);
            enemyChannel?.RemoveListener<StageClearEvent>(StageClearHandler);
        }

        private void StageClearHandler(StageClearEvent evt)
        {
            _currentStage = evt.ClearStageIdx;
        }

        private void BlockPlacedHandler(BlockPlacedEvent evt)
        {
            PlacementResult placementResult = evt.PlacementResult;
            int comboBefore = _scoreSession.Combo;

            PlacementScoreResult scoreResult = _scoreSession.ApplyPlacement(
                placementResult.ClearedLineResult.ClearedLineCount,
                placementResult.PlacedGridPositions.Count,
                placementResult.FirstDrop,
                placementResult.LastDrop);

            magnetGameChannel.RaiseEvent(MagnetGameEvents.ScoreChangedEvent.Init(scoreResult.TotalScore));

            RaiseAttackEvent(evt, scoreResult);
            RaiseComboChangedIfNeeded(comboBefore, scoreResult.ComboAfter, placementResult);

            if (evt.PlacementResult.LastDrop)
            {
                _blockSpawnBootstrap.Fill();
            }

            if (TurnService.IsGameOver(_gameBoard.Grid, _blockSpawnBootstrap.Candidates))
            {
                skinChannel.RaiseEvent(
                    SkinEvents.SkinUnlockCheckEvent.Init(
                        SkinUnlockTypeEnum.Stage,
                        _currentStage));

                magnetGameChannel.RaiseEvent(MagnetGameEvents.GameOverEvent.Init(_currentStage));
            }
        }

        private void RaiseAttackEvent(BlockPlacedEvent evt, PlacementScoreResult scoreResult)
        {
            Dictionary<Vector3, float> lineClearScoreDict = new Dictionary<Vector3, float>();

            Vector2Int minGrid = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int maxGrid = new Vector2Int(int.MinValue, int.MinValue);
            foreach (Vector2Int grid in evt.PlacementResult.PlacedGridPositions)
            {
                if (grid.x < minGrid.x)
                    minGrid.x = grid.x;
                if (grid.y < minGrid.y)
                    minGrid.y = grid.y;
                if (grid.x > maxGrid.x)
                    maxGrid.x = grid.x;
                if (grid.y > maxGrid.y)
                    maxGrid.y = grid.y;
            }

            Vector2 minWorld = _gameBoard.GridToWorld(minGrid);
            Vector2 maxWorld = _gameBoard.GridToWorld(maxGrid);
            Vector2 cellWorldSize = _gameBoard.GridToWorld(Vector2Int.right) - _gameBoard.GridToWorld(Vector2Int.zero);
            Vector2 blockCenter = (minWorld + maxWorld + cellWorldSize) * 0.5f;
            int placedCellScore = evt.PlacementResult.PlacedGridPositions.Count * scoreConfig.CellScore;
            float damageScale = scoreConfig.EnemyDamageMultiplier;
            
            HashSet<Vector2Int> destroyedCells = new HashSet<Vector2Int>();
            foreach (Line line in evt.PlacementResult.ClearedLineResult.ClearedLines)
            {
                foreach (Vector2Int grid in line.GetCells(_gameBoard.Grid.BoardSize))
                {
                    destroyedCells.Add(grid);
                }
            }
            
            int breakScore = scoreResult.ScoreDelta - placedCellScore;
            foreach (Vector2Int grid in destroyedCells)
            {
                Vector3 worldPos = _gameBoard.GridToWorld(grid);
                lineClearScoreDict.TryAdd(worldPos, breakScore / (float)destroyedCells.Count * damageScale);
            }

            enemyChannel.RaiseEvent(
                EnemyEvents.EnemyAttackRequestEvent.Init(blockCenter, placedCellScore * damageScale));
            foreach (Vector3 worldPos in lineClearScoreDict.Keys)
            {
                enemyChannel.RaiseEvent(
                    EnemyEvents.EnemyAttackRequestEvent.Init(worldPos, lineClearScoreDict[worldPos]));
            }
        }

        private void RaiseComboChangedIfNeeded(
            int comboBefore,
            int comboAfter,
            PlacementResult placementResult)
        {
            if (comboAfter == comboBefore)
            {
                return;
            }

            Vector3 worldPosition = ResolveComboWorldPosition(placementResult);
            magnetGameChannel.RaiseEvent(
                MagnetGameEvents.ComboChangedEvent.Init(comboAfter, worldPosition));
        }

        /// <summary>
        /// 콤보 터진(클리어) 칸들의 월드 중심. 클리어가 없으면 배치 블록 중심.
        /// </summary>
        private Vector3 ResolveComboWorldPosition(PlacementResult placementResult)
        {
            Vector2Int minGrid = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int maxGrid = new Vector2Int(int.MinValue, int.MinValue);
            bool hasClearedCell = false;

            foreach (Line line in placementResult.ClearedLineResult.ClearedLines)
            {
                foreach (Vector2Int grid in line.GetCells(_gameBoard.Grid.BoardSize))
                {
                    hasClearedCell = true;
                    ExpandBounds(ref minGrid, ref maxGrid, grid);
                }
            }

            if (!hasClearedCell)
            {
                foreach (Vector2Int grid in placementResult.PlacedGridPositions)
                {
                    ExpandBounds(ref minGrid, ref maxGrid, grid);
                }
            }

            Vector2 minWorld = _gameBoard.GridToWorld(minGrid);
            Vector2 maxWorld = _gameBoard.GridToWorld(maxGrid);
            Vector2 cellWorldSize =
                _gameBoard.GridToWorld(Vector2Int.right) - _gameBoard.GridToWorld(Vector2Int.zero);
            return (minWorld + maxWorld + cellWorldSize) * 0.5f;
        }

        private static void ExpandBounds(ref Vector2Int minGrid, ref Vector2Int maxGrid, Vector2Int grid)
        {
            if (grid.x < minGrid.x)
                minGrid.x = grid.x;
            if (grid.y < minGrid.y)
                minGrid.y = grid.y;
            if (grid.x > maxGrid.x)
                maxGrid.x = grid.x;
            if (grid.y > maxGrid.y)
                maxGrid.y = grid.y;
        }
    }
}
