using System.Collections.Generic;
using GameLib.EventChannelSystem;
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
        [SerializeField] private ScoreConfigSO scoreConfig;
        
        [Inject] private readonly GameBoard _gameBoard;
        [Inject] private readonly BlockSpawnBootstrap _blockSpawnBootstrap;
        
        private ScoreSession _scoreSession;
        
        private void Awake()
        {
            Debug.Assert(inGameChannel != null, "[TurnBootstrap] inGameChannel is not assigned.", this);
            Debug.Assert(magnetGameChannel != null, "[TurnBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(scoreConfig != null, "[TurnBootstrap] scoreConfig is not assigned.", this);
            Debug.Assert(_gameBoard != null, "[TurnBootstrap] GameBoard was not injected.", this);

            _scoreSession = new ScoreSession(scoreConfig);
        }

        private void OnEnable()
        {
            inGameChannel.AddListener<BlockPlacedEvent>(BlockPlacedHandler);
        }

        private void OnDisable()
        {
            inGameChannel?.RemoveListener<BlockPlacedEvent>(BlockPlacedHandler);
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

            RaiseAttackEvent(evt, scoreResult);
            RaiseComboChangedIfNeeded(comboBefore, scoreResult.ComboAfter);

            if (evt.PlacementResult.LastDrop)
            {
                _blockSpawnBootstrap.Fill();
            }

            if (TurnService.IsGameOver(_gameBoard.Grid, _blockSpawnBootstrap.Candidates))
            {
                magnetGameChannel.RaiseEvent(MagnetGameEvents.GameOverEvent.Init(scoreResult.TotalScore));
            }
        }

        private void RaiseAttackEvent(BlockPlacedEvent evt, PlacementScoreResult scoreResult)
        {
            Dictionary<Vector3, float> scoreDict = new Dictionary<Vector3, float>();
            foreach (Vector2Int grid in evt.PlacementResult.PlacedGridPositions)
            {
                scoreDict.Add(_gameBoard.GridToWorld(grid), scoreConfig.CellScore);
            }
            
            HashSet<Vector2Int> destroyedCells = new HashSet<Vector2Int>();
            foreach (Line line in evt.PlacementResult.ClearedLineResult.ClearedLines)
            {
                foreach (Vector2Int grid in line.GetCells(_gameBoard.Grid.BoardSize))
                {
                    destroyedCells.Add(grid);
                }
            }
            
            int breakScore = scoreResult.ScoreDelta - scoreDict.Count * scoreConfig.CellScore;
            foreach (Vector2Int grid in destroyedCells)
            {
                Vector3 worldPos = _gameBoard.GridToWorld(grid);
                if (scoreDict.TryAdd(worldPos, (float)breakScore / destroyedCells.Count))
                    scoreDict[worldPos] += breakScore;
            }

            foreach (Vector3 worldPos in scoreDict.Keys)
            {
                enemyChannel.RaiseEvent(EnemyEvents.EnemyAttackRequestEvent.Init(worldPos, scoreDict[worldPos]));
            }
        }

        private void RaiseComboChangedIfNeeded(int comboBefore, int comboAfter)
        {
            if (comboAfter == comboBefore)
            {
                return;
            }

            magnetGameChannel.RaiseEvent(MagnetGameEvents.ComboChangedEvent.Init(comboAfter));
        }
    }
}