namespace JTH.Scripts.Domain.BlockSelection.Generation
{
    /// <summary>
    /// OpportunityScorer 튜닝 입력. SO 구현체(BlockSelectionTuningSO·HybridTuningSO)가 공급한다.
    /// </summary>
    public interface IOpportunityTuning
    {
        float OpportunityNearLineWeight { get; }
        float OpportunityMultiLineBonus { get; }
        float OpportunityAllClearFillMax { get; }
        float OpportunityAllClearWeight { get; }
        int BigSlotNormalizeMax { get; }
        float OpportunityBigSlotWeight { get; }
        float OpportunityDeadZonePenalty { get; }
    }
}
