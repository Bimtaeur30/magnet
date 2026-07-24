using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Domain.Score;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Presentation;
using Magnet.Core.Events;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    // TODO: 턴 FSM·게임오버·클리어 FX와 재연결
    public sealed class BoardPlacementBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private EventChannelSO skinChannel;
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PlacementConfigSO placementConfig;
        [SerializeField] private ScoreConfigSO scoreConfig;

        [Inject] private readonly BlockSpawnBootstrap _blockSpawnBootstrap;
        [Inject] private readonly PlacedBlocksView _placedBlocksView;
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
            Debug.Assert(_placedBlocksView != null, "[BoardPlacementBootstrap] PlacedBlocksView was not injected.", this);

            _scoreSession = new ScoreSession(scoreConfig);
        }

        /// <summary>
        /// Place → line clear → ScoreSession 반영.
        /// </summary>
        public PlacementResult PlaceBlock(
            Vector2Int finalPivot,
            int slotIndex,
            ShapeBlock staging)
        {
            int filledBefore = CountFilledSlots();
            bool firstDrop = filledBefore == BlockSupply.SlotCount;
            bool lastDrop = filledBefore == 1;
            int cellsPlaced = staging.CellOffsets.Count;
            int comboBefore = _scoreSession.Combo;

            _blockSpawnBootstrap.Consume(slotIndex);
            _placedBlocksView.PlaceStagingBlock(staging, finalPivot);

            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockPlacedEvent.Init(
                finalPivot,
                staging.CellOffsets));

            ClearedLineResult clearResult = LineClearService.DetectAndApply(
                _gameBoard,
                GetChangedPositions(staging.CellOffsets, finalPivot));

            PlacementScoreResult scoreResult = ApplyPlacementScore(
                cellsPlaced,
                clearResult.ClearedLineCount,
                firstDrop,
                lastDrop);

            magnetGameChannel.RaiseEvent(MagnetGameEvents.ScoreChangedEvent.Init(scoreResult.TotalScore));
            RaiseComboChangedIfNeeded(comboBefore, scoreResult.ComboAfter);

            return new PlacementResult(gameOver: false);
        }

        private static int CountFilledSlots(IReadOnlyList<ShapeBlock> candidates)
        {
            int filled = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] != null)
                {
                    filled++;
                }
            }

            return filled;
        }

        private int CountFilledSlots()
            => CountFilledSlots(_blockSpawnBootstrap.Supply.Candidates);

        private IReadOnlyList<Vector2Int> GetChangedPositions(
            IReadOnlyList<Vector2Int> cellOffsets,
            Vector2Int finalPivot)
        {
            var changed = new List<Vector2Int>(cellOffsets.Count);
            for (int i = 0; i < cellOffsets.Count; i++)
            {
                changed.Add(cellOffsets[i] + finalPivot);
            }

            return changed;
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
