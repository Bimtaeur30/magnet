using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    public sealed class CellRelocation
    {
        public CellRelocation(int cellId, Vector2Int from, Vector2Int to)
        {
            CellId = cellId;
            From = from;
            To = to;
        }

        public int CellId { get; }
        public Vector2Int From { get; }
        public Vector2Int To { get; }
    }

    public sealed class ClearWave
    {
        public ClearWave(
            int squareSize,
            IReadOnlyList<Vector2Int> destroyedCells,
            IReadOnlyList<int> destroyedCellIds,
            IReadOnlyList<CellRelocation> relocations)
        {
            SquareSize = squareSize;
            DestroyedCells = destroyedCells;
            DestroyedCellIds = destroyedCellIds;
            Relocations = relocations;
        }

        public int SquareSize { get; }
        public IReadOnlyList<Vector2Int> DestroyedCells { get; }
        public IReadOnlyList<int> DestroyedCellIds { get; }
        public IReadOnlyList<CellRelocation> Relocations { get; }
        public int ScoreCells => DestroyedCells.Count;
    }

    public sealed class ClearReassemblyResult
    {
        public ClearReassemblyResult(IReadOnlyList<ClearWave> waves, bool hasCellsOutsideBounds)
        {
            Waves = waves;
            HasCellsOutsideBounds = hasCellsOutsideBounds;
        }

        public IReadOnlyList<ClearWave> Waves { get; }
        public bool HasCellsOutsideBounds { get; }
        public bool HasAnyWave => Waves != null && Waves.Count > 0;

        public static ClearReassemblyResult None { get; } = new(
            System.Array.Empty<ClearWave>(),
            hasCellsOutsideBounds: false);
    }
}
