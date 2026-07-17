using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    /// <summary>
    /// N×N 테두리 링이 「막혀 완성 불가」인지 판정한다.
    /// 빈칸의 상·좌·우(하=중앙)가 모두 점유면 그 빈칸은 막힘.
    /// 링에 빈칸이 있고 그중 하나라도 막히면 링 비활성.
    /// </summary>
    public static class BlockedRingDetector
    {
        /// <summary>
        /// 비활성 링의 한 변 길이 N(3,5,7…)을 오름차순으로 반환한다.
        /// </summary>
        public static IReadOnlyList<int> DetectInactiveSquareSizes(BoardGrid grid)
        {
            var inactive = new List<int>();
            if (grid == null)
            {
                return inactive;
            }

            int half = BoardCoordinates.HalfExtent(grid.BoardSize);
            for (int squareHalf = 1; squareHalf <= half; squareHalf++)
            {
                if (IsRingInactive(grid, squareHalf))
                {
                    inactive.Add(squareHalf * 2 + 1);
                }
            }

            return inactive;
        }

        private static bool IsRingInactive(BoardGrid grid, int squareHalf)
        {
            for (int x = -squareHalf; x <= squareHalf; x++)
            {
                for (int y = -squareHalf; y <= squareHalf; y++)
                {
                    if (BoardCoordinates.IsMagnetCell(x, y))
                    {
                        continue;
                    }

                    if (ChebyshevFromMagnet(x, y) != squareHalf)
                    {
                        continue;
                    }

                    var cell = new Vector2Int(x, y);
                    if (grid.IsOccupied(cell))
                    {
                        continue;
                    }

                    if (IsEmptyCellBlocked(grid, cell))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 빈칸 기준 상·좌·우 인접이 모두 점유면 막힘. 하(중앙 방향)는 보지 않는다.
        /// </summary>
        private static bool IsEmptyCellBlocked(BoardGrid grid, Vector2Int cell)
        {
            GetLocalDirs(cell, out _, out Vector2Int outward, out Vector2Int sideA, out Vector2Int sideB);
            return grid.IsOccupied(cell + outward)
                && grid.IsOccupied(cell + sideA)
                && grid.IsOccupied(cell + sideB);
        }

        /// <summary>
        /// 하=중앙, 상=바깥, 좌·우=측면(모서리는 테두리 이웃 두 방향).
        /// </summary>
        private static void GetLocalDirs(
            Vector2Int cell,
            out Vector2Int towardCenter,
            out Vector2Int outward,
            out Vector2Int sideA,
            out Vector2Int sideB)
        {
            int absX = Mathf.Abs(cell.x);
            int absY = Mathf.Abs(cell.y);

            if (absX == absY)
            {
                int signX = cell.x > 0 ? 1 : -1;
                int signY = cell.y > 0 ? 1 : -1;
                towardCenter = new Vector2Int(-signX, -signY);
                outward = new Vector2Int(signX, signY);
                sideA = new Vector2Int(-signX, 0);
                sideB = new Vector2Int(0, -signY);
                return;
            }

            if (absY > absX)
            {
                int signY = cell.y > 0 ? 1 : -1;
                towardCenter = new Vector2Int(0, -signY);
                outward = new Vector2Int(0, signY);
                sideA = new Vector2Int(-1, 0);
                sideB = new Vector2Int(1, 0);
                return;
            }

            int signXEdge = cell.x > 0 ? 1 : -1;
            towardCenter = new Vector2Int(-signXEdge, 0);
            outward = new Vector2Int(signXEdge, 0);
            sideA = new Vector2Int(0, -1);
            sideB = new Vector2Int(0, 1);
        }

        private static int ChebyshevFromMagnet(int x, int y)
        {
            return Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
        }
    }
}
