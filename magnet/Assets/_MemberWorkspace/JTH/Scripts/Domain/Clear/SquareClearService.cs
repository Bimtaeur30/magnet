using JTH.Scripts.Domain.Placement;

namespace JTH.Scripts.Domain.Clear
{
    /// <summary>
    /// 단일 Detect+테두리 제거. 재조립 연쇄는 <see cref="ClearReassemblyService"/> 사용.
    /// </summary>
    public sealed class SquareClearService
    {
        public ClearDetectionResult DetectAndApply(BoardSession session)
        {
            ClearDetectionResult detection = SquareClearDetector.Detect(session.Grid);
            session.RemoveCellsAt(detection.CellsToRemove);
            detection.HasCellsOutsideBounds = BlockPlacementCells.HasAnyCellOutsideBounds(session.Grid);
            return detection;
        }
    }
}
