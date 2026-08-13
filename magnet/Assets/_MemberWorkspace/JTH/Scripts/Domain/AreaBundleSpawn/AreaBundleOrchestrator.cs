using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Board;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    /// <summary>
    /// Unique(찬칸≥임계) → Normal 히트맵 → Easy 히트맵 → Easy 가중랜덤.
    /// 접대·올클·Clean체인·Area MaxArea·Death배제 없음.
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

        public AreaBundleSelectionResult Select(
            BoardGrid board,
            int turnIndex,
            bool isRetrySession,
            int currentScore = 0)
        {
            float emptyPenalty = _pool.ResolveEmptyHeatPenalty(currentScore);

            if (isRetrySession && turnIndex < _pool.RelifeEasyTurnCount)
            {
                return SelectEasy(board, "Relife Easy 강제", emptyPenalty);
            }

            int occupied = CountOccupied(board);
            bool dirty = occupied >= _pool.UniqueMinOccupied;
            bool canTryUnique = dirty && _rng.NextDouble() < _pool.UniqueProbability;

            if (canTryUnique)
            {
                AreaBundleSelectionResult unique = TrySelectUniqueDynamic(board);
                if (unique != null)
                {
                    return unique;
                }

                return SelectNormalOrEasy(
                    board, $"Unique 생성 실패 · occ={occupied} → Normal", emptyPenalty);
            }

            string reason = dirty
                ? $"dirty(occ={occupied}) but skip Unique (p={_pool.UniqueProbability:F2}) → Normal"
                : $"Normal 게이트 · occ={occupied}";
            return SelectNormalOrEasy(board, reason, emptyPenalty);
        }

        private AreaBundleSelectionResult TrySelectUniqueDynamic(BoardGrid board)
        {
            UniqueUnlockGenerator.Result gen = UniqueUnlockGenerator.TryGenerate(
                board,
                _rng,
                _pool.UniqueSampleCount,
                id => _pool.GetShapeWeight(id, ShapeWeightProfile.Unique));
            if (gen == null)
            {
                return null;
            }

            return new AreaBundleSelectionResult(
                gen.Pieces,
                gen.Ids,
                AreaBundleTier.Unique,
                bundleId: $"unlock_{gen.Ids[0]}_{gen.Ids[1]}_{gen.Ids[2]}",
                heatScore: float.NaN,
                isKillHand: false,
                reason: gen.Reason,
                explainSteps: gen.ExplainSteps,
                profile: ShapeWeightProfile.Unique);
        }

        private AreaBundleSelectionResult SelectNormalOrEasy(
            BoardGrid board,
            string reasonPrefix,
            float emptyPenalty)
        {
            IReadOnlyList<AreaBundleEntry> normal =
                ResolveList(_pool.NormalBundles, AreaBundleStarterData.CreateNormal());
            AreaBundleSelectionResult picked = TrySelectByHeatmap(
                board, normal, AreaBundleTier.Normal, reasonPrefix, ShapeWeightProfile.Main, emptyPenalty);
            if (picked != null)
            {
                return picked;
            }

            return SelectEasy(board, $"{reasonPrefix} → Easy 폴백", emptyPenalty);
        }

        private AreaBundleSelectionResult SelectEasy(
            BoardGrid board,
            string reasonPrefix,
            float emptyPenalty)
        {
            IReadOnlyList<AreaBundleEntry> easy =
                ResolveList(_pool.EasyBundles, AreaBundleStarterData.CreateEasy());
            AreaBundleSelectionResult picked = TrySelectByHeatmap(
                board, easy, AreaBundleTier.Easy, reasonPrefix, ShapeWeightProfile.Main, emptyPenalty);
            if (picked != null)
            {
                return picked;
            }

            AreaBundleEntry forced = PickWeighted(easy);
            List<IReadOnlyList<Vector2Int>> pieces = AreaBundlePieces.Build(forced);
            bool seekAllClear = CountOccupied(board) > 0;
            HeatmapHandScorer.ScoreBest(
                board, pieces, out List<AreaBundleExplainStep> explain, out _, seekAllClear, emptyPenalty);
            return new AreaBundleSelectionResult(
                pieces,
                forced.Ids,
                AreaBundleTier.Easy,
                forced.BundleId,
                heatScore: float.NaN,
                isKillHand: true,
                reason: $"{reasonPrefix} · Easy 후보 없음 → 가중랜덤 {forced.BundleId}",
                explainSteps: explain,
                profile: ShapeWeightProfile.Main);
        }

        private AreaBundleSelectionResult TrySelectByHeatmap(
            BoardGrid board,
            IReadOnlyList<AreaBundleEntry> list,
            AreaBundleTier tier,
            string reasonPrefix,
            ShapeWeightProfile profile,
            float emptyPenalty)
        {
            List<AreaBundleEntry> candidates = SampleCandidates(list);
            if (candidates.Count == 0)
            {
                return null;
            }

            bool seekAllClear = CountOccupied(board) > 0;
            AreaBundleEntry bestEntry = null;
            List<IReadOnlyList<Vector2Int>> bestPieces = null;
            List<AreaBundleExplainStep> bestExplain = null;
            float bestScore = float.NegativeInfinity;

            for (int i = 0; i < candidates.Count; ++i)
            {
                AreaBundleEntry entry = candidates[i];
                List<IReadOnlyList<Vector2Int>> pieces = AreaBundlePieces.Build(entry);
                float score = HeatmapHandScorer.ScoreBest(
                    board, pieces, out List<AreaBundleExplainStep> explain, out bool allCleared, seekAllClear,
                    emptyPenalty);
                if (allCleared)
                {
                    return new AreaBundleSelectionResult(
                        pieces,
                        entry.Ids,
                        tier,
                        entry.BundleId,
                        heatScore: score,
                        isKillHand: false,
                        reason: $"{reasonPrefix} · AllClear bundle={entry.BundleId} heat={score:F0} emptyPen={emptyPenalty:F2}",
                        explainSteps: explain,
                        profile: profile);
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestEntry = entry;
                    bestPieces = pieces;
                    bestExplain = explain;
                }
            }

            if (bestEntry == null)
            {
                return null;
            }

            return new AreaBundleSelectionResult(
                bestPieces,
                bestEntry.Ids,
                tier,
                bestEntry.BundleId,
                heatScore: bestScore,
                isKillHand: false,
                reason: $"{reasonPrefix} · Heatmap bundle={bestEntry.BundleId} score={bestScore:F0} emptyPen={emptyPenalty:F2}",
                explainSteps: bestExplain,
                profile: profile);
        }

        private List<AreaBundleEntry> SampleCandidates(IReadOnlyList<AreaBundleEntry> list)
        {
            List<AreaBundleEntry> copy = new(list.Count);
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] != null)
                {
                    copy.Add(list[i]);
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
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] != null)
                {
                    total += list[i].Weight;
                }
            }

            if (total <= 0)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i] != null)
                    {
                        return list[i];
                    }
                }

                return list[0];
            }

            int roll = _rng.Next(total);
            for (int i = 0; i < list.Count; ++i)
            {
                AreaBundleEntry e = list[i];
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
