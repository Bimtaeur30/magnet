using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    public sealed class AreaBundleOrchestrator
    {
        private readonly struct ScoredCandidate
        {
            public AreaBundleEntry Entry { get; }
            public List<IReadOnlyList<Vector2Int>> Pieces { get; }
            public int TotalClears { get; }
            public bool BoardEmptied { get; }
            public float PredictedArea { get; }
            public int SequenceCount { get; }

            public ScoredCandidate(
                AreaBundleEntry entry,
                List<IReadOnlyList<Vector2Int>> pieces,
                int totalClears,
                bool boardEmptied,
                float predictedArea,
                int sequenceCount)
            {
                Entry = entry;
                Pieces = pieces;
                TotalClears = totalClears;
                BoardEmptied = boardEmptied;
                PredictedArea = predictedArea;
                SequenceCount = sequenceCount;
            }
        }

        private readonly AreaBundlePoolSO _pool;
        private readonly Random _rng;
        private int _allClearCooldownRemaining;

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
            AreaBundleSelectionResult picked = TrySelectNormalPriority(board, boardArea, normal, reasonPrefix);
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

        private AreaBundleSelectionResult TrySelectNormalPriority(
            BoardGrid board,
            float boardArea,
            IReadOnlyList<AreaBundleEntry> list,
            string reasonPrefix)
        {
            bool onCooldown = _allClearCooldownRemaining > 0;
            if (_allClearCooldownRemaining > 0)
            {
                --_allClearCooldownRemaining;
            }

            List<ScoredCandidate> scored = ScoreSurvivors(board, list);
            if (scored.Count == 0)
            {
                return null;
            }

            bool boardEmpty = CountOccupied(board) == 0;
            bool canTryAllClear = !boardEmpty && !onCooldown;

            if (canTryAllClear)
            {
                List<ScoredCandidate> allClear = FilterBoardEmptied(scored);
                if (allClear.Count > 0)
                {
                    if (_rng.NextDouble() < _pool.AllClearProbability)
                    {
                        ScoredCandidate pick = PickMaxClears(allClear);
                        _allClearCooldownRemaining = _pool.AllClearCooldownTurns;
                        return ToResult(
                            pick,
                            AreaBundleTier.AllClear,
                            boardArea,
                            $"{reasonPrefix} · AllClear p={_pool.AllClearProbability:P0} bundle={pick.Entry.BundleId}"
                            + $" clears={pick.TotalClears}");
                    }

                    scored = ExcludeBoardEmptied(scored);
                    if (scored.Count == 0)
                    {
                        return null;
                    }
                }
            }

            int hardMin = _pool.MultiClearHardMinLines;
            List<ScoredCandidate> multi = FilterMinClears(scored, hardMin);
            if (multi.Count > 0)
            {
                ScoredCandidate pick = PickMaxClears(multi);
                return ToResult(
                    pick,
                    AreaBundleTier.MultiClear,
                    boardArea,
                    $"{reasonPrefix} · MultiClear clears={pick.TotalClears} (≥{hardMin})"
                    + $" bundle={pick.Entry.BundleId}");
            }

            ScoredCandidate areaPick = PickMaxArea(scored);
            return ToResult(
                areaPick,
                AreaBundleTier.Normal,
                boardArea,
                $"{reasonPrefix} · maxArea bundle={areaPick.Entry.BundleId} pred={areaPick.PredictedArea:F1}");
        }

        private List<ScoredCandidate> ScoreSurvivors(BoardGrid board, IReadOnlyList<AreaBundleEntry> list)
        {
            List<AreaBundleEntry> candidates = SampleCandidates(list);
            List<ScoredCandidate> scored = new(candidates.Count);

            foreach (AreaBundleEntry entry in candidates)
            {
                List<IReadOnlyList<Vector2Int>> pieces = AreaBundlePieces.Build(entry);
                SequenceOutcomeEstimator.SequenceOutcome outcome = SequenceOutcomeEstimator.Estimate(
                    board, pieces, _pool.OutcomeBeamWidth);
                if (!outcome.SequenceFound)
                {
                    continue;
                }

                float predicted = AreaBundleMetrics.MaxAreaAfterFullSequence(
                    board, pieces, _pool.MaxSequencesPerBundle, out bool any, _pool.AreaScore);
                if (!any)
                {
                    continue;
                }

                int seq = AreaBundleMetrics.CountSequences(board, pieces, _pool.MaxSequencesPerBundle);
                scored.Add(new ScoredCandidate(
                    entry, pieces, outcome.TotalClears, outcome.BoardEmptied, predicted, seq));
            }

            return scored;
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

        private static AreaBundleSelectionResult ToResult(
            ScoredCandidate pick,
            AreaBundleTier tier,
            float boardArea,
            string reason)
        {
            return new AreaBundleSelectionResult(
                pick.Pieces,
                pick.Entry.Ids,
                tier,
                pick.Entry.BundleId,
                boardArea,
                pick.PredictedArea,
                pick.SequenceCount,
                deathCount: 0,
                isKillHand: false,
                reason: reason);
        }

        private static List<ScoredCandidate> FilterBoardEmptied(List<ScoredCandidate> scored)
        {
            List<ScoredCandidate> list = new();
            foreach (ScoredCandidate c in scored)
            {
                if (c.BoardEmptied)
                {
                    list.Add(c);
                }
            }

            return list;
        }

        private static List<ScoredCandidate> ExcludeBoardEmptied(List<ScoredCandidate> scored)
        {
            List<ScoredCandidate> list = new();
            foreach (ScoredCandidate c in scored)
            {
                if (!c.BoardEmptied)
                {
                    list.Add(c);
                }
            }

            return list;
        }

        private static List<ScoredCandidate> FilterMinClears(List<ScoredCandidate> scored, int minLines)
        {
            List<ScoredCandidate> list = new();
            foreach (ScoredCandidate c in scored)
            {
                if (c.TotalClears >= minLines)
                {
                    list.Add(c);
                }
            }

            return list;
        }

        private static ScoredCandidate PickMaxClears(List<ScoredCandidate> list)
        {
            ScoredCandidate best = list[0];
            for (int i = 1; i < list.Count; ++i)
            {
                ScoredCandidate c = list[i];
                if (c.TotalClears > best.TotalClears
                    || (c.TotalClears == best.TotalClears && c.PredictedArea > best.PredictedArea))
                {
                    best = c;
                }
            }

            return best;
        }

        private static ScoredCandidate PickMaxArea(List<ScoredCandidate> list)
        {
            ScoredCandidate best = list[0];
            for (int i = 1; i < list.Count; ++i)
            {
                if (list[i].PredictedArea > best.PredictedArea)
                {
                    best = list[i];
                }
            }

            return best;
        }

        private static int CountOccupied(BoardGrid board)
        {
            int size = board.BoardSize;
            int occupied = 0;
            Vector2Int cell = Vector2Int.zero;
            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    cell.x = x;
                    cell.y = y;
                    if (board.IsOccupied(cell))
                    {
                        ++occupied;
                    }
                }
            }

            return occupied;
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
