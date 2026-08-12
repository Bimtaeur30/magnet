using System.Collections.Generic;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    public readonly struct AreaScoreResult
    {
        public AreaScoreResult(
            float total,
            IReadOnlyList<AreaComponentScore> components,
            int cornerRectArea,
            float baseArea,
            float cornerRectPenalty,
            int areaCount,
            float areaCountPenalty)
        {
            Total = total;
            Components = components;
            CornerRectArea = cornerRectArea;
            BaseArea = baseArea;
            CornerRectPenalty = cornerRectPenalty;
            AreaCount = areaCount;
            AreaCountPenalty = areaCountPenalty;
        }

        public float Total { get; }
        public IReadOnlyList<AreaComponentScore> Components { get; }
        public int CornerRectArea { get; }
        public float BaseArea { get; }
        public float CornerRectPenalty { get; }
        public int AreaCount { get; }
        public float AreaCountPenalty { get; }
    }

    public readonly struct AreaComponentScore
    {
        public AreaComponentScore(bool occupied, int size, float baseScore)
        {
            Occupied = occupied;
            Size = size;
            BaseScore = baseScore;
        }

        public bool Occupied { get; }
        public int Size { get; }
        public float BaseScore { get; }
        public float Total => BaseScore;
    }
}
