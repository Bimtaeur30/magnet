using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    public sealed class ClearedLineResult
    {
        public ClearedLineResult(IReadOnlyList<Line> clearedLines)
        {
            ClearedLines = clearedLines;
        }

        public int ClearedLineCount => ClearedLines.Count;
        public IReadOnlyList<Line> ClearedLines { get; }

        public List<Vector2Int> CollectClearedCells(int boardSize)
        {
            List<Vector2Int> clearedCells = new List<Vector2Int>();

            for (int i = 0; i < ClearedLines.Count; ++i)
            {
                clearedCells.AddRange(ClearedLines[i].GetCells(boardSize));
            }

            return clearedCells;
        }
    }
}
