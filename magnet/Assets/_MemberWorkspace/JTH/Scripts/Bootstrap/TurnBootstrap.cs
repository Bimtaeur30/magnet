using System;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Score;
using JTH.Scripts.Domain.Turn;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Core.Events;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    public class TurnBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private ScoreConfigSO scoreConfig;
        
        [Inject] private BlockSpawnBootstrap _blockSpawnBootstrap;
        [Inject] private GameBoard _gameBoard;
        
        private ScoreSession _scoreSession;
        
        private void Awake()
        {
            inGameChannel.AddListener<BlockPlacedEvent>(BlockPlacedHandler);
            
            _scoreSession = new ScoreSession();
        }

        private void OnDestroy()
        {
            inGameChannel.RemoveListener<BlockPlacedEvent>(BlockPlacedHandler);
        }

        private void BlockPlacedHandler(BlockPlacedEvent evt)
        {
            if (TurnService.IsGameOver(_gameBoard.Grid, evt.Candidates))
            {
                magnetGameChannel.RaiseEvent(MagnetGameEvents.GameOverEvent);
            }
        }
    }
}