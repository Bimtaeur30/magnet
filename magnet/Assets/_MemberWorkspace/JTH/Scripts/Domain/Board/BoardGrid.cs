using UnityEngine;

namespace JTH.Scripts.Domain.Board
{
    public sealed class BoardGrid
    {
        public int BoardSize { get; private set; }

        private readonly bool[,] _cells;

        public BoardGrid(int boardSize)
        {
            BoardSize = boardSize;

            _cells = new bool[boardSize, boardSize];
        }

        private BoardGrid(int boardSize, bool[,] cells)
        {
            BoardSize = boardSize;

            _cells = cells;
        }

        public BoardGrid Clone()
        {
            return new BoardGrid(BoardSize, (bool[,])_cells.Clone());
        }

        public bool IsOccupied(Vector2Int grid)
        {
            return IsInBounds(grid) && _cells[grid.x, grid.y];
        }

        public void SetOccupied(Vector2Int grid, bool occupied)
        {
            _cells[grid.x, grid.y] = occupied;
        }

        public bool IsInBounds(Vector2Int grid)
        {
            return grid.x >= 0 && grid.x < BoardSize && grid.y >= 0 && grid.y < BoardSize;
        }
    }
}
