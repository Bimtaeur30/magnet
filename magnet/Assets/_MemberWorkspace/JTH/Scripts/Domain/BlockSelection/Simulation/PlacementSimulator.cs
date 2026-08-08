using System.Collections.Generic;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Clear;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Simulation
{
    public static class PlacementSimulator
    {
        /// <summary>
        /// grid와 cellOffsets, pivot을 받아 피스를 올리고, 완성된 라인을 지운다. 지운 라인 개수를 반환한다.
        /// </summary>
        public static int PlaceAndClear(BoardGrid grid, IReadOnlyList<Vector2Int> cellOffsets, Vector2Int pivot)
        {
            List<Vector2Int> placedCells = new(cellOffsets.Count);
            for (int i = 0; i < cellOffsets.Count; ++i)
            {
                Vector2Int cell = pivot + cellOffsets[i];
                grid.SetOccupied(cell, true);
                placedCells.Add(cell);
            }

            ClearedLineResult result = LineClearDetector.Detect(grid, placedCells);
            foreach (Vector2Int cell in result.CollectClearedCells(grid.BoardSize))
            {
                grid.SetOccupied(cell, false);
            }

            return result.ClearedLineCount;
        }
    }
}
