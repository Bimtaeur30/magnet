using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
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
                placementResult.CellsPlaced,
                placementResult.FirstDrop,
                placementResult.LastDrop);

            magnetGameChannel.RaiseEvent(MagnetGameEvents.ScoreChangedEvent.Init(scoreResult.TotalScore));
            RaiseComboChangedIfNeeded(comboBefore, scoreResult.ComboAfter);

            if (TurnService.IsGameOver(_gameBoard.Grid, placementResult.Candidates))
            {
                magnetGameChannel.RaiseEvent(MagnetGameEvents.GameOverEvent.Init(scoreResult.TotalScore));
                return;
            }

            if (evt.PlacementResult.LastDrop)
            {
                _blockSpawnBootstrap.Fill();
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