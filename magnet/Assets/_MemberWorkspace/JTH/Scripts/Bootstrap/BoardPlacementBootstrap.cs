using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Events;
using JTH.Scripts.Presentation;
using Magnet.Contracts;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    public sealed class BoardPlacementBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO inGameChannel;

        [Inject] private readonly BlockSpawnBootstrap _blockSpawnBootstrap;
        [Inject] private GameBoard _gameBoard;

        private void Awake()
        {
            Debug.Assert(inGameChannel != null, "[BoardPlacementBootstrap] inGameChannel is not assigned.", this);
            Debug.Assert(_blockSpawnBootstrap != null, "[BoardPlacementBootstrap] BlockSpawnBootstrap was not injected.", this);
            Debug.Assert(_gameBoard != null, "[BoardPlacementBootstrap] GameBoard was not injected.", this);
        }

        public void PlaceBlock(
            IReadOnlyList<Block> detached,
            IReadOnlyList<Vector2Int> gridOffsets,
            int slotIndex)
        {
            if (!IsPlacementFree(gridOffsets))
            {
                _gameBoard.ReturnUnplacedBlocks(detached);
                return;
            }

            int filledBefore = CountFilledSlots();
            bool firstDrop = filledBefore == BlockSupply.SlotCount;
            bool lastDrop = filledBefore == 1;

            _gameBoard.AddBlock(detached, gridOffsets);
            _blockSpawnBootstrap.Consume(slotIndex);

            ClearedLineResult clearedLineResult = LineClearService.DetectAndApply(_gameBoard);

            PlacementResult placementResult = new PlacementResult(
                _blockSpawnBootstrap.Candidates,
                gridOffsets,
                clearedLineResult,
                firstDrop,
                lastDrop);

            inGameChannel.RaiseEvent(InGameEvents.BlockPlacedEvent.Init(placementResult));
        }

        private bool IsPlacementFree(IReadOnlyList<Vector2Int> gridOffsets)
        {
            for (int i = 0; i < gridOffsets.Count; ++i)
            {
                Vector2Int cell = gridOffsets[i];
                if (!_gameBoard.Grid.IsInBounds(cell) || _gameBoard.Grid.IsOccupied(cell))
                {
                    return false;
                }
            }

            return true;
        }

        private int CountFilledSlots()
        {
            IReadOnlyList<ShapeBlockData> candidates = _blockSpawnBootstrap.Candidates;

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
    }
}
