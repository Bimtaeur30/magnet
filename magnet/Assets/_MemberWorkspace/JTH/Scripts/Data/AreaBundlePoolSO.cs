using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "AreaBundlePool", menuName = "Magnet/Area Bundle Pool")]
    public sealed class AreaBundlePoolSO : ScriptableObject
    {
        [Header("Lists")]
        [SerializeField, Tooltip("기본(Normal) 번들 — Blocks2 스크린샷 전수. 필터 없음, weight=관측횟수만")]
        private List<AreaBundleEntry> normalBundles = new();

        [SerializeField, Tooltip("Easy 폴백 — 1x1 계열·소형. Relife 1턴도 사용")]
        private List<AreaBundleEntry> easyBundles = new();

        [SerializeField, Tooltip("올클 전용 고정 번들(소수). 점유 칸이 적을 때만 Exact 완주·비움 검사")]
        private List<AreaBundleEntry> allClearBundles = new();

        [Header("Area Score")]
        [SerializeField, Tooltip("빈/찬 Area size + 찬 직사각·Area 개수 패널티")]
        private AreaScoreTuning areaScore = new();

        [Header("Gate")]
        [SerializeField, Tooltip("현재 보드 Area ≤ 이면 dirty — pUnique로 Unique 시도")]
        private float uniqueAreaThreshold = -15f;

        [SerializeField, Tooltip("dirty일 때 Unique를 고를 확률 (0~1)")]
        [Range(0f, 1f)]
        private float uniqueProbability = 0.45f;

        [SerializeField, Tooltip("Relife(IsRetrySession) 직후 Easy 강제 턴 수")]
        private int relifeEasyTurnCount = 1;

        [SerializeField, Tooltip("Unique 동적 생성 샘플 횟수 (트리플 추첨 × 역할 탐색)")]
        private int uniqueSampleCount = 80;

        [Header("Budget")]
        [SerializeField, Tooltip("Normal/Easy 평가 후보 상한")]
        private int maxCandidatesToScore = 16;

        [SerializeField, Tooltip("번들당 완주 시퀀스 탐색 상한")]
        private int maxSequencesPerBundle = 48;

        [SerializeField, Tooltip("완주 클리어·올클 추정 빔 폭 (권장 4~8)")]
        private int outcomeBeamWidth = 4;

        [Header("Clear Priority (Normal)")]
        [SerializeField, Tooltip("점유 칸이 이 값 이하일 때만 올클 고정 풀 Exact 검사 (권장 16). 빔 미사용")]
        private int allClearMaxOccupied = 16;

        [SerializeField, Tooltip("올클 풀 Exact 통과 후보가 있을 때 지급 확률 (권장 0.75)")]
        [Range(0f, 1f)]
        private float allClearProbability = 0.75f;

        [SerializeField, Tooltip("올클 패 지급 후 올클 최우선을 쉬는 턴 수 (권장 1). 빈 보드는 별도로 올클 검사 스킵")]
        private int allClearCooldownTurns = 1;

        [SerializeField, Tooltip("접대: 구멍 8이웃 윤곽 채움 비율 하한 (권장 0.7)")]
        [Range(0f, 1f)]
        private float hospitalityContourMinFill = 0.7f;

        [SerializeField, Tooltip("접대 후보가 있을 때 지급 확률 (권장 0.35). 낙첨 시 이번 턴 Normal")]
        [Range(0f, 1f)]
        private float hospitalityProbability = 0.35f;

        public IReadOnlyList<AreaBundleEntry> NormalBundles => normalBundles;
        public IReadOnlyList<AreaBundleEntry> EasyBundles => easyBundles;
        public IReadOnlyList<AreaBundleEntry> AllClearBundles => allClearBundles;
        public AreaScoreTuning AreaScore => areaScore ??= new AreaScoreTuning();
        public float UniqueAreaThreshold => uniqueAreaThreshold;
        public float UniqueProbability => uniqueProbability;
        public int RelifeEasyTurnCount => relifeEasyTurnCount < 0 ? 0 : relifeEasyTurnCount;
        public int UniqueSampleCount => uniqueSampleCount < 1 ? 1 : uniqueSampleCount;
        public int MaxCandidatesToScore => maxCandidatesToScore < 1 ? 1 : maxCandidatesToScore;
        public int MaxSequencesPerBundle => maxSequencesPerBundle < 1 ? 1 : maxSequencesPerBundle;
        public int OutcomeBeamWidth => outcomeBeamWidth < 1 ? 1 : outcomeBeamWidth;
        public int AllClearMaxOccupied => allClearMaxOccupied < 1 ? 1 : allClearMaxOccupied;
        public float AllClearProbability => allClearProbability;
        public int AllClearCooldownTurns => allClearCooldownTurns < 0 ? 0 : allClearCooldownTurns;
        public float HospitalityContourMinFill => hospitalityContourMinFill;
        public float HospitalityProbability => hospitalityProbability;

#if UNITY_EDITOR
        [ContextMenu("Fill Starter Normal+Easy+AllClear Bundles")]
        private void FillStarterBundles()
        {
            normalBundles = AreaBundleStarterData.CreateNormal();
            easyBundles = AreaBundleStarterData.CreateEasy();
            allClearBundles = AreaBundleStarterData.CreateAllClear();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
