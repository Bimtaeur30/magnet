using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    public sealed class ClearDetectionResult
    {
        private static readonly ClearDetectionResult Empty = new(
            System.Array.Empty<ClearedSquareInfo>(),
            new HashSet<Vector2Int>());

        public ClearDetectionResult(
            IReadOnlyList<ClearedSquareInfo> clearedSquares,
            IReadOnlyCollection<Vector2Int> cellsToRemove)
        {
            ClearedSquares = clearedSquares;
            CellsToRemove = cellsToRemove;
        }

        public IReadOnlyList<ClearedSquareInfo> ClearedSquares { get; }
        public IReadOnlyCollection<Vector2Int> CellsToRemove { get; }

        public bool HasAnyClear => CellsToRemove.Count > 0;

        public static ClearDetectionResult None => Empty;
    }
}
