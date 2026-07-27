using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Board
{
    public sealed class BoardGrid
    {
        public int BoardSize { get; private set; }

        private readonly HashSet<Vector2Int> _occupied;
        
        public BoardGrid(int boardSize)
        {
            BoardSize = boardSize;
            
            _occupied = new HashSet<Vector2Int>();
        }

        public bool IsOccupied(Vector2Int grid)
        {
            return _occupied.Contains(grid);
        }

        public void SetOccupied(Vector2Int grid, bool occupied)
        {
            if (occupied)
            {
                _occupied.Add(grid);
            }
            else
            {
                _occupied.Remove(grid);
            }
        }

        public bool IsInBounds(Vector2Int grid)
        {
            return grid.x >= 0 && grid.x < BoardSize && grid.y >= 0 && grid.y < BoardSize;
        }

        public bool HasOccupiedCellOutsideBounds()
        {
            foreach (Vector2Int grid in _occupied)
            {
                if (!IsInBounds(grid))
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}
