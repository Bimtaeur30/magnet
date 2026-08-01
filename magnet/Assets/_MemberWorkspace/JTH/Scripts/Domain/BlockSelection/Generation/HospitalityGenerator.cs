using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Bundles;
using JTH.Scripts.Domain.BlockSelection.Health;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.BlockSelection.Generation
{
    /// <summary>
    /// 접대 실시간 생성 (SPEC §10). 강한 기회일 때만, 억지 블록 없이(가중치 0으로 배제),
    /// 완벽 플레이 시 클리어가 충분한 조합만 후보로 삼는다. 실패 시 null → fallthrough.
    /// </summary>
    public static class HospitalityGenerator
    {
        /// <summary>후보가 이만큼 모이면 샘플링 조기 종료 (품질 가중 추첨에는 충분).</summary>
        private const int MaxCandidates = 8;

        private readonly struct Candidate
        {
            public List<IReadOnlyList<Vector2Int>> Pieces { get; }
            public int Quality { get; }

            public Candidate(List<IReadOnlyList<Vector2Int>> pieces, int quality)
            {
                Pieces = pieces;
                Quality = quality;
            }
        }

        public static List<IReadOnlyList<Vector2Int>> TryGenerate(
            BoardGrid board,
            BoardHealthResult health,
            IReadOnlyList<WeightedShape> pool,
            BlockSelectionTuningSO tuning,
            Random rng)
        {
            if (OpportunityScorer.Score(board, health, tuning) < tuning.OpportunityHighThreshold)
            {
                return null;
            }

            List<Candidate> candidates = new();

            for (int sample = 0; sample < tuning.HospitalitySampleCount; ++sample)
            {
                List<IReadOnlyList<Vector2Int>> pieces = ShapeSampler.Sample3Rotated(pool, rng);
                if (pieces == null)
                {
                    return null;
                }

                SequenceOutcomeEstimator.SequenceOutcome outcome =
                    SequenceOutcomeEstimator.Estimate(board, pieces, tuning.OutcomeBeamWidth);

                if (!outcome.SequenceFound)
                {
                    continue;
                }

                // 올클리어는 라인 수 이상의 가치 — 보너스 2라인 상당
                int quality = outcome.TotalClears + (outcome.BoardEmptied ? 2 : 0);
                if (quality < tuning.HospitalityMinQualityClears)
                {
                    continue;
                }

                candidates.Add(new Candidate(pieces, quality));
                if (candidates.Count >= MaxCandidates)
                {
                    break;
                }
            }

            return candidates.Count == 0 ? null : PickWeightedByQuality(candidates, rng);
        }

        private static List<IReadOnlyList<Vector2Int>> PickWeightedByQuality(List<Candidate> candidates, Random rng)
        {
            int totalQuality = 0;
            foreach (Candidate candidate in candidates)
            {
                totalQuality += candidate.Quality;
            }

            int roll = rng.Next(totalQuality);
            int accumulated = 0;

            foreach (Candidate candidate in candidates)
            {
                accumulated += candidate.Quality;
                if (roll < accumulated)
                {
                    return candidate.Pieces;
                }
            }

            return candidates[^1].Pieces;
        }
    }
}
