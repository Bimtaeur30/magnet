using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Score;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Contracts;
using Magnet.Core.Events;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    public sealed class BoardPlacementBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private EventChannelSO skinChannel;
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;
        [SerializeField] private ScoreConfigSO scoreConfig;

        [Inject] private readonly BlockSpawnBootstrap _blockSpawnBootstrap;
        [Inject] private GameBoard _gameBoard;

        private ScoreSession _scoreSession;

        public ScoreSession ScoreSession => _scoreSession;

        private void Awake()
        {
            Debug.Assert(boardConfig != null, "[BoardPlacementBootstrap] BoardConfigSO is not assigned.", this);
            Debug.Assert(placementConfig != null, "[BoardPlacementBootstrap] PlacementConfigSO is not assigned.", this);
            Debug.Assert(scoreConfig != null, "[BoardPlacementBootstrap] ScoreConfigSO is not assigned.", this);
            Debug.Assert(magnetGameChannel != null, "[BoardPlacementBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(_blockSpawnBootstrap != null, "[BoardPlacementBootstrap] BlockSpawnBootstrap was not injected.", this);

            _scoreSession = new ScoreSession(scoreConfig);
        }

        /// <summary>
        /// Place → line clear → ScoreSession 반영.
        /// </summary>
        public void PlaceBlock(
            IReadOnlyList<Block> detached,
            Vector2Int finalPivot,
            IReadOnlyList<Vector2Int> cellOffsets,
            int slotIndex)
        {
            int filledBefore = CountFilledSlots();
            bool firstDrop = filledBefore == BlockSupply.SlotCount;
            bool lastDrop = filledBefore == 1;
            int cellsPlaced = cellOffsets.Count;
            int comboBefore = _scoreSession.Combo;

            _blockSpawnBootstrap.Consume(slotIndex);
            _gameBoard.AddBlock(detached, finalPivot, cellOffsets);

            ClearedLineResult clearResult = LineClearService.DetectAndApply(
                _gameBoard);

            PlacementScoreResult scoreResult = ApplyPlacementScore(
                cellsPlaced,
                clearResult.ClearedLineCount,
                firstDrop,
                lastDrop);

            magnetGameChannel.RaiseEvent(MagnetGameEvents.ScoreChangedEvent.Init(scoreResult.TotalScore));
            RaiseComboChangedIfNeeded(comboBefore, scoreResult.ComboAfter);
            
            inGameChannel.RaiseEvent(InGameEvents.BlockPlacedEvent.Init(_blockSpawnBootstrap.Supply.Candidates));
        }

        private int CountFilledSlots()
        {
            IReadOnlyList<ShapeBlockData> candidates = _blockSpawnBootstrap.Supply.Candidates;
            
            int filled = 0;
            foreach (var block in candidates)
            {
                if (block != null)
                {
                    filled++;
                }
            }

            return filled;
        }
        
        private void RaiseComboChangedIfNeeded(int comboBefore, int comboAfter)
        {
            if (comboAfter == comboBefore)
            {
                return;
            }

            magnetGameChannel.RaiseEvent(MagnetGameEvents.ComboChangedEvent.Init(comboAfter));
        }

        private PlacementScoreResult ApplyPlacementScore(
            int cellsPlaced,
            int clearedLineCount,
            bool firstDrop,
            bool lastDrop)
        {
            return _scoreSession.ApplyPlacement(
                clearedLineCount,
                cellsPlaced,
                firstDrop,
                lastDrop);
        }
    }
}
