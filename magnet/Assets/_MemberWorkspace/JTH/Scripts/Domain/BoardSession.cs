using System.Collections.Generic;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Domain.Rotation;
using UnityEngine;

namespace JTH.Scripts.Domain
{
    public sealed class BoardSession
    {
        private readonly BoardGrid _grid;
        private readonly List<PlacedBlock> _placedBlocks = new();
        private int _nextBlockId = 1;

        public BoardSession(int boardSize)
        {
            _grid = new BoardGrid(boardSize);
        }

        public BoardGrid Grid => _grid;
        public IReadOnlyList<PlacedBlock> PlacedBlocks => _placedBlocks;

        /// <summary>
        /// 블록 ID 발급 + 목록 등록 + 격자 점유를 한 번에 처리한다.
        /// </summary>
        public int AddPlacedBlock(string shapeId, Vector2Int pivot, IReadOnlyList<Vector2Int> cellOffsets)
        {
            int blockId = _nextBlockId++;
            var placedBlock = new PlacedBlock(blockId, shapeId, pivot, cellOffsets);
            _placedBlocks.Add(placedBlock);
            OccupyBlockCells(placedBlock);
            return blockId;
        }

        public bool TryGetPlacedBlock(int blockId, out PlacedBlock placedBlock)
        {
            for (int i = 0; i < _placedBlocks.Count; i++)
            {
                if (_placedBlocks[i].BlockId == blockId)
                {
                    placedBlock = _placedBlocks[i];
                    return true;
                }
            }

            placedBlock = null;
            return false;
        }

        /// <summary>
        /// 제거 대상 칸을 격자·PlacedBlock에서 해제한다. 내부에 남는 칸이 있으면 offsets만 갱신한다.
        /// </summary>
        public void RemoveCells(IReadOnlyCollection<Vector2Int> cellsToRemove)
        {
            if (cellsToRemove == null || cellsToRemove.Count == 0)
            {
                return;
            }

            var removeSet = cellsToRemove as HashSet<Vector2Int> ?? new HashSet<Vector2Int>(cellsToRemove);

            foreach (Vector2Int cell in removeSet)
            {
                _grid.SetOccupied(cell, false);
            }

            for (int i = _placedBlocks.Count - 1; i >= 0; i--)
            {
                PlacedBlock block = _placedBlocks[i];
                var remainingAbsolute = new List<Vector2Int>();

                foreach (Vector2Int absoluteCell in block.AbsoluteCells())
                {
                    if (!removeSet.Contains(absoluteCell))
                    {
                        remainingAbsolute.Add(absoluteCell);
                    }
                }

                if (remainingAbsolute.Count == 0)
                {
                    _placedBlocks.RemoveAt(i);
                    continue;
                }

                Vector2Int newPivot = remainingAbsolute[0];
                var newOffsets = new List<Vector2Int>(remainingAbsolute.Count);
                for (int j = 0; j < remainingAbsolute.Count; j++)
                {
                    newOffsets.Add(remainingAbsolute[j] - newPivot);
                }

                block.SetPlacement(newPivot, newOffsets);
            }
        }

        public void RotateAllClockwise90()
        {
            for (int i = 0; i < _placedBlocks.Count; i++)
            {
                PlacedBlock block = _placedBlocks[i];
                Vector2Int rotatedPivot = GridRotation.RotateClockwise90(block.Pivot);
                var rotatedOffsets = new List<Vector2Int>(block.CellOffsets.Count);

                for (int j = 0; j < block.CellOffsets.Count; j++)
                {
                    rotatedOffsets.Add(GridRotation.RotateClockwise90(block.CellOffsets[j]));
                }

                block.SetPlacement(rotatedPivot, rotatedOffsets);
            }

            RebuildOccupancyFromPlacedBlocks();
        }

        private void RebuildOccupancyFromPlacedBlocks()
        {
            _grid.ClearOccupancy();

            for (int i = 0; i < _placedBlocks.Count; i++)
            {
                OccupyBlockCells(_placedBlocks[i]);
            }
        }

        private void OccupyBlockCells(PlacedBlock block)
        {
            foreach (Vector2Int absoluteCell in block.AbsoluteCells())
            {
                _grid.SetOccupied(absoluteCell, true);
            }
        }
    }
}
