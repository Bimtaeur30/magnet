namespace JTH.Scripts.Domain.BlockSelection
{
    /// <summary>
    /// 티어 우선순위 스택의 최종 선택 결과 (SPEC §9). 로그·디버그·UI 훅 판정에 쓴다.
    /// </summary>
    public enum SelectionTier
    {
        Relife,
        Trap,
        ComboBreak,
        Hospitality,
        Momentum,
        Easy,
        Pressure,
        Normal,
        Fallback,
    }
}
