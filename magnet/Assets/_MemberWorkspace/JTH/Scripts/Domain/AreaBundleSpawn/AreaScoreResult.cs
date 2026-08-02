using System.Collections.Generic;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    /// <summary>
    /// 보드 Area 점수. Total = BaseArea − RectPenalty − AreaCountPenalty.
    /// </summary>
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
        /// <summary>찬+빈 최대면적 greedy 직사각 총개수.</summary>
        public int RectCount { get; }
        /// <summary>4-연결 size·변 합.</summary>
        public float BaseArea { get; }
        /// <summary>rectCountPenalty × RectCount.</summary>
        public float RectPenalty { get; }
        /// <summary>4-연결 Area(찬+빈) 개수.</summary>
        public int AreaCount { get; }
        /// <summary>areaCountPenalty × AreaCount.</summary>
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
        /// <summary>직교 외곽의 직선 변 개수. 빈 Area는 0.</summary>
        public int SideCount { get; }
        public float BaseScore { get; }
        public float SideBonus { get; }
        public float Total => BaseScore + SideBonus;
    }
}
