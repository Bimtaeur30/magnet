using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    /// <summary>
    /// 자석 방향 부채꼴 안 빈칸 중 자석에 가장 가까운 칸을 고른다.
    /// 보존 구역(chebyshev &lt; preserveBelowChebyshev)은 후보에서 제외한다.
    /// </summary>
    public static class CellRelocationTargetFinder
    {
        private const float PrimaryHalfSectorDegrees = 45f;
        private const float FallbackHalfSectorDegrees = 90f;
        private const int OutsideSearchPadding = 8;

        /// <param name="preserveBelowChebyshev">
        /// 이 값보다 작은 Chebyshev 칸은 폭발 N 내부 보존 구역. 목표 후보에서 제외.
        /// </param>
        public static bool TryFind(
            BoardGrid grid,
            Vector2Int ejector,
            int preserveBelowChebyshev,
            out Vector2Int target)
        {
            if (TryFindInternal(
                    grid,
                    ejector,
                    preserveBelowChebyshev,
                    PrimaryHalfSectorDegrees,
                    requireCircle: true,
                    requireInward: true,
                    out target))
            {
                return true;
            }

            if (TryFindInternal(
                    grid,
                    ejector,
                    preserveBelowChebyshev,
                    PrimaryHalfSectorDegrees,
                    requireCircle: false,
                    requireInward: true,
                    out target))
            {
                return true;
            }

            if (TryFindInternal(
                    grid,
                    ejector,
                    preserveBelowChebyshev,
                    FallbackHalfSectorDegrees,
                    requireCircle: false,
                    requireInward: true,
                    out target))
            {
                return true;
            }

            if (TryFindInternal(
                    grid,
                    ejector,
                    preserveBelowChebyshev,
                    halfSectorDegrees: 180f,
                    requireCircle: false,
                    requireInward: true,
                    out target))
            {
                return true;
            }

            if (TryFindInternal(
                    grid,
                    ejector,
                    preserveBelowChebyshev,
                    PrimaryHalfSectorDegrees,
                    requireCircle: false,
                    requireInward: false,
                    out target))
            {
                return true;
            }

            if (TryFindInternal(
                    grid,
                    ejector,
                    preserveBelowChebyshev,
                    halfSectorDegrees: 180f,
                    requireCircle: false,
                    requireInward: false,
                    out target))
            {
                return true;
            }

            target = AllocateOutsidePark(grid, ejector, parkIndex: 0);
            return true;
        }

        private static bool TryFindInternal(
            BoardGrid grid,
            Vector2Int ejector,
            int preserveBelowChebyshev,
            float halfSectorDegrees,
            bool requireCircle,
            bool requireInward,
            out Vector2Int target)
        {
            target = default;
            Vector2 ejectorPos = ejector;
            Vector2 axis = Vector2.zero - ejectorPos;
            float axisLen = axis.magnitude;
            if (axisLen < 0.0001f)
            {
                return false;
            }

            Vector2 axisDir = axis / axisLen;
            float maxDist = axisLen;
            int ejectorRing = CellRelocationOrder.Chebyshev(ejector);

            bool found = false;
            int bestChebyshev = int.MaxValue;
            float bestEuclidean = float.MaxValue;
            float bestAxisAngle = float.MaxValue;
            float bestClock = float.MaxValue;
            bool bestInBounds = false;
            Vector2Int best = default;

            int half = BoardCoordinates.HalfExtent(grid.BoardSize);
            int search = half + OutsideSearchPadding;
            for (int x = -search; x <= search; x++)
            {
                for (int y = -search; y <= search; y++)
                {
                    if (BoardCoordinates.IsMagnetCell(x, y))
                    {
                        continue;
                    }

                    Vector2Int candidate = new(x, y);
                    if (candidate == ejector)
                    {
                        continue;
                    }

                    if (grid.IsOccupied(candidate))
                    {
                        continue;
                    }

                    int chebyshev = CellRelocationOrder.Chebyshev(candidate);

                    // 터진 N의 순수 내부는 보존 — 바깥 칸이 여기로 파고들지 않음
                    if (chebyshev < preserveBelowChebyshev)
                    {
                        continue;
                    }

                    if (requireInward && chebyshev >= ejectorRing)
                    {
                        continue;
                    }

                    Vector2 candidatePos = candidate;
                    Vector2 toCandidate = candidatePos - ejectorPos;
                    float dist = toCandidate.magnitude;
                    if (requireCircle && dist > maxDist + 0.0001f)
                    {
                        continue;
                    }

                    float axisAngle = dist < 0.0001f
                        ? 0f
                        : Vector2.Angle(axisDir, toCandidate / dist);
                    if (axisAngle > halfSectorDegrees + 0.0001f)
                    {
                        continue;
                    }

                    float euclidean = candidatePos.magnitude;
                    float clock = CellRelocationOrder.ClockAngle01(candidate);
                    bool inBounds = grid.IsInBounds(candidate.x, candidate.y);

                    bool better = !found
                        || (inBounds && !bestInBounds)
                        || (inBounds == bestInBounds && chebyshev < bestChebyshev)
                        || (inBounds == bestInBounds
                            && chebyshev == bestChebyshev
                            && euclidean < bestEuclidean - 0.0001f)
                        || (inBounds == bestInBounds
                            && chebyshev == bestChebyshev
                            && Mathf.Abs(euclidean - bestEuclidean) <= 0.0001f
                            && axisAngle < bestAxisAngle - 0.0001f)
                        || (inBounds == bestInBounds
                            && chebyshev == bestChebyshev
                            && Mathf.Abs(euclidean - bestEuclidean) <= 0.0001f
                            && Mathf.Abs(axisAngle - bestAxisAngle) <= 0.0001f
                            && clock < bestClock);

                    if (!better)
                    {
                        continue;
                    }

                    found = true;
                    best = candidate;
                    bestChebyshev = chebyshev;
                    bestEuclidean = euclidean;
                    bestAxisAngle = axisAngle;
                    bestClock = clock;
                    bestInBounds = inBounds;
                }
            }

            if (!found)
            {
                return false;
            }

            target = best;
            return true;
        }

        public static Vector2Int AllocateOutsidePark(BoardGrid grid, Vector2Int from, int parkIndex)
        {
            int half = BoardCoordinates.HalfExtent(grid.BoardSize);
            Vector2 dir = ((Vector2)from).sqrMagnitude > 0.0001f
                ? ((Vector2)from).normalized
                : Vector2.up;

            for (int step = 0; step < 32; step++)
            {
                float radius = half + 2 + parkIndex + step;
                Vector2Int candidate = new(
                    Mathf.RoundToInt(dir.x * radius),
                    Mathf.RoundToInt(dir.y * radius));

                if (BoardCoordinates.IsMagnetCell(candidate.x, candidate.y))
                {
                    continue;
                }

                if (grid.IsInBounds(candidate.x, candidate.y))
                {
                    continue;
                }

                if (!grid.IsOccupied(candidate))
                {
                    return candidate;
                }
            }

            return new Vector2Int(parkIndex, half + 2 + parkIndex);
        }
    }
}
