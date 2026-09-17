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

        public int CountOccupied()
        {
            int count = 0;
            for (int x = 0; x < BoardSize; ++x)
            {
                for (int y = 0; y < BoardSize; ++y)
                {
                    if (_cells[x, y])
                    {
                        ++count;
                    }
                }
            }

            return count;
        }

        /// <summary>보드에 남은 칸이 하나도 없는 상태(올클리어).</summary>
        public bool IsEmpty()
        {
            for (int x = 0; x < BoardSize; ++x)
            {
                for (int y = 0; y < BoardSize; ++y)
                {
                    if (_cells[x, y])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 8×8 이하일 때 보드를 64비트로 팩. 탐색 중복 상태 제거(메모이제이션)용.
        /// BoardSize가 8을 넘으면 false를 반환한다.
        /// </summary>
        public bool TryPackBits(out ulong bits)
        {
            bits = 0UL;
            if (BoardSize > 8)
            {
                return false;
            }

            int bit = 0;
            for (int x = 0; x < BoardSize; ++x)
            {
                for (int y = 0; y < BoardSize; ++y, ++bit)
                {
                    if (_cells[x, y])
                    {
                        bits |= 1UL << bit;
                    }
                }
            }

            return true;
        }
    }
}
