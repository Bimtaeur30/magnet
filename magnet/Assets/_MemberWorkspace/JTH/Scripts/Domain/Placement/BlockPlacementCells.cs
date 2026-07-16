using System.Collections.Generic;
using Magnet.Contracts.BlockShapes;
using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    internal static class BlockPlacementCells
    {
        public static List<Vector2Int> ToAbsolute(IBlockShape shape, Vector2Int pivot)
        {
            var cells = new List<Vector2Int>(shape.CellOffsets.Count);
            foreach (Vector2Int offset in shape.CellOffsets)
            {
                cells.Add(pivot + offset);
            }

            return cells;
        }

        /// <summary>
        /// 자석 칸 또는 이미 점유된 칸과 겹치면 실패 사유를 반환한다.
        /// MagnetSnapSimulator.CanStep 과 동일한 겹침 규칙을 쓴다.
        /// </summary>
        public static PlacementFailureReason GetOverlapReason(IBlockShape shape, Vector2Int pivot, BoardGrid grid)
        {
            foreach (Vector2Int offset in shape.CellOffsets)
            {
                Vector2Int cell = pivot + offset;

                if (BoardCoordinates.IsMagnetCell(cell.x, cell.y))
                {
                    return PlacementFailureReason.OverlapsMagnet;
                }

                if (grid.IsOccupied(cell))
                {
                    return PlacementFailureReason.OverlapsOccupied;
                }
            }

            return PlacementFailureReason.None;
        }

        public static bool HasOverlap(IBlockShape shape, Vector2Int pivot, BoardGrid grid)
        {
            return GetOverlapReason(shape, pivot, grid) != PlacementFailureReason.None;
        }

        public static bool HasAnyCellOutsideBounds(BoardGrid grid)
        {
            IReadOnlyCollection<Vector2Int> cells = grid.OccupiedCells;
            
            foreach (Vector2Int cell in cells)
            {
                if (!grid.IsInBounds(cell.x, cell.y))
                {
                    return true;
                }
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

        /// <summary>형태 칸 중 가장 큰 offset.y. 스테이징 피벗을 최상단 칸에 둘 때 사용.</summary>
        public static int GetMaxOffsetY(IReadOnlyList<Vector2Int> offsets)
        {
            if (offsets == null || offsets.Count == 0)
            {
                return 0;
            }

            int maxY = int.MinValue;
            for (int i = 0; i < offsets.Count; i++)
            {
                if (offsets[i].y > maxY)
                {
                    maxY = offsets[i].y;
                }
            }

            return maxY;
        }

        /// <summary>최상단 칸이 stagingGridY에 오도록 Domain pivot Y를 계산한다.</summary>
        public static int GetStagingPivotY(int stagingGridY, IReadOnlyList<Vector2Int> offsets)
        {
            return stagingGridY - GetMaxOffsetY(offsets);
        }

        public static void GetPivotXRange(IBlockShape shape, int boardSize, out int minPivotX, out int maxPivotX)
        {
            int half = BoardCoordinates.HalfExtent(boardSize);
            int minOffsetX = int.MaxValue;
            int maxOffsetX = int.MinValue;

            foreach (Vector2Int offset in shape.CellOffsets)
            {
                if (offset.x < minOffsetX)
                {
                    minOffsetX = offset.x;
                }

                if (offset.x > maxOffsetX)
                {
                    maxOffsetX = offset.x;
                }
            }

            minPivotX = -half - minOffsetX;
            maxPivotX = half - maxOffsetX;
        }

        public static int WorldCenterXToPivotX(float worldCenterX, float cellSize, float shapeCenterOffsetX, int minPivotX, int maxPivotX)
        {
            float centerGridX = worldCenterX / cellSize;
            int pivotX = Mathf.RoundToInt(centerGridX - shapeCenterOffsetX);
            return Mathf.Clamp(pivotX, minPivotX, maxPivotX);
        }

        public static float PivotXToWorldCenterX(int pivotX, float cellSize, float shapeCenterOffsetX)
        {
            return (pivotX + shapeCenterOffsetX) * cellSize;
        }

        public static void GetWorldCenterXRange(
            int minPivotX,
            int maxPivotX,
            float cellSize,
            float shapeCenterOffsetX,
            out float minWorldCenterX,
            out float maxWorldCenterX)
        {
            minWorldCenterX = PivotXToWorldCenterX(minPivotX, cellSize, shapeCenterOffsetX);
            maxWorldCenterX = PivotXToWorldCenterX(maxPivotX, cellSize, shapeCenterOffsetX);
        }
    }
}
