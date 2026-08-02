using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Board;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    /// <summary>
    /// cascade: Relife1턴 Easy / dirty·pUnique Unique(동적) → Normal → Easy.
    /// Unique는 번들이 아니라 UniqueUnlockGenerator.
    /// </summary>
    public sealed class AreaBundleOrchestrator
    {
        private readonly AreaBundlePoolSO _pool;
        private readonly Random _rng;

        public AreaBundleOrchestrator(AreaBundlePoolSO pool, Random rng = null)
        {
            _pool = pool;
            _rng = rng ?? new Random();
        }

        public AreaBundleSelectionResult Select(BoardGrid board, int turnIndex, bool isRetrySession)
        {
            float boardArea = AreaScoreCalculator.ScoreTotal(board, _pool.AreaScore);

            if (isRetrySession && turnIndex < _pool.RelifeEasyTurnCount)
            {
                return SelectEasy(board, boardArea, "Relife Easy 강제");
            }

            bool dirty = boardArea <= _pool.UniqueAreaThreshold;
            bool canTryUnique = dirty && _rng.NextDouble() < _pool.UniqueProbability;

            if (canTryUnique)
            {
                AreaBundleSelectionResult unique = TrySelectUniqueDynamic(board, boardArea);
                if (unique != null)
                {
                    return unique;
                }

                return SelectNormalOrEasy(board, boardArea, "Unique 생성 실패 → Normal");
            }

            string reason = dirty
                ? $"dirty but skip Unique (p={_pool.UniqueProbability:F2}) → Normal"
                : "Normal 게이트";
            return SelectNormalOrEasy(board, boardArea, reason);
        }

        private AreaBundleSelectionResult TrySelectUniqueDynamic(BoardGrid board, float boardArea)
        {
            UniqueUnlockGenerator.Result gen = UniqueUnlockGenerator.TryGenerate(
                board, _rng, _pool.UniqueSampleCount);
            if (gen == null)
            {
                return null;
            }

            return new AreaBundleSelectionResult(
                gen.Pieces,
                gen.Ids,
                AreaBundleTier.Unique,
                bundleId: $"unlock_{gen.Ids[0]}_{gen.Ids[1]}_{gen.Ids[2]}",
                boardArea,
                predictedAreaScore: float.NaN,
                sequenceCount: 0,
                deathCount: 0,
                isKillHand: false,
                reason: gen.Reason);
        }

        private AreaBundleSelectionResult SelectNormalOrEasy(BoardGrid board, float boardArea, string reasonPrefix)
        {
            IReadOnlyList<AreaBundleEntry> normal = ResolveList(_pool.NormalBundles, AreaBundleStarterData.CreateNormal());
            AreaBundleSelectionResult picked = TrySelectByMaxArea(board, boardArea, normal, AreaBundleTier.Normal, reasonPrefix);
            if (picked != null)
            {
                return picked;
            }

            return SelectEasy(board, boardArea, $"{reasonPrefix} → Easy 폴백");
        }

        private AreaBundleSelectionResult SelectEasy(BoardGrid board, float boardArea, string reasonPrefix)
        {
            IReadOnlyList<AreaBundleEntry> easy = ResolveList(_pool.EasyBundles, AreaBundleStarterData.CreateEasy());
            AreaBundleSelectionResult picked = TrySelectByMaxArea(board, boardArea, easy, AreaBundleTier.Easy, reasonPrefix);
            if (picked != null)
            {
                return picked;
            }

            AreaBundleEntry forced = PickWeighted(easy);
            List<IReadOnlyList<Vector2Int>> pieces = AreaBundlePieces.Build(forced);
            return new AreaBundleSelectionResult(
                pieces,
                forced.Ids,
                AreaBundleTier.Easy,
                forced.BundleId,
                boardArea,
                predictedAreaScore: float.NaN,
                sequenceCount: 0,
                deathCount: 0,
                isKillHand: true,
                reason: $"{reasonPrefix} · Easy 완주 없음 → 가중랜덤 {forced.BundleId}");
        }

        private AreaBundleSelectionResult TrySelectByMaxArea(
            BoardGrid board,
            float boardArea,
            IReadOnlyList<AreaBundleEntry> list,
            AreaBundleTier tier,
            string reasonPrefix)
        {
            List<AreaBundleEntry> candidates = SampleCandidates(list);
            AreaBundleEntry best = null;
            float bestArea = float.NegativeInfinity;
            int bestSeq = 0;
            List<IReadOnlyList<Vector2Int>> bestPieces = null;

            foreach (AreaBundleEntry entry in candidates)
            {
                List<IReadOnlyList<Vector2Int>> pieces = AreaBundlePieces.Build(entry);
                if (!AreaBundleMetrics.CanSurvive(board, pieces))
                {
                    continue;
                }

                float predicted = AreaBundleMetrics.MaxAreaAfterFullSequence(
                    board, pieces, _pool.MaxSequencesPerBundle, out bool any, _pool.AreaScore);
                if (!any)
                {
                    continue;
                }

                if (predicted > bestArea)
                {
                    bestArea = predicted;
                    best = entry;
                    bestPieces = pieces;
                    bestSeq = AreaBundleMetrics.CountSequences(board, pieces, _pool.MaxSequencesPerBundle);
                }
            }

            if (best == null)
            {
                return null;
            }

            return new AreaBundleSelectionResult(
                bestPieces,
                best.Ids,
                tier,
                best.BundleId,
                boardArea,
                bestArea,
                bestSeq,
                deathCount: 0,
                isKillHand: false,
                reason: $"{reasonPrefix} · maxArea bundle={best.BundleId} pred={bestArea:F1}");
        }

        private List<AreaBundleEntry> SampleCandidates(IReadOnlyList<AreaBundleEntry> list)
        {
            List<AreaBundleEntry> copy = new(list.Count);
            foreach (AreaBundleEntry e in list)
            {
                if (e != null)
                {
                    copy.Add(e);
                }
            }

            Shuffle(copy);
            int take = Mathf.Min(_pool.MaxCandidatesToScore, copy.Count);
            if (copy.Count > take)
            {
                copy.RemoveRange(take, copy.Count - take);
            }

            return copy;
        }

        private AreaBundleEntry PickWeighted(IReadOnlyList<AreaBundleEntry> list)
        {
            int total = 0;
            foreach (AreaBundleEntry e in list)
            {
                if (e != null)
                {
                    total += e.Weight;
                }
            }

            if (total <= 0)
            {
                return list[0];
            }

            int roll = _rng.Next(total);
            foreach (AreaBundleEntry e in list)
            {
                if (e == null)
                {
                    continue;
                }

                roll -= e.Weight;
                if (roll < 0)
                {
                    return e;
                }
            }

            return list[list.Count - 1];
        }

        private void Shuffle(List<AreaBundleEntry> list)
        {
            for (int i = list.Count - 1; i > 0; --i)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static IReadOnlyList<AreaBundleEntry> ResolveList(
            IReadOnlyList<AreaBundleEntry> configured,
            List<AreaBundleEntry> starter)
        {
            if (configured != null && configured.Count > 0)
            {
                return configured;
            }

            return starter;
        }
    }
}
