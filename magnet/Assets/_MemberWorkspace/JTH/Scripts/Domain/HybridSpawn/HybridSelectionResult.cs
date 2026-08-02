using System.Collections.Generic;
using JTH.Scripts.Domain.BlockBlast;
using JTH.Scripts.Domain.BlockSelection.Health;
using JTH.Scripts.Domain.BlockSelection.Solution;
using UnityEngine;

namespace JTH.Scripts.Domain.HybridSpawn
{
    /// <summary>
    /// 하이브리드 스폰 1회 선택 결과. Pieces가 게임 파이프라인 출력이고 나머지는 진단·UI 훅용.
    /// </summary>
    public sealed class HybridSelectionResult
    {
        public HybridTier Tier { get; }

        /// <summary>42-ID 카탈로그 기준 블록 ID 3개 (슬롯 순서).</summary>
        public IReadOnlyList<int> BlockIds { get; }

        public List<IReadOnlyList<Vector2Int>> Pieces { get; }

        /// <summary>Pressure만 non-null — 배치별 정답 매칭(엄지척 UI 데이터)용.</summary>
        public UniqueSolution UniqueSolution { get; }

        /// <summary>Pressure intent — 라운드 무사 완료 시 brilliant escape.</summary>
        public bool IsBrilliantEscapeCandidate => Tier == HybridTier.Pressure;

        public float HealthScore { get; }
        public HealthZone Zone { get; }
        public float Blame { get; }

        /// <summary>BaseChain일 때만 non-null — 핸드오프 체인 진단(알고리즘 ID 등).</summary>
        public BlockBlastSelection BaseSelection { get; }

        /// <summary>선택 이유 + 상위 티어 스킵 경과 (진단 로그용).</summary>
        public string Reason { get; }

        public HybridSelectionResult(
            HybridTier tier,
            IReadOnlyList<int> blockIds,
            List<IReadOnlyList<Vector2Int>> pieces,
            UniqueSolution uniqueSolution,
            float healthScore,
            HealthZone zone,
            float blame,
            BlockBlastSelection baseSelection,
            string reason)
        {
            Tier = tier;
            BlockIds = blockIds;
            Pieces = pieces;
            UniqueSolution = uniqueSolution;
            HealthScore = healthScore;
            Zone = zone;
            Blame = blame;
            BaseSelection = baseSelection;
            Reason = reason;
        }
    }
}
