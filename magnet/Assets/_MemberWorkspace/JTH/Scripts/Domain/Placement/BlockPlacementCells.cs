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

        public static PlacementFailureReason GetOverlapReason(IReadOnlyList<Vector2Int> cells, BoardGrid grid)
        {
            foreach (Vector2Int cell in cells)
            {
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
