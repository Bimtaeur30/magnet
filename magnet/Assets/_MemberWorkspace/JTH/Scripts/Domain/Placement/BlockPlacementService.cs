using System.Collections.Generic;
using Magnet.Contracts.BlockShapes;
using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    public sealed class BlockPlacementService
    {
        private readonly BoardSession _session;
        private readonly MagnetSnapSimulator _snapSimulator;

        public BlockPlacementService(BoardSession session, MagnetSnapSimulator snapSimulator = null)
        {
            _session = session;
            _snapSimulator = snapSimulator ?? new MagnetSnapSimulator();
        }

        public BoardSession Session => _session;

        public PlacementResult Simulate(IBlockShape shape, Vector2Int startPivot)
        {
            return BuildResult(shape, startPivot);
        }

        public PlacementResult TryPlace(IBlockShape shape, Vector2Int startPivot)
        {
            PlacementResult result = BuildResult(shape, startPivot);
            if (!result.Success)
            {
                return result;
            }

            int blockId = _session.AddPlacedBlock(shape.ShapeId, result.FinalPivot, shape.CellOffsets);
            return result.WithBlockId(blockId);
        }

        private PlacementResult BuildResult(IBlockShape shape, Vector2Int startPivot)
        {
            BoardGrid grid = _session.Grid;

            PlacementFailureReason startOverlap = BlockPlacementCells.GetOverlapReason(shape, startPivot, grid);
            if (startOverlap != PlacementFailureReason.None)
            {
                return PlacementResult.Failed(startOverlap);
            }

            if (!_snapSimulator.TrySnap(shape, startPivot, grid, out Vector2Int finalPivot))
            {
                return PlacementResult.Failed(PlacementFailureReason.NoSnapTarget);
            }

            List<Vector2Int> finalCells = BlockPlacementCells.ToAbsolute(shape, finalPivot);
            return PlacementResult.Succeeded(finalPivot, finalCells);
        }
    }
}
