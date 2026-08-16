using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "AreaBundlePool", menuName = "Magnet/Area Bundle Pool")]
    public sealed class AreaBundlePoolSO : ScriptableObject
    {
        [Header("Lists")]
        [SerializeField, Tooltip("Normal 히트맵 후보 번들")]
        private List<AreaBundleEntry> normalBundles = new();

        [SerializeField, Tooltip("Easy 폴백 — 소형. Relife 1턴도 사용")]
        private List<AreaBundleEntry> easyBundles = new();

        [Header("Gate")]
        [SerializeField, Tooltip("점유 칸 ≥ 이면 dirty — pUnique로 Unique 시도 (권장 40)")]
        private int uniqueMinOccupied = 40;

        [SerializeField, Tooltip("dirty일 때 Unique를 고를 확률 (0~1)")]
        [Range(0f, 1f)]
        private float uniqueProbability = 0.45f;

        [SerializeField, Tooltip("Relife(IsRetrySession) 직후 Easy 강제 턴 수")]
        private int relifeEasyTurnCount = 1;

        [SerializeField, Tooltip("Unique 동적 생성 샘플 횟수 (트리플 추첨 × 역할 탐색)")]
        private int uniqueSampleCount = 80;

        [Header("Budget")]
        [SerializeField, Tooltip("Normal/Easy 히트맵 평가 후보 상한 (권장 64). 셔플 후 앞에서부터 하나씩 ScoreBest")]
        private int maxCandidatesToScore = 64;

        [Header("Heatmap Score")]
        [SerializeField, Tooltip("heat==0 칸당 감점 상한 n (권장 2). 실제 페널티 = clamp01(t)×n")]
        private int emptyHeatPenalty = 2;

        [SerializeField, Tooltip("t = 현재점수 / (이 값/3*2). t≥1이면 페널티=n. 권장 3000")]
        private int emptyHeatPenaltyMaxScore = 3000;

        [Header("Shape Weights — Main")]
        [SerializeField, Tooltip("ShapeId 1~42 가중. Normal/Easy용(현재 선택에선 Unique만 가중 사용)")]
        private float[] shapeWeights;

        [Header("Shape Weights — Unique unlock")]
        [SerializeField, Tooltip("Unique 동적 생성 추첨 가중. 0=제외")]
        private float[] uniqueShapeWeights;

        public IReadOnlyList<AreaBundleEntry> NormalBundles => normalBundles;
        public IReadOnlyList<AreaBundleEntry> EasyBundles => easyBundles;
        public int UniqueMinOccupied => uniqueMinOccupied < 0 ? 0 : uniqueMinOccupied;
        public float UniqueProbability => uniqueProbability;
        public int RelifeEasyTurnCount => relifeEasyTurnCount < 0 ? 0 : relifeEasyTurnCount;
        public int UniqueSampleCount => uniqueSampleCount < 1 ? 1 : uniqueSampleCount;
        public int MaxCandidatesToScore => maxCandidatesToScore < 1 ? 1 : maxCandidatesToScore;
        public int EmptyHeatPenalty => emptyHeatPenalty < 0 ? 0 : emptyHeatPenalty;
        public int EmptyHeatPenaltyMaxScore => emptyHeatPenaltyMaxScore < 1 ? 1 : emptyHeatPenaltyMaxScore;

        /// <summary>
        /// t = score / (max/3*2), 페널티 = clamp01(t) × EmptyHeatPenalty.
        /// </summary>
        public float ResolveEmptyHeatPenalty(int currentScore)
        {
            float maxN = EmptyHeatPenalty;
            if (maxN <= 0f)
            {
                return 0f;
            }

            float denom = EmptyHeatPenaltyMaxScore / 3f * 2f;
            if (denom <= 0f)
            {
                return maxN;
            }

            float t = currentScore / denom;
            if (t <= 0f)
            {
                return 0f;
            }

            if (t >= 1f)
            {
                return maxN;
            }

            return t * maxN;
        }

        /// <summary>ShapeId 가중. 범위 밖·미설정은 1. 음수는 0.</summary>
        public float GetShapeWeight(int shapeId, ShapeWeightProfile profile = ShapeWeightProfile.Main)
        {
            float[] weights = profile == ShapeWeightProfile.Unique
                ? EnsureUniqueShapeWeights()
                : EnsureShapeWeights();
            if (shapeId < ShapeIdMin || shapeId > ShapeIdMax)
            {
                return 1f;
            }

            float weight = weights[shapeId];
            return weight < 0f ? 0f : weight;
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

        /// <summary>Unique: 4칸 테트로미노 중심.</summary>
        private static float DefaultUniqueShapeWeight(int id)
        {
            return id switch
            {
                1 => 1f,
                2 => 1.5f,
                3 => 1.5f,
                37 => 1.5f,
                38 => 1.5f,
                4 => 2.5f,
                5 => 2.5f,
                6 => 5.5f,
                15 => 5.5f,
                27 => 5.5f,
                28 => 5.5f,
                39 => 2f,
                40 => 2f,
                41 => 2f,
                10 => 10f,
                14 => 10f,
                16 => 10f,
                18 => 10f,
                19 => 10f,
                20 => 10f,
                25 => 10f,
                26 => 10f,
                8 => 9f,
                29 => 9f,
                30 => 9f,
                31 => 9f,
                32 => 9f,
                33 => 9f,
                34 => 9f,
                42 => 9f,
                9 => 4.5f,
                7 => 2.5f,
                17 => 2.5f,
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
            EnsureUniqueShapeWeights();
            ClampWeights(shapeWeights);
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

        [ContextMenu("Fill Starter Normal+Easy Bundles")]
        private void FillStarterBundles()
        {
            normalBundles = AreaBundleStarterData.CreateNormal();
            easyBundles = AreaBundleStarterData.CreateEasy();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Reset Main Shape Weights To 1")]
        private void ResetShapeWeightsToOne()
        {
            shapeWeights = null;
            EnsureShapeWeights();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Reset Unique Shape Weights (tetromino bias)")]
        private void ResetUniqueShapeWeights()
        {
            uniqueShapeWeights = null;
            EnsureUniqueShapeWeights();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
