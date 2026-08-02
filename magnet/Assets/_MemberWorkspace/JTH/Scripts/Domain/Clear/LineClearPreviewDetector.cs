using System.Collections.Generic;
using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    /// <summary>
    /// 실제 보드를 건드리지 않고, 피벗에 블록을 놓았을 때 클리어될 라인을 시뮬한다.
    /// </summary>
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
