using System;
using System.Collections.Generic;
using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    public static class PlacementService
    {
        public static bool CanPlace(IReadOnlyList<Vector2Int> cellOffsets, Vector2Int pivot, BoardGrid grid)
            => !GetOverlap(cellOffsets, pivot, grid) && !GetOutOfBoard(cellOffsets, pivot, grid);

        public static bool CanPlaceAnywhere(IReadOnlyList<Vector2Int> cellOffsets, BoardGrid grid)
        {
            Vector2Int pivot = Vector2Int.zero;

            for (int x = 0; x < grid.BoardSize; ++x)
            {
                for (int y = 0; y < grid.BoardSize; ++y)
                {
                    pivot.x = x;
                    pivot.y = y;

                    if (CanPlace(cellOffsets, pivot, grid))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool GetOverlap(IReadOnlyList<Vector2Int> cellOffsets, Vector2Int pivot, BoardGrid grid)
            => AnyMatch(cellOffsets, pivot, grid.IsOccupied);

        public static bool GetOutOfBoard(IReadOnlyList<Vector2Int> cellOffsets, Vector2Int pivot, BoardGrid grid)
            => AnyMatch(cellOffsets, pivot, cell => !grid.IsInBounds(cell));

        private static bool AnyMatch(
            IReadOnlyList<Vector2Int> cellOffsets,
            Vector2Int pivot,
            Func<Vector2Int, bool> predicate)
        {
            foreach (Vector2Int offset in cellOffsets)
            {
                if (predicate(pivot + offset))
                    return true;
            }
            return false;
        }

        public static Vector2 GetShapeCenterOffset(IReadOnlyList<Vector2Int> offsets)
        {
            long sumX = 0;
            long sumY = 0;
            for (int i = 0; i < offsets.Count; i++)
            {
                sumX += offsets[i].x;
                sumY += offsets[i].y;
            }
        
            float count = offsets.Count;
            return new Vector2(sumX / count, sumY / count);
        }
        
        public static bool TryGetBoardPivot(Vector2 pivot, IReadOnlyList<Vector2Int> cellOffsets
            , BoardGrid grid, Vector2Int? lastBoardPivot, float lastPivotSnapThreshold, out Vector2Int boardPivot)
        {
            //가장 처음에 단순히 round 선택
            Vector2Int rounded = new(Mathf.RoundToInt(pivot.x), Mathf.RoundToInt(pivot.y));
            if (CanPlace(cellOffsets, rounded, grid))
            {
                boardPivot = rounded;
                return true;
            }
            
            Span<Vector2Int> candidates = stackalloc Vector2Int[4];
            candidates[0] = new Vector2Int(Mathf.FloorToInt(pivot.x), Mathf.FloorToInt(pivot.y));
            candidates[1] = new Vector2Int(Mathf.CeilToInt(pivot.x), Mathf.CeilToInt(pivot.y));
            candidates[2] = new Vector2Int(Mathf.CeilToInt(pivot.x), Mathf.FloorToInt(pivot.y));
            candidates[3] = new Vector2Int(Mathf.FloorToInt(pivot.x), Mathf.CeilToInt(pivot.y));
            
            //올림과 내림으로 가장 가까운 피벗 선택
            Vector2Int best = default;
            float bestDist = float.MaxValue;
            bool found = false;
            for (int i = 0; i < 4; i++)
            {
                Vector2Int c = candidates[i];
                if (c == rounded)
                    continue;
                if (!CanPlace(cellOffsets, c, grid))
                    continue;
                float dist = Vector2.Distance(pivot, c);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = c;
                    found = true;
                }
            }
            if (found)
            {
                boardPivot = best;
                return true;
            }

            //마지막까지 안되면 직전 피벗이 일정 거리 이상 떨어지지 않았는 가에 직전 피벗 선택
            if (lastBoardPivot is { } last)
            {
                float xDistance = Mathf.Abs(last.x - pivot.x);
                float yDistance = Mathf.Abs(last.y - pivot.y);

                if (xDistance < lastPivotSnapThreshold
                    && yDistance < lastPivotSnapThreshold
                    && CanPlace(cellOffsets, last, grid))
                {
                    boardPivot = last;
                    return true;
                }
            }

            //다 안되면 false
            boardPivot = default;
            return false;
        }
    }
}
