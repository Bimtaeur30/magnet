namespace JTH.Scripts.Domain.BlockSelection.Health
{
    public readonly struct BoardHealthResult
    {
        public float FillRate { get; }
        public int DeadZoneCount { get; }
        public int BigPieceSlots { get; }
        public float PlacementFreedom { get; }
        public float Score { get; }
        public HealthZone Zone { get; }

        public BoardHealthResult(
            float fillRate,
            int deadZoneCount,
            int bigPieceSlots,
            float placementFreedom,
            float score,
            HealthZone zone)
        {
            FillRate = fillRate;
            DeadZoneCount = deadZoneCount;
            BigPieceSlots = bigPieceSlots;
            PlacementFreedom = placementFreedom;
            Score = score;
            Zone = zone;
        }
    }
}
