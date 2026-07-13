using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    public sealed class ClearedSquareInfo
    {
        public ClearedSquareInfo(int squareSize, IReadOnlyList<Vector2Int> clearedCells)
        {
            SquareSize = squareSize;
            ClearedCells = clearedCells;
        }

        public int SquareSize { get; }
        public IReadOnlyList<Vector2Int> ClearedCells { get; }
    }
}
