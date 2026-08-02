using JTH.Scripts.Domain.BlockSelection.Blame;
using JTH.Scripts.Domain.BlockSelection.Generation;
using JTH.Scripts.Domain.BlockSelection.Health;
using UnityEngine;

namespace JTH.Scripts.Data
{
    /// <summary>
    /// 병합 스폰(hybrid-spawn-algorithm) 튜닝. 구 BlockSelectionTuningSO에서 살아남는 필드
    /// (BoardHealth·Blame·특수 티어 게이트)만 갖고, 블록 분포는 42-ID 칸 수 가중표로 대체한다.
    /// 구 SO는 구 코드와 함께 롤백용으로 보존.
    /// </summary>
    [CreateAssetMenu(fileName = "HybridTuning", menuName = "Magnet/Hybrid Spawn Tuning")]
    public sealed class HybridTuningSO : ScriptableObject, IBoardHealthTuning, IBlameTuning, IOpportunityTuning
    {
        [field: Header("Health Zone")]
        [field: Tooltip("fillRate가 이 값 미만이면 TooEmpty 구간 (권장 0.12)")]
        [field: SerializeField] public float TooEmptyFillMax { get; private set; } = 0.12f;

        [field: Tooltip("fillRate가 이 값 초과면 TooDirty 구간 (권장 0.55)")]
        [field: SerializeField] public float TooDirtyFillMin { get; private set; } = 0.55f;

        [field: Tooltip("fill 구간 판정 후, healthScore가 이 값 미만이면 TooEmpty (권장 0.35)")]
        [field: SerializeField] public float TooEmptyScoreMax { get; private set; } = 0.35f;

        [field: Tooltip("fill 구간 판정 후, healthScore가 이 값 미만이면 TooDirty (권장 0.40)")]
        [field: SerializeField] public float TooDirtyScoreMax { get; private set; } = 0.40f;

        [field: Tooltip("TooDirtyFillMin 초과 시 fill 성분이 0까지 떨어지는 fillRate 폭. 0.35면 fill 0.90에서 0")]
        [field: SerializeField] public float FillDirtyFalloff { get; private set; } = 0.35f;

        [field: Header("Health Weights")]
        [field: Tooltip("healthScore에서 fillRate 성분 가중치 (성분 합 1 권장)")]
        [field: SerializeField] public float FillWeight { get; private set; } = 0.35f;

        [field: Tooltip("healthScore에서 dead zone(고립 빈칸 1~3) 성분 가중치")]
        [field: SerializeField] public float DeadZoneWeight { get; private set; } = 0.15f;

        [field: Tooltip("healthScore에서 큰 블록(3x3·1x5) 슬롯 성분 가중치")]
        [field: SerializeField] public float BigSlotWeight { get; private set; } = 0.15f;

        [field: Tooltip("healthScore에서 배치 자유도(프로브 피스 평균 합법 배치 수) 성분 가중치")]
        [field: SerializeField] public float FreedomWeight { get; private set; } = 0.15f;

        [field: Tooltip("healthScore에서 클러스터(점유 칸 직교 연결 응집도·최대 덩어리 크기) 성분 가중치")]
        [field: SerializeField] public float ClusterWeight { get; private set; } = 0.2f;

        [field: Header("Health Normalize")]
        [field: Tooltip("dead zone 개수를 0~1로 정규화할 상한. 이 개수 이상이면 성분 0")]
        [field: SerializeField] public int DeadZoneNormalizeMax { get; private set; } = 6;

        [field: Tooltip("큰 블록 슬롯 수 정규화 상한. 빈 8×8 보드 = 100 (3x3 36 + 1x5 가로·세로 64)")]
        [field: SerializeField] public int BigSlotNormalizeMax { get; private set; } = 100;

        [field: Tooltip("배치 자유도(피스당 평균 합법 배치 수, 회전 포함) 정규화 상한. 빈 보드 기준 ≈100")]
        [field: SerializeField] public float FreedomNormalizeMax { get; private set; } = 100f;

        [field: Tooltip("클러스터 성분에서 응집도(한 덩어리로 모임) 비중. 나머지는 최대 덩어리 크기 비중 (권장 0.5)")]
        [field: SerializeField, Range(0f, 1f)] public float ClusterCohesionShare { get; private set; } = 0.5f;

        [field: Tooltip("최대 덩어리 크기를 0~1로 정규화할 상한 칸 수. 이 이상 모여 있으면 크기 성분 만점 (권장 20)")]
        [field: SerializeField] public int ClusterSizeNormalizeMax { get; private set; } = 20;

        [field: Header("Blame")]
        [field: Tooltip("턴 종료 시 새 dead zone 1개당 blame 증가량. 1~3칸 포켓은 흔한 플레이라 과하면 응징 남발 (권장 5~12)")]
        [field: SerializeField] public float BlamePerDeadZone { get; private set; } = 8f;

        [field: Tooltip("중앙 2×2 영역 새 점유 칸 1개당 blame 증가량 (권장 3~5)")]
        [field: SerializeField] public float BlamePerCenterCell { get; private set; } = 4f;

        [field: Tooltip("큰 블록(3x3·1x5) 슬롯 수가 줄어든 턴에 1회 가산되는 blame (권장 8~12)")]
        [field: SerializeField] public float BlamePerBigSlotLost { get; private set; } = 10f;

        [field: Tooltip("배치 자유도 감소량 1당 blame 증가량. 클리어 없는 턴은 자유도가 자연 하락하므로 높으면 평범한 플레이도 벌점 (권장 0.1~0.2)")]
        [field: SerializeField] public float BlamePerFreedomDrop { get; private set; } = 0.15f;

        [field: Tooltip("healthScore 증가 1.0당 blame 차감량. 판을 개선한 턴은 실수 벌점을 상쇄 (권장 40~80, 0이면 끔)")]
        [field: SerializeField] public float BlameHealthGainRelief { get; private set; } = 60f;

        [field: Tooltip("매 턴 종료 시 누적 blame에 곱하는 감쇠율 (권장 0.65~0.75)")]
        [field: SerializeField] public float BlameDecayRate { get; private set; } = 0.7f;

        [field: Tooltip("GoodTurn 판정: 3피스 전부 배치 + 이번 턴 blame delta가 이 값 이하 (권장 5)")]
        [field: SerializeField] public float GoodTurnBlameDeltaMax { get; private set; } = 5f;

        [field: Header("Tier Gates")]
        [field: Tooltip("Relife(재시작 접대) 티어가 적용되는 재시작 세션 첫 턴 수 (권장 1~2). IsRetrySession 배선 전까지 미발동")]
        [field: SerializeField] public int RelifeTurnCount { get; private set; } = 2;

        [field: Tooltip("Trap 티어 게이트: blame이 이 값 이상 (권장 55)")]
        [field: SerializeField] public float BlameTrapThreshold { get; private set; } = 55f;

        [field: Tooltip("Trap 티어 발동 확률 (게이트 통과 후, 권장 0.005~0.01)")]
        [field: SerializeField] public float TrapProbability { get; private set; } = 0.008f;

        [field: Tooltip("ComboBreak 티어 게이트: blame이 이 값 이상 (권장 25)")]
        [field: SerializeField] public float BlameComboBreakThreshold { get; private set; } = 25f;

        [field: Tooltip("ComboBreak 티어 발동 확률 (게이트 통과 후, 권장 0.03~0.05)")]
        [field: SerializeField] public float ComboBreakProbability { get; private set; } = 0.04f;

        [field: Tooltip("기회 게이트 통과 후 Hospitality를 실제로 시도할 확률 (변덕, 권장 0.7~0.85)")]
        [field: SerializeField] public float HospitalityProbability { get; private set; } = 0.75f;

        [field: Tooltip("Pressure 게이트 통과 후 실제로 시도할 확률 (100% 아님)")]
        [field: SerializeField] public float PressureProbability { get; private set; } = 0.5f;

        [field: Tooltip("TooDirty가 아니어도 healthScore가 이 값 미만이면 Pressure 게이트 통과")]
        [field: SerializeField] public float PressureHealthThreshold { get; private set; } = 0.45f;

        [field: Header("Sampling")]
        [field: Tooltip("Relife 트리플 샘플 시도 횟수 (검증 실패분 포함)")]
        [field: SerializeField] public int RelifeSampleTries { get; private set; } = 12;

        [field: Tooltip("Trap 트리플 샘플 시도 횟수. Trap 검증(완주 불가 증명)은 비싸므로 보수적으로 (권장 6~10)")]
        [field: SerializeField] public int TrapSampleTries { get; private set; } = 8;

        [field: Tooltip("ComboBreak 트리플 샘플 시도 횟수. 콤보 불가 증명은 비싸므로 보수적으로 (권장 6~10)")]
        [field: SerializeField] public int ComboBreakSampleTries { get; private set; } = 8;

        [field: Tooltip("Hospitality 후보 트리플 샘플 횟수 (권장 50~200)")]
        [field: SerializeField] public int HospitalitySampleCount { get; private set; } = 60;

        [field: Tooltip("Pressure 후보 트리플 샘플 횟수 (유일수 판정은 비싸므로 보수적으로)")]
        [field: SerializeField] public int PressureSampleCount { get; private set; } = 40;

        [field: Tooltip("Hospitality 최선 결과 추정(빔 서치) 폭. 클수록 정확하지만 느림 (권장 4~8)")]
        [field: SerializeField] public int OutcomeBeamWidth { get; private set; } = 4;

        [field: Header("Hospitality")]
        [field: Tooltip("opportunityScore가 이 값 이상이어야 Hospitality 시도 (권장 0.65~0.75)")]
        [field: SerializeField] public float OpportunityHighThreshold { get; private set; } = 0.7f;

        [field: Tooltip("Hospitality 후보의 최소 품질: 완벽 플레이 시 총 클리어 라인 수가 이 값 미만이면 버림 (억지 올클 차단)")]
        [field: SerializeField] public int HospitalityMinQualityClears { get; private set; } = 2;

        [field: Tooltip("한 칸 부족한 행·열 1개당 opportunityScore 가산")]
        [field: SerializeField] public float OpportunityNearLineWeight { get; private set; } = 0.25f;

        [field: Tooltip("한 칸 부족한 행·열이 2개 이상일 때 추가 가산 (멀티라인 잠재)")]
        [field: SerializeField] public float OpportunityMultiLineBonus { get; private set; } = 0.15f;

        [field: Tooltip("올클리어 잠재 가산: fillRate가 하한 이하 + dead zone 0일 때")]
        [field: SerializeField] public float OpportunityAllClearWeight { get; private set; } = 0.2f;

        [field: Tooltip("올클리어 잠재로 판정하는 fillRate 상한 (권장 0.2)")]
        [field: SerializeField] public float OpportunityAllClearFillMax { get; private set; } = 0.2f;

        [field: Tooltip("큰 블록 슬롯 성분 가중치: 정규화된 bigPieceSlots × 이 값 가산")]
        [field: SerializeField] public float OpportunityBigSlotWeight { get; private set; } = 0.15f;

        [field: Tooltip("dead zone 1개당 opportunityScore 감점 (억지 패널티). 과하면 포켓 있을 때 접대가 안 나옴 (권장 0.05~0.1)")]
        [field: SerializeField] public float OpportunityDeadZonePenalty { get; private set; } = 0.08f;

        [field: Header("Pressure")]
        [field: Tooltip("유일해 난이도가 이 값 미만이면 버림 (너무 쉬운 unique 제외)")]
        [field: SerializeField] public float PressureDifficultyMin { get; private set; } = 0.5f;

        [field: Tooltip("난이도 가산: 유일해의 마지막 스텝이 큰 블록일 때")]
        [field: SerializeField] public float PressureBigFinishWeight { get; private set; } = 0.5f;

        [field: Tooltip("난이도 가산: 유일해의 앞 두 스텝에서 라인 클리어가 필요할 때")]
        [field: SerializeField] public float PressureSetupClearWeight { get; private set; } = 0.5f;

        [field: Tooltip("'큰 블록'으로 치는 최소 칸 수 (5칸 이상 권장)")]
        [field: SerializeField] public int PressureBigFinishMinCells { get; private set; } = 5;

        [Header("42-ID Pool Weights (칸 수 기준)")]
        [SerializeField, Tooltip("Relife 풀 가중치 — 1x1 포함 소형 위주 접대. 1x1(칸 1)은 이 풀에서만 나옴")]
        private CellCountWeightTable relifeWeights = new(new[] { 0f, 1.5f, 1f, 1f, 1f, 0f, 0f, 0f, 0f, 0f });

        [SerializeField, Tooltip("Trap 풀 가중치 — 대형 위주 (순서 함정 유도)")]
        private CellCountWeightTable trapWeights = new(new[] { 0f, 0f, 0f, 0.2f, 0.6f, 1.2f, 1.5f, 0f, 0f, 2f });

        [SerializeField, Tooltip("ComboBreak 풀 가중치 — 소·중형 위주 (넣을 순 있으나 클리어 어려움)")]
        private CellCountWeightTable comboBreakWeights = new(new[] { 0f, 0f, 0.8f, 1.2f, 1f, 0.3f, 0.2f, 0f, 0f, 0f });

        [SerializeField, Tooltip("Hospitality 풀 가중치 — 큰·긴 블록 우선 (강한 기회를 시원하게)")]
        private CellCountWeightTable hospitalityWeights = new(new[] { 0f, 0f, 0.3f, 0.6f, 1f, 1.2f, 1.2f, 0f, 0f, 1f });

        [SerializeField, Tooltip("Pressure 풀 가중치 — 큰 마무리 블록 선호 (유일수 난이도 확보)")]
        private CellCountWeightTable pressureWeights = new(new[] { 0f, 0f, 0.4f, 0.8f, 1f, 1.2f, 1f, 0f, 0f, 1.2f });

        public CellCountWeightTable RelifeWeights => relifeWeights;
        public CellCountWeightTable TrapWeights => trapWeights;
        public CellCountWeightTable ComboBreakWeights => comboBreakWeights;
        public CellCountWeightTable HospitalityWeights => hospitalityWeights;
        public CellCountWeightTable PressureWeights => pressureWeights;
    }
}
