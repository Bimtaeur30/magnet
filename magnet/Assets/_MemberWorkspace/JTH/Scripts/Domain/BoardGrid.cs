using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain
{
    /// <summary>
    /// 보드 칸 점유 상태. 좌표는 자석=(0,0) 격자. 배열 대신 Dictionary.
    /// </summary>
    public sealed class BoardGrid
    {
        private readonly int _boardSize;
        private readonly Dictionary<Vector2Int, bool> _occupied = new();

        public BoardGrid(int boardSize)
        {
            _boardSize = boardSize;
        }

        public int BoardSize => _boardSize;

        public bool IsOccupied(Vector2Int grid)
        {
            return _occupied.TryGetValue(grid, out bool occupied) && occupied;
        }

        public void SetOccupied(Vector2Int grid, bool occupied)
        {
            if (occupied)
            {
                _occupied[grid] = true;
            }
            else
            {
                _occupied.Remove(grid);
            }
        }

        public bool IsInBounds(int gridX, int gridY)
        {
            return BoardCoordinates.IsInBounds(gridX, gridY, _boardSize);
        }

        public bool HasOccupiedCellOutsideBounds()
        {
            foreach (Vector2Int grid in _occupied.Keys)
            {
                if (!IsInBounds(grid.x, grid.y))
                {
                    return true;
                }
            }

            return false;
        }

        public IReadOnlyDictionary<Vector2Int, bool> OccupiedCells => _occupied;
    }
}
