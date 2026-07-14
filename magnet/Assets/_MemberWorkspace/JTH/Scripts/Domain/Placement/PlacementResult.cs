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
        public IReadOnlyList<int> CellIds { get; }

        private PlacementResult(
            bool success,
            PlacementFailureReason failureReason,
            int blockId,
            Vector2Int finalPivot,
            IReadOnlyList<Vector2Int> cellPositions,
            IReadOnlyList<int> cellIds)
        {
            Success = success;
            FailureReason = failureReason;
            BlockId = blockId;
            FinalPivot = finalPivot;
            CellPositions = cellPositions;
            CellIds = cellIds;
        }

        public static PlacementResult Succeeded(
            Vector2Int finalPivot,
            IReadOnlyList<Vector2Int> cellPositions,
            int blockId = 0,
            IReadOnlyList<int> cellIds = null)
        {
            return new PlacementResult(
                true,
                PlacementFailureReason.None,
                blockId,
                finalPivot,
                cellPositions,
                cellIds ?? System.Array.Empty<int>());
        }

        public static PlacementResult Failed(PlacementFailureReason reason)
        {
            return new PlacementResult(
                false,
                reason,
                0,
                default,
                System.Array.Empty<Vector2Int>(),
                System.Array.Empty<int>());
        }

        public PlacementResult WithBlockId(int blockId)
        {
            return new PlacementResult(
                Success,
                FailureReason,
                blockId,
                FinalPivot,
                CellPositions,
                CellIds);
        }

        public PlacementResult WithCellIds(IReadOnlyList<int> cellIds)
        {
            int blockId = cellIds != null && cellIds.Count > 0 ? cellIds[0] : BlockId;
            return new PlacementResult(
                Success,
                FailureReason,
                blockId,
                FinalPivot,
                CellPositions,
                cellIds ?? System.Array.Empty<int>());
        }
    }
}
