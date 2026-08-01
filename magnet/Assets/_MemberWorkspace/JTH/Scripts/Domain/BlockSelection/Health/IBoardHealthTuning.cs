namespace JTH.Scripts.Domain.BlockSelection.Health
{
    /// <summary>
    /// BoardHealthCalculator 튜닝 입력. SO 구현체(BlockSelectionTuningSO·HybridTuningSO)가 공급한다
    /// — 하이브리드 병합에서 새 튜닝 SO가 기존 계산기를 재사용하기 위한 추출 인터페이스.
    /// </summary>
    public interface IBoardHealthTuning
    {
        float TooEmptyFillMax { get; }
        float TooDirtyFillMin { get; }
        float TooEmptyScoreMax { get; }
        float TooDirtyScoreMax { get; }
        float FillDirtyFalloff { get; }
        float FillWeight { get; }
        float DeadZoneWeight { get; }
        float BigSlotWeight { get; }
        float FreedomWeight { get; }
        float ClusterWeight { get; }
        int DeadZoneNormalizeMax { get; }
        int BigSlotNormalizeMax { get; }
        float FreedomNormalizeMax { get; }
        float ClusterCohesionShare { get; }
        int ClusterSizeNormalizeMax { get; }
    }
}
