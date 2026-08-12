using System.Collections.Generic;
using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    public static class LineClearPreviewDetector
    {
        public static ClearedLineResult Detect(
            BoardGrid sourceGrid,
            IReadOnlyList<Vector2Int> cellOffsets,
            Vector2Int pivot)
        {
            BoardGrid simulated = sourceGrid.Clone();
            List<Vector2Int> placedCells = new List<Vector2Int>(cellOffsets.Count);

            for (int i = 0; i < cellOffsets.Count; ++i)
            {
                Vector2Int cell = pivot + cellOffsets[i];
                simulated.SetOccupied(cell, true);
                placedCells.Add(cell);
            }

            return LineClearDetector.Detect(simulated, placedCells);
        }
    }
}
