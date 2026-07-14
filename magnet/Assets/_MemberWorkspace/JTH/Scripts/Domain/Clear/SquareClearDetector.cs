using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    /// <summary>
    /// 자석 (0,0) 중심 홀수 N×N 테두리 클리어 판정.
    /// 완성된 N 중 <b>가장 안쪽만</b> 선택하고, 파괴 칸은 <b>테두리만</b>이다.
    /// </summary>
    public static class SquareClearDetector
    {
        public static bool TryDetectInnermost(BoardGrid grid, out ClearedSquareInfo squareInfo)
        {
            squareInfo = null;
            int half = BoardCoordinates.HalfExtent(grid.BoardSize);

            for (int squareHalf = 1; squareHalf <= half; squareHalf++)
            {
                if (!TryEvaluateBorder(grid, squareHalf, out squareInfo))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        /// <summary>레거시 호환: 최내곽 1개만 CellsToRemove에 담아 반환.</summary>
        public static ClearDetectionResult Detect(BoardGrid grid)
        {
            if (!TryDetectInnermost(grid, out ClearedSquareInfo squareInfo))
            {
                return ClearDetectionResult.None;
            }

            return new ClearDetectionResult(
                new List<ClearedSquareInfo> { squareInfo },
                new HashSet<Vector2Int>(squareInfo.ClearedCells));
        }

        private static bool TryEvaluateBorder(BoardGrid grid, int squareHalf, out ClearedSquareInfo squareInfo)
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

                    if (ChebyshevFromMagnet(x, y) == squareHalf)
                    {
                        borderCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (!AreAllOccupied(grid, borderCells))
            {
                return false;
            }

            int squareSize = squareHalf * 2 + 1;
            squareInfo = new ClearedSquareInfo(squareSize, borderCells);
            return true;
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
    }
}
