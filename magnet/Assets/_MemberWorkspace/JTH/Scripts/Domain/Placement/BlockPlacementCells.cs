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

        public static bool HasAnyCellOutsideBounds(IReadOnlyList<Vector2Int> cells, BoardGrid grid)
        {
            foreach (Vector2Int cell in cells)
            {
                if (!grid.IsInBounds(cell.x, cell.y))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
