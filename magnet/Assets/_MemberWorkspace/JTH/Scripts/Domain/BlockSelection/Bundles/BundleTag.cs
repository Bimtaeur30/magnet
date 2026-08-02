namespace JTH.Scripts.Domain.BlockSelection.Bundles
{
    /// <summary>
    /// 번들의 용도 태그. 티어 스택(SPEC §9)이 태그별로 후보를 거른다.
    /// </summary>
    public enum BundleTag
    {
        Normal,
        Trap,
        ComboBreak,
        Relife,

        /// <summary>직전 턴 클리어 직후 "기분 좋은 패" — 큼직한 사각 위주 (사진 분석 근거, phase9).</summary>
        Momentum,
    }
}
