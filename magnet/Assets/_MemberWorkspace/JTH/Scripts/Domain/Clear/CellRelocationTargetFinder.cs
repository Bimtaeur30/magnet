using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    /// <summary>
    /// 원점(자석)→원래 칸 직선 복도(수선 반폭)의 빈칸으로만 이동한다.
    /// 선택: 수선거리 최소 → 안쪽로 가장 가까운 빈칸(축 투영 최대) → 시계.
    /// 중간 구멍을 건너뛰지 않는다. 후보 없으면 제자리.
    /// </summary>
    public static class CellRelocationTargetFinder
    {
        private const int OutsideSearchPadding = 8;
        private const float Epsilon = 0.0001f;

        /// <param name="corridorHalfWidth">
        /// 원점–원래칸 직선에서 수직으로 허용하는 반폭(격자 단위).
        /// </param>
        public static bool TryFind(
            BoardGrid grid,
            Vector2Int ejector,
            float corridorHalfWidth,
            out Vector2Int target)
        {
            target = default;
            float halfWidth = Mathf.Max(0.01f, corridorHalfWidth);

            if (!TryGetAxis(ejector, out Vector2 axisDir, out float maxDist))
            {
                return false;
            }

            return TryPickNearestInwardEmpty(
                grid,
                ejector,
                axisDir,
                maxDist,
                halfWidth,
                out target);
        }

        /// <summary>복도 안 이동 가능 빈칸 전부.</summary>
        public static void CollectCorridorCandidates(
            BoardGrid grid,
            Vector2Int ejector,
            float corridorHalfWidth,
            HashSet<Vector2Int> results)
        {
            float halfWidth = Mathf.Max(0.01f, corridorHalfWidth);
            if (!TryGetAxis(ejector, out Vector2 axisDir, out float maxDist))
            {
                return;
            }

            int half = BoardCoordinates.HalfExtent(grid.BoardSize);
            int search = half + OutsideSearchPadding;
            for (int x = -search; x <= search; x++)
            {
                for (int y = -search; y <= search; y++)
                {
                    if (!TryReadEmptySlot(
                            grid,
                            ejector,
                            axisDir,
                            maxDist,
                            halfWidth,
                            x,
                            y,
                            out Vector2Int candidate,
                            out _,
                            out _,
                            out _))
                    {
                        continue;
                    }

                    results.Add(candidate);
                }
            }
        }

        public static void CollectSequentialTargets(
            BoardGrid grid,
            Vector2Int ejector,
            float corridorHalfWidth,
            List<Vector2Int> results,
            int maxSteps = 64)
        {
            for (int step = 0; step < maxSteps; step++)
            {
                if (!TryFind(grid, ejector, corridorHalfWidth, out Vector2Int target)
                    || target == ejector)
                {
                    break;
                }

                results.Add(target);
                grid.SetOccupied(target, true);
            }
        }

        private static bool TryPickNearestInwardEmpty(
            BoardGrid grid,
            Vector2Int ejector,
            Vector2 axisDir,
            float maxDist,
            float halfWidth,
            out Vector2Int target)
        {
            target = default;
            bool found = false;
            float bestPerp = float.MaxValue;
            float bestAlong = float.MinValue;
            float bestClock = float.MaxValue;
            Vector2Int best = default;

            int half = BoardCoordinates.HalfExtent(grid.BoardSize);
            int search = half + OutsideSearchPadding;
            for (int x = -search; x <= search; x++)
            {
                for (int y = -search; y <= search; y++)
                {
                    if (!TryReadEmptySlot(
                            grid,
                            ejector,
                            axisDir,
                            maxDist,
                            halfWidth,
                            x,
                            y,
                            out Vector2Int candidate,
                            out float perp,
                            out float along,
                            out float clock))
                    {
                        continue;
                    }

                    // 1) 축에 더 가까운 수선 2) 안쪽이지만 가장 가까운 빈칸(투영 최대) 3) 시계
                    bool better = !found
                        || perp < bestPerp - Epsilon
                        || (Mathf.Abs(perp - bestPerp) <= Epsilon && along > bestAlong + Epsilon)
                        || (Mathf.Abs(perp - bestPerp) <= Epsilon
                            && Mathf.Abs(along - bestAlong) <= Epsilon
                            && clock < bestClock);

                    if (!better)
                    {
                        continue;
                    }

                    found = true;
                    best = candidate;
                    bestPerp = perp;
                    bestAlong = along;
                    bestClock = clock;
                }
            }

            if (!found)
            {
                return false;
            }

            target = best;
            return true;
        }

        private static bool TryGetAxis(Vector2Int ejector, out Vector2 axisDir, out float maxDist)
        {
            Vector2 ejectorPos = ejector;
            maxDist = ejectorPos.magnitude;
            if (maxDist < Epsilon)
            {
                axisDir = default;
                return false;
            }

            axisDir = ejectorPos / maxDist;
            return true;
        }

        private static float PerpendicularDistance(Vector2 point, Vector2 unitAxis)
        {
            return Mathf.Abs(point.x * unitAxis.y - point.y * unitAxis.x);
        }

        private static bool TryReadEmptySlot(
            BoardGrid grid,
            Vector2Int ejector,
            Vector2 axisDir,
            float maxDist,
            float halfWidth,
            int x,
            int y,
            out Vector2Int candidate,
            out float perp,
            out float along,
            out float clock)
        {
            candidate = default;
            perp = 0f;
            along = 0f;
            clock = 0f;

            if (BoardCoordinates.IsMagnetCell(x, y))
            {
                return false;
            }

            candidate = new Vector2Int(x, y);
            if (candidate == ejector)
            {
                return false;
            }

            if (grid.IsOccupied(candidate))
            {
                return false;
            }

            Vector2 candidatePos = candidate;
            along = Vector2.Dot(candidatePos, axisDir);
            if (along < Epsilon || along > maxDist + Epsilon)
            {
                return false;
            }

            perp = PerpendicularDistance(candidatePos, axisDir);
            if (perp > halfWidth + Epsilon)
            {
                return false;
            }

            clock = CellRelocationOrder.ClockAngle01(candidate);
            return true;
        }
    }
}
