using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Data
{
    /// <summary>
    /// Area-번들 풀 + cascade 게이트.
    /// Unique는 동적 UniqueUnlockGenerator (번들 리스트 미사용).
    /// Area = size/변 base − k×직사각수.
    /// </summary>
    [CreateAssetMenu(fileName = "AreaBundlePool", menuName = "Magnet/Area Bundle Pool")]
    public sealed class AreaBundlePoolSO : ScriptableObject
    {
        [Header("Lists")]
        [SerializeField, Tooltip("기본(Normal) 번들 — 빈도≥2 비유일")]
        private List<AreaBundleEntry> normalBundles = new();

        [SerializeField, Tooltip("Easy 폴백 — 1x1 계열·소형. Relife 1턴도 사용")]
        private List<AreaBundleEntry> easyBundles = new();

        [Header("Area Score")]
        [SerializeField, Tooltip("빈/찬 Area·변 보너스 + 직사각 개수 패널티")]
        private AreaScoreTuning areaScore = new();

        [Header("Gate")]
        [SerializeField, Tooltip("현재 보드 Area ≤ 이면 dirty — pUnique로 Unique 시도")]
        private float uniqueAreaThreshold = -5f;

        [SerializeField, Tooltip("dirty일 때 Unique를 고를 확률 (0~1)")]
        [Range(0f, 1f)]
        private float uniqueProbability = 0.35f;

        [SerializeField, Tooltip("Relife(IsRetrySession) 직후 Easy 강제 턴 수")]
        private int relifeEasyTurnCount = 1;

        [SerializeField, Tooltip("Unique 동적 생성 샘플 횟수 (트리플 추첨 × 역할 탐색)")]
        private int uniqueSampleCount = 80;

        [Header("Budget")]
        [SerializeField, Tooltip("Normal/Easy 평가 후보 상한")]
        private int maxCandidatesToScore = 16;

        [SerializeField, Tooltip("번들당 완주 시퀀스 탐색 상한")]
        private int maxSequencesPerBundle = 48;

        public IReadOnlyList<AreaBundleEntry> NormalBundles => normalBundles;
        public IReadOnlyList<AreaBundleEntry> EasyBundles => easyBundles;
        public AreaScoreTuning AreaScore => areaScore ??= new AreaScoreTuning();
        public float UniqueAreaThreshold => uniqueAreaThreshold;
        public float UniqueProbability => uniqueProbability;
        public int RelifeEasyTurnCount => relifeEasyTurnCount < 0 ? 0 : relifeEasyTurnCount;
        public int UniqueSampleCount => uniqueSampleCount < 1 ? 1 : uniqueSampleCount;
        public int MaxCandidatesToScore => maxCandidatesToScore < 1 ? 1 : maxCandidatesToScore;
        public int MaxSequencesPerBundle => maxSequencesPerBundle < 1 ? 1 : maxSequencesPerBundle;

#if UNITY_EDITOR
        [ContextMenu("Fill Starter Normal+Easy Bundles")]
        private void FillStarterBundles()
        {
            normalBundles = AreaBundleStarterData.CreateNormal();
            easyBundles = AreaBundleStarterData.CreateEasy();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
