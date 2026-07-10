using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    public sealed class ClearedSquareInfo
    {
        public ClearedSquareInfo(int squareSize, bool isFullClear, IReadOnlyList<Vector2Int> clearedCells)
        {
            SquareSize = squareSize;
            IsFullClear = isFullClear;
            ClearedCells = clearedCells;
        }

        public int SquareSize { get; }
        public bool IsFullClear { get; }
        public IReadOnlyList<Vector2Int> ClearedCells { get; }
    }
}
