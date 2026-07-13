using JTH.Scripts.Domain.Placement;

namespace JTH.Scripts.Domain.Clear
{
    public sealed class SquareClearService
    {
        public ClearDetectionResult DetectAndApply(BoardSession session)
        {
            ClearDetectionResult detection = SquareClearDetector.Detect(session.Grid);

            session.RemoveCells(detection.CellsToRemove);
            detection.HasCellsOutsideBounds = BlockPlacementCells.HasAnyCellOutsideBounds(session.Grid);
            return detection;
        }
    }
}
