using System.Collections.Generic;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    /// <summary>
    /// 보드 Area 점수 합산 결과. 게이트·번들 선택 타이브레이크에 쓴다.
    /// </summary>
    public readonly struct AreaScoreResult
    {
        public AreaScoreResult(float total, IReadOnlyList<AreaComponentScore> components)
        {
            Total = total;
            Components = components;
        }

        public float Total { get; }
        public IReadOnlyList<AreaComponentScore> Components { get; }
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
