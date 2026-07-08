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

            int blockId = _session.AllocateBlockId();
            ApplyPlacement(shape, result.FinalPivot, blockId);
            return PlacementResult.Succeeded(result.FinalPivot, result.CellPositions, result.HasCellsOutsideBounds);
        }

        private void ApplyPlacement(IBlockShape shape, Vector2Int finalPivot, int blockId)
        {
            BoardGrid grid = _session.Grid;
            List<Vector2Int> finalCells = BlockPlacementCells.ToAbsolute(shape, finalPivot);

            foreach (Vector2Int cell in finalCells)
            {
                grid.SetOccupied(cell, true);
            }

            _session.RegisterPlacedBlock(new PlacedBlock(
                blockId,
                shape.ShapeId,
                finalPivot,
                new List<Vector2Int>(shape.CellOffsets)));
        }

        private PlacementResult BuildResult(IBlockShape shape, Vector2Int startPivot)
        {
            BoardGrid grid = _session.Grid;
            List<Vector2Int> startCells = BlockPlacementCells.ToAbsolute(shape, startPivot);

            PlacementFailureReason startOverlap = BlockPlacementCells.GetOverlapReason(startCells, grid);
            if (startOverlap != PlacementFailureReason.None)
            {
                return PlacementResult.Failed(startOverlap);
            }

            Vector2Int finalPivot = _snapSimulator.Snap(shape, startPivot, grid);
            List<Vector2Int> finalCells = BlockPlacementCells.ToAbsolute(shape, finalPivot);

            PlacementFailureReason finalOverlap = BlockPlacementCells.GetOverlapReason(finalCells, grid);
            if (finalOverlap != PlacementFailureReason.None)
            {
                return PlacementResult.Failed(finalOverlap);
            }

            bool hasCellsOutsideBounds = BlockPlacementCells.HasAnyCellOutsideBounds(finalCells, grid);
            return PlacementResult.Succeeded(finalPivot, finalCells, hasCellsOutsideBounds);
        }
    }
}
