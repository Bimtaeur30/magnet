namespace JTH.Scripts.Domain.HybridSpawn
{
    /// <summary>
    /// 하이브리드 스폰의 최종 선택 경로. 특수 티어 5종은 게이트 통과 시에만,
    /// 나머지는 전부 BlockBlast 핸드오프 체인(BaseChain)이 담당한다.
    /// </summary>
    public enum HybridTier
    {
        Relife,
        Trap,
        ComboBreak,
        Hospitality,
        Pressure,
        BaseChain,
    }
}
