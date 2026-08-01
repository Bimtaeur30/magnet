using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Generation;
using JTH.Scripts.Domain.BlockSelection.Health;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using Random = System.Random;

namespace JTH.Scripts.Domain.HybridSpawn
{
    /// <summary>
    /// 접대 실시간 생성의 42-ID 이식판 (구 HospitalityGenerator와 동일 원칙 — SPEC §10).
    /// 기회 게이트(OpportunityScorer) 통과 시에만, 완벽 플레이 시 클리어가 충분한 트리플을
    /// 품질 가중으로 추첨한다. 실패 시 null → fallthrough.
    /// </summary>
    public static class HybridHospitalityGenerator
    {
        /// <summary>후보가 이만큼 모이면 샘플링 조기 종료 (품질 가중 추첨에는 충분).</summary>
        private const int MaxCandidates = 8;

        private readonly struct Candidate
        {
            public int[] Triple { get; }
            public int Quality { get; }

            public Candidate(int[] triple, int quality)
            {
                Triple = triple;
                Quality = quality;
            }
        }

        public static int[] TryGenerate(
            BoardGrid board,
            BoardHealthResult health,
            HybridPiecePool pool,
            HybridTuningSO tuning,
            Random rng,
            int[] avoidTriple)
        {
            if (OpportunityScorer.Score(board, health, tuning) < tuning.OpportunityHighThreshold)
            {
                return null;
            }

            List<Candidate> candidates = new();

            for (int sample = 0; sample < tuning.HospitalitySampleCount; ++sample)
            {
                int[] triple = pool.SampleTriple(rng, avoidTriple);
                if (triple == null)
                {
                    return null;
                }

                SequenceOutcomeEstimator.SequenceOutcome outcome = SequenceOutcomeEstimator.Estimate(
                    board, HybridPiecePool.BuildPieces(triple), tuning.OutcomeBeamWidth);

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

                candidates.Add(new Candidate(triple, quality));
                if (candidates.Count >= MaxCandidates)
                {
                    break;
                }
            }

            return candidates.Count == 0 ? null : PickWeightedByQuality(candidates, rng);
        }

        private static int[] PickWeightedByQuality(List<Candidate> candidates, Random rng)
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
                    return candidate.Triple;
                }
            }

            return candidates[^1].Triple;
        }
    }
}
