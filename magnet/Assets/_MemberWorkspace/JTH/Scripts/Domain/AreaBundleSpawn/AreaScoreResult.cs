using System.Collections.Generic;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    public readonly struct AreaScoreResult
    {
        public AreaScoreResult(
            float total,
            IReadOnlyList<AreaComponentScore> components,
            int rectCount,
            float baseArea,
            float rectPenalty,
            int areaCount,
            float areaCountPenalty)
        {
            Total = total;
            Components = components;
            RectCount = rectCount;
            BaseArea = baseArea;
            RectPenalty = rectPenalty;
            AreaCount = areaCount;
            AreaCountPenalty = areaCountPenalty;
        }

        public float Total { get; }
        public IReadOnlyList<AreaComponentScore> Components { get; }
        public int RectCount { get; }
        public float BaseArea { get; }
        public float RectPenalty { get; }
        public int AreaCount { get; }
        public float AreaCountPenalty { get; }
    }

    public readonly struct AreaComponentScore
    {
        public AreaComponentScore(bool occupied, int size, int sideCount, float baseScore, float sideBonus)
        {
            Occupied = occupied;
            Size = size;
            SideCount = sideCount;
            BaseScore = baseScore;
            SideBonus = sideBonus;
        }

        public bool Occupied { get; }
        public int Size { get; }
        public int SideCount { get; }
        public float BaseScore { get; }
        public float SideBonus { get; }
        public float Total => BaseScore + SideBonus;
    }
}
