using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    public sealed class ClearDetectionResult
    {
        public ClearDetectionResult(
            IReadOnlyList<ClearedSquareInfo> clearedSquares,
            IReadOnlyCollection<Vector2Int> cellsToRemove)
        {
            ClearedSquares = clearedSquares;
            CellsToRemove = cellsToRemove;
        }

        public IReadOnlyList<ClearedSquareInfo> ClearedSquares { get; }
        public IReadOnlyCollection<Vector2Int> CellsToRemove { get; }

        public bool HasCellsOutsideBounds { get; set; }
        
        public static ClearDetectionResult None { get; } = new(
            System.Array.Empty<ClearedSquareInfo>(),
            new HashSet<Vector2Int>());
    }
}
