namespace JTH.Scripts.Domain.BlockSelection.Health
{
    public readonly struct BoardHealthResult
    {
        public float FillRate { get; }
        public int DeadZoneCount { get; }
        public int BigPieceSlots { get; }
        public float PlacementFreedom { get; }

        /// <summary>점유 칸의 직교(상하좌우) 연결 덩어리 수. 적을수록 모여 있음.</summary>
        public int ClusterCount { get; }

        /// <summary>가장 큰 덩어리의 칸 수. 클수록 좋음.</summary>
        public int LargestClusterSize { get; }

        public float Score { get; }
        public HealthZone Zone { get; }

        public BoardHealthResult(
            float fillRate,
            int deadZoneCount,
            int bigPieceSlots,
            float placementFreedom,
            int clusterCount,
            int largestClusterSize,
            float score,
            HealthZone zone)
        {
            FillRate = fillRate;
            DeadZoneCount = deadZoneCount;
            BigPieceSlots = bigPieceSlots;
            PlacementFreedom = placementFreedom;
            ClusterCount = clusterCount;
            LargestClusterSize = largestClusterSize;
            Score = score;
            Zone = zone;
        }
    }
}
