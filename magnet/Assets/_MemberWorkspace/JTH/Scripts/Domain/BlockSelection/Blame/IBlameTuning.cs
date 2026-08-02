namespace JTH.Scripts.Domain.BlockSelection.Blame
{
    /// <summary>
    /// BlameTracker 튜닝 입력. SO 구현체(BlockSelectionTuningSO·HybridTuningSO)가 공급한다.
    /// </summary>
    public interface IBlameTuning
    {
        float BlamePerDeadZone { get; }
        float BlamePerCenterCell { get; }
        float BlamePerBigSlotLost { get; }
        float BlamePerFreedomDrop { get; }
        float BlameHealthGainRelief { get; }
        float BlameDecayRate { get; }
        float GoodTurnBlameDeltaMax { get; }
    }
}
