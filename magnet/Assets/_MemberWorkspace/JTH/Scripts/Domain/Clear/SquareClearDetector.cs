using System.Collections.Generic;
using JTH.Scripts.Domain.Placement;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    /// <summary>
    /// 자석 (0,0) 중심 홀수 N×N 테두리 클리어 판정.
    /// 클리어 시: 테두리 칸 + N×N <b>바깥</b> 점유 칸 제거. N×N <b>내부</b> 칸은 유지.
    /// </summary>
    public static class SquareClearDetector
    {
        public static ClearDetectionResult Detect(BoardGrid grid)
        {
            var clearedSquares = new List<ClearedSquareInfo>();
            var cellsToRemove = new HashSet<Vector2Int>();
            int half = BoardCoordinates.HalfExtent(grid.BoardSize);

            for (int size = 2; size <= grid.BoardSize; size++)
            {
                int squareHalf = size / 2;
                if (squareHalf > half)
                {
                    break;
                }

                if (!TryEvaluateSquare(grid, squareHalf, out ClearedSquareInfo squareInfo))
                {
                    continue;
                }

                // 이미 더 작은 사각형이 가져간 칸은 이후 ClearedCells에 넣지 않는다.
                var uniqueCells = new List<Vector2Int>();
                foreach (Vector2Int cell in squareInfo.ClearedCells)
                {
                    if (cellsToRemove.Add(cell))
                    {
                        uniqueCells.Add(cell);
                    }
                }

                if (uniqueCells.Count == 0)
                {
                    continue;
                }

                clearedSquares.Add(new ClearedSquareInfo(squareInfo.SquareSize, uniqueCells));
            }

            if (clearedSquares.Count == 0)
            {
                return ClearDetectionResult.None;
            }
            
            return new ClearDetectionResult(clearedSquares, cellsToRemove);
        }

        private static bool TryEvaluateSquare(BoardGrid grid, int squareHalf, out ClearedSquareInfo squareInfo)
        {
            squareInfo = null;
            var borderCells = new List<Vector2Int>();

            for (int x = -squareHalf; x <= squareHalf; x++)
            {
                for (int y = -squareHalf; y <= squareHalf; y++)
                {
                    if (BoardCoordinates.IsMagnetCell(x, y))
                    {
                        continue;
                    }

                    int chebyshev = ChebyshevFromMagnet(x, y);
                    Vector2Int cell = new(x, y);

                    if (chebyshev == squareHalf)
                        borderCells.Add(cell);
                }
            }

            if (!AreAllOccupied(grid, borderCells))
            {
                return false;
            }

            int squareSize = squareHalf * 2 + 1;
            IReadOnlyList<Vector2Int> clearedCells = BuildClearedCells(grid, squareHalf, borderCells);

            squareInfo = new ClearedSquareInfo(squareSize, clearedCells);
            return true;
        }

        /// <summary>테두리 + N×N 바깥 점유 칸. 내부(chebyshev &lt; squareHalf)는 포함하지 않는다.</summary>
        private static List<Vector2Int> BuildClearedCells(BoardGrid grid, int squareHalf, IReadOnlyList<Vector2Int> borderCells)
        {
            var clearedCells = new HashSet<Vector2Int>(borderCells);

            foreach (Vector2Int occupied in grid.OccupiedCells)
            {
                if (ChebyshevFromMagnet(occupied.x, occupied.y) > squareHalf)
                {
                    clearedCells.Add(occupied);
                }
            }

            return new List<Vector2Int>(clearedCells);
        }

        private static bool AreAllOccupied(BoardGrid grid, IReadOnlyList<Vector2Int> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (!grid.IsOccupied(cells[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static int ChebyshevFromMagnet(int x, int y)
        {
            return Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
        }

        private static int ChebyshevFromMagnet(Vector2Int cell)
        {
            return ChebyshevFromMagnet(cell.x, cell.y);
        }
    }
}
