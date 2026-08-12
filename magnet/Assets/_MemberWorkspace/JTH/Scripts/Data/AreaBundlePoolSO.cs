using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "AreaBundlePool", menuName = "Magnet/Area Bundle Pool")]
    public sealed class AreaBundlePoolSO : ScriptableObject
    {
        [Header("Lists")]
        [SerializeField, Tooltip("Main Normal 번들. Clean 전용 올클 친화 번들은 여기 없음")]
        private List<AreaBundleEntry> normalBundles = new();

        [SerializeField, Tooltip("Clean Normal Area 전용(Main + 올클 친화 추가분). Main Area/빈보드 가중랜덤엔 미사용")]
        private List<AreaBundleEntry> cleanBundles = new();

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

        [SerializeField, Tooltip("번들당 완주 시퀀스 탐색 상한 (권장 120). 올리면 Explain/MaxArea 품질↑·비용↑")]
        private int maxSequencesPerBundle = 120;

        [SerializeField, Tooltip("완주 클리어·올클 추정 빔 폭 (권장 4~8)")]
        private int outcomeBeamWidth = 4;

        [SerializeField, Tooltip("빔 Area 근사 후 MaxArea로 정밀화할 상위 후보 수 (권장 3~5). 0=정밀화 없음")]
        private int maxAreaRefineTopK = 4;

        [Header("Clear Priority (Normal)")]
        [SerializeField, Tooltip("점유 칸이 이 값 이하일 때만 올클 고정 풀 Exact 검사 (권장 16). 빔 미사용")]
        private int allClearMaxOccupied = 16;

        [SerializeField, Tooltip("올클 풀 Exact 통과 후보가 있을 때 지급 확률 (권장 0.75)")]
        [Range(0f, 1f)]
        private float allClearProbability = 0.75f;

        [SerializeField, Tooltip("올클 패 지급 후 올클 최우선을 쉬는 턴 수 (권장 1). 빈 보드는 별도로 올클 검사 스킵")]
        private int allClearCooldownTurns = 1;

        [SerializeField, Tooltip("접대: 구멍 8이웃 윤곽 채움 비율 하한 (권장 0.35)")]
        [Range(0f, 1f)]
        private float hospitalityContourMinFill = 0.35f;

        [SerializeField, Tooltip("접대 후보가 있을 때 지급 확률 (권장 0.35). 낙첨 시 이번 턴 Normal")]
        [Range(0f, 1f)]
        private float hospitalityProbability = 0.35f;

        [SerializeField, Tooltip("접대 확정 후, 핏이 3칸뿐일 때 추가 통과 확률 (권장 0.5). 낙첨 시 Normal")]
        [Range(0f, 1f)]
        private float hospitalityThreeCellProbability = 0.5f;

        [Header("Death Reject (Normal/Easy Area)")]
        [SerializeField, Tooltip("Death%가 이 값 초과면 배제(예산 내 완주 시에만). 권장 30")]
        [Range(0f, 100f)]
        private float deathRejectPercent = 30f;

        [SerializeField, Tooltip("effective Area 상위부터 Death 배제 시도 횟수. 전부 배제 시 1등 채택. 권장 8")]
        private int deathRejectMaxTries = 8;

        [SerializeField, Tooltip("Death 분모(검사 갈래) 상한. 초과 시 검사 중단·통과. 0=무제한. 권장 48")]
        private int deathBranchBudget = 48;

        [Header("Normal Dual Mode (Clean / Main)")]
        [SerializeField, Tooltip("boardArea ≤ 이면 Main(생존 가중). > 이면 Clean(올클 친화 가중). 낮출수록 Main 드묾 (권장 -15)")]
        private float survivalAreaMax = -15f;

        [SerializeField, Tooltip("Clean Normal Area 지급 시 다음 패를 최적 보드에서 미리 뽑을 확률 (권장 0.4)")]
        [Range(0f, 1f)]
        private float cleanChainProbability = 0.4f;

        [Header("Shape Weights — Main (survival)")]
        [SerializeField, Tooltip("Main: ShapeId 1~42 가중. boardArea≤survivalAreaMax 일 때 Normal/Easy. 접대/올클 미적용")]
        private float[] shapeWeights;

        [Header("Shape Weights — Clean (all-clear friendly)")]
        [SerializeField, Tooltip("Clean: ShapeId 1~42 가중. boardArea>survivalAreaMax 일 때 Normal Area. 기본 1 · 작은 ㄱ만 0")]
        private float[] cleanShapeWeights;

        [Header("Shape Weights — Unique unlock")]
        [SerializeField, Tooltip("Unique 동적 생성 추첨 가중. 기본=Unique 폴더 관측. 0=제외. 작은 ㄱ·미관측 초소형/대각 0")]
        private float[] uniqueShapeWeights;

        public IReadOnlyList<AreaBundleEntry> NormalBundles => normalBundles;
        public IReadOnlyList<AreaBundleEntry> CleanBundles => cleanBundles;
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
        public int MaxAreaRefineTopK => maxAreaRefineTopK < 0 ? 0 : maxAreaRefineTopK;
        public int AllClearMaxOccupied => allClearMaxOccupied < 1 ? 1 : allClearMaxOccupied;
        public float AllClearProbability => allClearProbability;
        public int AllClearCooldownTurns => allClearCooldownTurns < 0 ? 0 : allClearCooldownTurns;
        public float HospitalityContourMinFill => hospitalityContourMinFill;
        public float HospitalityProbability => hospitalityProbability;
        public float HospitalityThreeCellProbability => hospitalityThreeCellProbability;
        public float DeathRejectPercent => deathRejectPercent;
        public int DeathRejectMaxTries => deathRejectMaxTries < 1 ? 1 : deathRejectMaxTries;
        public int DeathBranchBudget => deathBranchBudget < 0 ? 0 : deathBranchBudget;
        public float SurvivalAreaMax => survivalAreaMax;
        public float CleanChainProbability => cleanChainProbability;

        /// <summary>ShapeId 가중. 범위 밖·미설정은 1. 음수는 0.</summary>
        public float GetShapeWeight(int shapeId, ShapeWeightProfile profile = ShapeWeightProfile.Main)
        {
            float[] weights = profile switch
            {
                ShapeWeightProfile.Clean => EnsureCleanShapeWeights(),
                ShapeWeightProfile.Unique => EnsureUniqueShapeWeights(),
                _ => EnsureShapeWeights(),
            };
            if (shapeId < ShapeIdMin || shapeId > ShapeIdMax)
            {
                return 1f;
            }

            float weight = weights[shapeId];
            return weight < 0f ? 0f : weight;
        }

        /// <summary>번들 세 피스 Shape 가중 산술평균.</summary>
        public float MeanShapeWeight(IReadOnlyList<int> ids, ShapeWeightProfile profile = ShapeWeightProfile.Main)
        {
            if (ids == null || ids.Count == 0)
            {
                return 1f;
            }

            float sum = 0f;
            for (int i = 0; i < ids.Count; ++i)
            {
                sum += GetShapeWeight(ids[i], profile);
            }

            return sum / ids.Count;
        }

        private const int ShapeIdMin = 1;
        private const int ShapeIdMax = 42;

        private float[] EnsureShapeWeights()
        {
            int needed = ShapeIdMax + 1;
            if (shapeWeights != null && shapeWeights.Length == needed)
            {
                return shapeWeights;
            }

            float[] next = new float[needed];
            for (int id = ShapeIdMin; id <= ShapeIdMax; ++id)
            {
                next[id] = shapeWeights != null && id < shapeWeights.Length
                    ? shapeWeights[id]
                    : 1f;
            }

            shapeWeights = next;
            return shapeWeights;
        }

        private float[] EnsureCleanShapeWeights()
        {
            int needed = ShapeIdMax + 1;
            if (cleanShapeWeights != null && cleanShapeWeights.Length == needed)
            {
                return cleanShapeWeights;
            }

            float[] next = new float[needed];
            for (int id = ShapeIdMin; id <= ShapeIdMax; ++id)
            {
                if (cleanShapeWeights != null && id < cleanShapeWeights.Length)
                {
                    next[id] = cleanShapeWeights[id];
                }
                else
                {
                    next[id] = 1f;
                }
            }

            cleanShapeWeights = next;
            return cleanShapeWeights;
        }

        private float[] EnsureUniqueShapeWeights()
        {
            int needed = ShapeIdMax + 1;
            if (uniqueShapeWeights != null && uniqueShapeWeights.Length == needed)
            {
                return uniqueShapeWeights;
            }

            float[] next = new float[needed];
            for (int id = ShapeIdMin; id <= ShapeIdMax; ++id)
            {
                if (uniqueShapeWeights != null && id < uniqueShapeWeights.Length)
                {
                    next[id] = uniqueShapeWeights[id];
                }
                else
                {
                    next[id] = DefaultUniqueShapeWeight(id);
                }
            }

            uniqueShapeWeights = next;
            return uniqueShapeWeights;
        }

        /// <summary>
        /// Unique: 4칸 테트로미노 중심.
        /// 너무 작으면 해가 많고, 너무 크면 자리가 뻔함 → Death%↑·해 경로↓ 균형.
        /// </summary>
        private static float DefaultUniqueShapeWeight(int id)
        {
            return id switch
            {
                // 1~2칸 — 해 과다 → 낮게
                1 => 1f,
                2 => 1.5f,
                3 => 1.5f,
                37 => 1.5f,
                38 => 1.5f,
                // 3칸 — 보조 (작은 ㄱ은 언락용으로 중간)
                4 => 2.5f,
                5 => 2.5f,
                6 => 5.5f,
                15 => 5.5f,
                27 => 5.5f,
                28 => 5.5f,
                39 => 2f,
                40 => 2f,
                41 => 2f,
                // 4칸 T/S/Z — 주력
                10 => 10f,
                14 => 10f,
                16 => 10f,
                18 => 10f,
                19 => 10f,
                20 => 10f,
                25 => 10f,
                26 => 10f,
                // 4칸 L/J — 주력
                8 => 9f,
                29 => 9f,
                30 => 9f,
                31 => 9f,
                32 => 9f,
                33 => 9f,
                34 => 9f,
                42 => 9f,
                // 2×2·I4 — 중간/낮음 (배치 유연하거나 일자로 뻔함)
                9 => 4.5f,
                7 => 2.5f,
                17 => 2.5f,
                // 5칸+ — 거의 제외 (놓을 곳 적어 유일수가 너무 쉬움)
                11 => 0.25f,
                12 => 0.25f,
                13 => 0f,
                21 => 0.25f,
                22 => 0.25f,
                23 => 0.25f,
                24 => 0.25f,
                35 => 0.25f,
                36 => 0.25f,
                _ => 0f,
            };
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureShapeWeights();
            EnsureCleanShapeWeights();
            EnsureUniqueShapeWeights();
            ClampWeights(shapeWeights);
            ClampWeights(cleanShapeWeights);
            ClampWeights(uniqueShapeWeights);
        }

        private static void ClampWeights(float[] weights)
        {
            if (weights == null)
            {
                return;
            }

            for (int id = ShapeIdMin; id <= ShapeIdMax && id < weights.Length; ++id)
            {
                if (weights[id] < 0f)
                {
                    weights[id] = 0f;
                }
            }
        }

        [ContextMenu("Fill Starter Normal+Clean+Easy+AllClear Bundles")]
        private void FillStarterBundles()
        {
            normalBundles = AreaBundleStarterData.CreateNormal();
            cleanBundles = AreaBundleStarterData.CreateClean();
            easyBundles = AreaBundleStarterData.CreateEasy();
            allClearBundles = AreaBundleStarterData.CreateAllClear();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Reset Main Shape Weights To 1")]
        private void ResetShapeWeightsToOne()
        {
            shapeWeights = null;
            EnsureShapeWeights();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Reset Clean Shape Weights (all 1)")]
        private void ResetCleanShapeWeights()
        {
            cleanShapeWeights = null;
            EnsureCleanShapeWeights();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Reset Unique Shape Weights (folder freq)")]
        private void ResetUniqueShapeWeights()
        {
            uniqueShapeWeights = null;
            EnsureUniqueShapeWeights();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
