using System.Collections.Generic;
using JTH.Scripts.Domain.BlockSelection.Health;
using JTH.Scripts.Domain.BlockSelection.Solution;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection
{
    /// <summary>
    /// 매 리필의 선택 결과 (SPEC §16.3). 피스 3개 + 로깅·UI 훅용 진단 데이터.
    /// </summary>
    public sealed class BlockSelectionResult
    {
        public SelectionTier Tier { get; }

        /// <summary>번들 선택이면 번들 id, 실시간 생성이면 null.</summary>
        public string BundleId { get; }

        /// <summary>실시간 생성(Hospitality/Pressure/Fallback)이면 true.</summary>
        public bool WasGenerated => BundleId == null;

        public float HealthScore { get; }
        public HealthZone Zone { get; }
        public float Blame { get; }

        /// <summary>Pressure intent — 라운드 무사 완료 시 brilliant escape (SPEC §11.4).</summary>
        public bool IsBrilliantEscapeCandidate => Tier == SelectionTier.Pressure;

        /// <summary>Pressure만 non-null (SPEC §11.5, 엄지척 UI 판정용).</summary>
        public UniqueSolution UniqueSolution { get; }

        public List<IReadOnlyList<Vector2Int>> Pieces { get; }

        public BlockSelectionResult(
            SelectionTier tier,
            string bundleId,
            List<IReadOnlyList<Vector2Int>> pieces,
            UniqueSolution uniqueSolution,
            float healthScore,
            HealthZone zone,
            float blame)
        {
            Tier = tier;
            BundleId = bundleId;
            Pieces = pieces;
            UniqueSolution = uniqueSolution;
            HealthScore = healthScore;
            Zone = zone;
            Blame = blame;
        }
    }
}
