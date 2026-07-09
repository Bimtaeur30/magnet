using System.Collections.Generic;
using JTH.Scripts.Domain.Placement;
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
            _placedBlocks.Add(new PlacedBlock(blockId, shapeId, pivot, new List<Vector2Int>(cellOffsets)));

            foreach (Vector2Int offset in cellOffsets)
            {
                _grid.SetOccupied(pivot + offset, true);
            }

            return blockId;
        }
    }
}
