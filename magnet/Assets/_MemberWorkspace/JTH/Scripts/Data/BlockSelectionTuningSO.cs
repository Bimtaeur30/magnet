using System.Collections.Generic;
using JTH.Scripts.Domain.BlockSelection.Blame;
using JTH.Scripts.Domain.BlockSelection.Generation;
using JTH.Scripts.Domain.BlockSelection.Health;
using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "BlockSelectionTuning", menuName = "Magnet/Block Selection Tuning")]
    public sealed class BlockSelectionTuningSO : ScriptableObject, IBoardHealthTuning, IBlameTuning, IOpportunityTuning
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

        [field: Tooltip("healthScore에서 배치 자유도(테스트 피스 평균 합법 배치 수) 성분 가중치")]
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

        [field: Tooltip("배치 자유도 감소량 1당 blame 증가량. 클리어 없는 턴은 자유도가 자연 하락(10~30)하므로 높으면 평범한 플레이도 벌점 (권장 0.1~0.2)")]
        [field: SerializeField] public float BlamePerFreedomDrop { get; private set; } = 0.15f;

        [field: Tooltip("healthScore 증가 1.0당 blame 차감량. 판을 개선한 턴은 실수 벌점을 상쇄 — +0.1 개선이면 -6 (권장 40~80, 0이면 끔)")]
        [field: SerializeField] public float BlameHealthGainRelief { get; private set; } = 60f;

        [field: Tooltip("매 턴 종료 시 누적 blame에 곱하는 감쇠율 (권장 0.65~0.75)")]
        [field: SerializeField] public float BlameDecayRate { get; private set; } = 0.7f;

        [field: Header("Blame Thresholds")]
        [field: Tooltip("ComboBreak 티어 게이트: blame이 이 값 이상 (권장 25)")]
        [field: SerializeField] public float BlameComboBreakThreshold { get; private set; } = 25f;

        [field: Tooltip("Pressure 가중 게이트: blame이 이 값 이상 (권장 35)")]
        [field: SerializeField] public float BlamePressureThreshold { get; private set; } = 35f;

        [field: Tooltip("Trap 티어 게이트: blame이 이 값 이상 (권장 55)")]
        [field: SerializeField] public float BlameTrapThreshold { get; private set; } = 55f;

        [field: Tooltip("Easy 티어 게이트: blame이 이 값 미만이어야 함 (유저 탓 아님, 권장 15)")]
        [field: SerializeField] public float EasyBlameMax { get; private set; } = 15f;

        [field: Tooltip("GoodTurn 판정: 3피스 전부 배치 + 이번 턴 blame delta가 이 값 이하 (권장 5)")]
        [field: SerializeField] public float GoodTurnBlameDeltaMax { get; private set; } = 5f;

        [Header("Block Weights")]
        [SerializeField, Tooltip("모양별 티어 추첨 가중치 테이블 (SPEC §14.2). 1x1·1x2는 전 티어 0 권장")]
        private List<BlockShapeWeight> blockWeights = new();

        public IReadOnlyList<BlockShapeWeight> BlockWeights => blockWeights;

        [field: Header("Tier Gates")]
        [field: Tooltip("Relife(재시작 접대) 티어가 적용되는 재시작 세션 첫 턴 수 (권장 1~2)")]
        [field: SerializeField] public int RelifeTurnCount { get; private set; } = 2;

        [field: Tooltip("Trap 티어 발동 확률 (게이트 통과 후, 권장 0.005~0.01)")]
        [field: SerializeField] public float TrapProbability { get; private set; } = 0.008f;

        [field: Tooltip("ComboBreak 티어 발동 확률 (게이트 통과 후, 권장 0.03~0.05)")]
        [field: SerializeField] public float ComboBreakProbability { get; private set; } = 0.04f;

        [field: Tooltip("Easy 티어 게이트: healthScore가 이 값 미만이면 판이 험함으로 판정")]
        [field: SerializeField] public float EasyHealthThreshold { get; private set; } = 0.45f;

        [field: Tooltip("티어 하나가 번들 검증(솔버)을 시도할 최대 번들 수. 초과 시 fallthrough")]
        [field: SerializeField] public int BundleProbeCount { get; private set; } = 8;

        [field: Tooltip("Normal 티어에서 결과 BoardHealth를 비교할 통과 후보 핸드 수. 1이면 단순 가중 랜덤과 동일 (권장 3~5)")]
        [field: SerializeField] public int NormalHealthCandidateCount { get; private set; } = 4;

        [field: Tooltip("Normal·Easy 티어 독립 추첨 핸드의 최대 샘플 시도 횟수. 검증 실패분 포함 (권장 10~16)")]
        [field: SerializeField] public int NormalSampleTries { get; private set; } = 12;

        [field: Header("Momentum")]
        [field: Tooltip("Momentum(큼직한 기분 좋은 패) 티어 시도 확률. 높으면 클리어→큰 사각→또 클리어 양성 루프로 점수가 쉬워짐 (권장 0.3~0.5, 0이면 끔)")]
        [field: SerializeField] public float MomentumProbability { get; private set; } = 0.4f;

        [field: Tooltip("Momentum 발동에 필요한 직전 턴 최소 클리어 칸 수. 한 줄 = 8칸이므로 10이면 멀티라인급 턴에서만 발동 (권장 9~16)")]
        [field: SerializeField] public int MomentumMinClearedCells { get; private set; } = 10;

        [field: Header("Density Bias")]
        [field: Tooltip("fillRate가 이 값 초과(빽빽)면 얇은 블록 부스트 + 큰 블록 감점 적용 (권장 0.38~0.45)")]
        [field: SerializeField] public float DenseFillMin { get; private set; } = 0.38f;

        [field: Tooltip("빽빽한 보드에서 얇은 블록 포함 번들에 곱하는 배수 (권장 1.5~2.5, 1이면 끔)")]
        [field: SerializeField] public float DenseSlimBoost { get; private set; } = 2f;

        [field: Tooltip("빽빽한 보드에서 큰 블록(6칸+)에 곱하는 배수 (0~1). 꽉 찬 판에 3x3·3x2가 쏟아지는 것 방지 (권장 0.3~0.6, 1이면 끔)")]
        [field: SerializeField, Range(0f, 1f)] public float DenseBigPenalty { get; private set; } = 0.45f;

        [field: Tooltip("fillRate가 이 값 미만(널널)이면 큰 블록(6칸 이상) 포함 번들의 추첨 가중 배수 적용 (권장 0.25)")]
        [field: SerializeField] public float SparseFillMax { get; private set; } = 0.25f;

        [field: Tooltip("널널한 보드에서 큰 블록 포함 번들에 곱하는 배수 (권장 1.3~2, 1이면 끔)")]
        [field: SerializeField] public float SparseBigBoost { get; private set; } = 1.5f;

        [field: Header("Snug Fit")]
        [field: Tooltip("쏙 판정 최소 둘레 막힘 비율. 위만 뚫린 포켓 ≈ 0.75, 사방 밀폐 = 1.0. 이 미만이면 보너스 없음. 낮으면 작은 조각이 상시 부스트돼 노골적 (권장 0.8)")]
        [field: SerializeField, Range(0f, 1f)] public float SnugEnclosureMin { get; private set; } = 0.8f;

        [field: Tooltip("쏙 맞는 모양의 추첨 가중 증가폭. 사방 밀폐 시 가중 ×(1+이 값). 크면 노골적 (권장 0.5~1, 0이면 끔)")]
        [field: SerializeField] public float SnugWeightBoost { get; private set; } = 0.6f;

        [field: Tooltip("Normal 후보 랭킹에 더하는 쏙 보너스 상한 (healthScore 스케일). 예측 Health가 비슷할 때만 갈리는 수준 권장 (권장 0.05~0.1)")]
        [field: SerializeField] public float SnugNormalRankBonus { get; private set; } = 0.06f;

        [field: Header("Hospitality")]
        [field: Tooltip("opportunity 게이트 통과 후 Hospitality를 실제로 시도할 확률 (변덕, 권장 0.7~0.85)")]
        [field: SerializeField] public float HospitalityProbability { get; private set; } = 0.75f;

        [field: Tooltip("opportunityScore가 이 값 이상이어야 Hospitality 시도 (권장 0.65~0.75)")]
        [field: SerializeField] public float OpportunityHighThreshold { get; private set; } = 0.7f;

        [field: Tooltip("Hospitality 후보 3피스 조합 샘플 횟수 (권장 50~200)")]
        [field: SerializeField] public int HospitalitySampleCount { get; private set; } = 60;

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

        [field: Tooltip("최선 결과 추정(빔 서치) 폭. 클수록 정확하지만 느림 (권장 4~8)")]
        [field: SerializeField] public int OutcomeBeamWidth { get; private set; } = 4;

        [field: Header("Pressure")]
        [field: Tooltip("Pressure 게이트 통과 후 실제로 시도할 확률 (100% 아님)")]
        [field: SerializeField] public float PressureProbability { get; private set; } = 0.5f;

        [field: Tooltip("TooDirty가 아니어도 healthScore가 이 값 미만이면 Pressure 게이트 통과")]
        [field: SerializeField] public float PressureHealthThreshold { get; private set; } = 0.45f;

        [field: Tooltip("Pressure 후보 3피스 조합 샘플 횟수 (유일수 판정은 비싸므로 보수적으로)")]
        [field: SerializeField] public int PressureSampleCount { get; private set; } = 40;

        [field: Tooltip("유일해 난이도가 이 값 미만이면 버림 (너무 쉬운 unique 제외)")]
        [field: SerializeField] public float PressureDifficultyMin { get; private set; } = 0.5f;

        [field: Tooltip("난이도 가산: 유일해의 마지막 스텝이 큰 블록일 때")]
        [field: SerializeField] public float PressureBigFinishWeight { get; private set; } = 0.5f;

        [field: Tooltip("난이도 가산: 유일해의 앞 두 스텝에서 라인 클리어가 필요할 때")]
        [field: SerializeField] public float PressureSetupClearWeight { get; private set; } = 0.5f;

        [field: Tooltip("'큰 블록'으로 치는 최소 칸 수 (1x5·3x3·L3x3 = 5칸 이상)")]
        [field: SerializeField] public int PressureBigFinishMinCells { get; private set; } = 5;

        [field: Header("Fallback")]
        [field: Tooltip("Fallback 실시간 조합 샘플 횟수")]
        [field: SerializeField] public int FallbackSampleCount { get; private set; } = 40;
    }
}
