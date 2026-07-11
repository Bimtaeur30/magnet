namespace JTH.Scripts.Domain.Clear
{
    public sealed class SquareClearService
    {
        public ClearDetectionResult Detect(BoardSession session)
        {
            return SquareClearDetector.Detect(session.Grid);
        }

        public ClearDetectionResult DetectAndApply(BoardSession session)
        {
            ClearDetectionResult detection = Detect(session);
            if (!detection.HasAnyClear)
            {
                return detection;
            }

            session.RemoveCells(detection.CellsToRemove);
            return detection;
        }
    }
}
