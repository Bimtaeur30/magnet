using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    public sealed class PlacementResult
    {
        public bool Success { get; }
        public PlacementFailureReason FailureReason { get; }
        public int BlockId { get; }
        public Vector2Int FinalPivot { get; }
        public IReadOnlyList<Vector2Int> CellPositions { get; }
        public bool HasCellsOutsideBounds { get; }

        private PlacementResult(
            bool success,
            PlacementFailureReason failureReason,
            int blockId,
            Vector2Int finalPivot,
            IReadOnlyList<Vector2Int> cellPositions,
            bool hasCellsOutsideBounds)
        {
            Success = success;
            FailureReason = failureReason;
            BlockId = blockId;
            FinalPivot = finalPivot;
            CellPositions = cellPositions;
            HasCellsOutsideBounds = hasCellsOutsideBounds;
        }

        public static PlacementResult Succeeded(
            Vector2Int finalPivot,
            IReadOnlyList<Vector2Int> cellPositions,
            bool hasCellsOutsideBounds,
            int blockId = 0)
        {
            return new PlacementResult(
                true,
                PlacementFailureReason.None,
                blockId,
                finalPivot,
                cellPositions,
                hasCellsOutsideBounds);
        }

        public static PlacementResult Failed(PlacementFailureReason reason)
        {
            return new PlacementResult(false, reason, 0, default, System.Array.Empty<Vector2Int>(), false);
        }

        public PlacementResult WithBlockId(int blockId)
        {
            return new PlacementResult(
                Success,
                FailureReason,
                blockId,
                FinalPivot,
                CellPositions,
                HasCellsOutsideBounds);
        }
    }
}
