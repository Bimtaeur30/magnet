namespace JTH.Scripts.Domain.BlockSelection.Tiers
{
    /// <summary>
    /// 번들이 티어 조건을 만족하는지 판정하는 규칙 (SPEC §9).
    /// </summary>
    public enum BundleValidation
    {
        /// <summary>hasAny + fullSequence — Relife·Normal용 (통과 가능 번들).</summary>
        Passable,

        /// <summary>hasAny + fullSequence 불가 — 일부만 넣고 막힘.</summary>
        Trap,

        /// <summary>hasAny + fullSequence + 콤보 유지 불가 — 살지만 이번 라운드 클리어 불가.</summary>
        ComboBreak,

        /// <summary>hasAny + fullSequence + 콤보 유지 가능 — 험한 판을 풀어줄 쉬운 번들.</summary>
        Easy,

        /// <summary>hasAny만 — 최종 fallback 강제 선택용.</summary>
        AnyPlaceable,
    }
}
