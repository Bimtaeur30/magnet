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
    }
}
