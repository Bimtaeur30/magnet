using System.Collections.Generic;
using System.Diagnostics;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using UnityEngine;
using Debug = UnityEngine.Debug;
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
        private AreaBundleSelectionResult _queuedCleanChain;

        public AreaBundleOrchestrator(AreaBundlePoolSO pool, Random rng = null)
        {
            _pool = pool;
            _rng = rng ?? new Random();
        }

        public AreaBundleSelectionResult Select(BoardGrid board, int turnIndex, bool isRetrySession)
        {
            Stopwatch total = Stopwatch.StartNew();
            AreaBundleSelectionResult result = SelectCore(board, turnIndex, isRetrySession);
            LogPerf($"Select total tier={result?.Tier} bundle={result?.BundleId}", total.Elapsed.TotalMilliseconds);
            return result;
        }

        private AreaBundleSelectionResult SelectCore(BoardGrid board, int turnIndex, bool isRetrySession)
        {
            if (_queuedCleanChain != null)
            {
                AreaBundleSelectionResult queued = _queuedCleanChain;
                _queuedCleanChain = null;

                if (CanLineClearOnBoard(board, queued.Pieces))
                {
                    LogGate($"Clean 체이닝 예약 패 지급 · bundle={queued.BundleId}"
                        + $" blocks=[{string.Join(",", queued.BlockIds)}]");
                    return queued;
                }

                LogGate($"Clean 체이닝 폐기 · 현재 보드에서 라인클리어 불가"
                    + $" · bundle={queued.BundleId} → 일반 뽑기");
            }

            float boardArea = AreaScoreCalculator.ScoreTotal(board, _pool.AreaScore);

            if (isRetrySession && turnIndex < _pool.RelifeEasyTurnCount)
            {
                return SelectEasy(board, boardArea, "Relife Easy 강제");
            }

            bool dirty = boardArea <= _pool.UniqueAreaThreshold;
            bool canTryUnique = dirty && _rng.NextDouble() < _pool.UniqueProbability;

            if (canTryUnique)
            {
                Stopwatch uniqueSw = Stopwatch.StartNew();
                AreaBundleSelectionResult unique = TrySelectUniqueDynamic(board, boardArea);
                LogPerf("Unique", uniqueSw.Elapsed.TotalMilliseconds);
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

        private AreaBundleSelectionResult SelectWeightedRandomNormal(BoardGrid board, float boardArea, string reasonPrefix)
        {
            IReadOnlyList<AreaBundleEntry> normal = ResolveList(_pool.NormalBundles, AreaBundleStarterData.CreateNormal());
            AreaBundleEntry picked = PickWeighted(normal);
            List<IReadOnlyList<Vector2Int>> pieces = AreaBundlePieces.Build(picked);
            LogGate($"올클 상태 가중랜덤 · bundle={picked.BundleId} blocks=[{string.Join(",", picked.Ids)}]");
            return new AreaBundleSelectionResult(
                pieces,
                picked.Ids,
                AreaBundleTier.Normal,
                picked.BundleId,
                boardArea,
                predictedAreaScore: float.NaN,
                sequenceCount: 0,
                isKillHand: false,
                reason: $"{reasonPrefix} · 올클 상태 가중랜덤 · bundle={picked.BundleId}",
                explainSteps: CaptureExplain(board, pieces),
                profile: ShapeWeightProfile.Main);
        }

        private bool CanLineClearOnBoard(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            SequenceOutcomeEstimator.SequenceOutcome outcome = SequenceOutcomeEstimator.Estimate(
                board, pieces, _pool.OutcomeBeamWidth);
            return outcome.SequenceFound && outcome.TotalClears >= 1;
        }

        private AreaBundleSelectionResult TrySelectUniqueDynamic(BoardGrid board, float boardArea)
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
                boardArea,
                predictedAreaScore: float.NaN,
                sequenceCount: 0,
                isKillHand: false,
                reason: gen.Reason,
                explainSteps: gen.ExplainSteps,
                profile: ShapeWeightProfile.Unique);
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

            int occupied = CountOccupied(board);
            bool boardEmpty = occupied == 0;
            if (boardEmpty)
            {
                LogGate("올클 상태(빈 보드) → Normal 가중랜덤");
                return SelectWeightedRandomNormal(board, boardArea, reasonPrefix);
            }

            bool canTryAllClear = !onCooldown
                && occupied <= _pool.AllClearMaxOccupied
                && _pool.AllClearBundles != null
                && _pool.AllClearBundles.Count > 0;

            if (canTryAllClear)
            {
                Stopwatch allClearSw = Stopwatch.StartNew();
                AreaBundleSelectionResult allClear = TrySelectAllClearExact(board, boardArea, reasonPrefix);
                LogPerf("AllClear Exact", allClearSw.Elapsed.TotalMilliseconds);
                if (allClear != null)
                {
                    double allClearRoll = _rng.NextDouble();
                    if (allClearRoll < _pool.AllClearProbability)
                    {
                        LogGate($"올클 Exact 확률 통과 · roll={allClearRoll:F2} < p={_pool.AllClearProbability:P0}"
                            + $" · bundle={allClear.BundleId}");
                        _allClearCooldownRemaining = _pool.AllClearCooldownTurns;
                        return allClear;
                    }

                    LogGate($"올클 Exact 확률 낙첨 · roll={allClearRoll:F2} ≥ p={_pool.AllClearProbability:P0}"
                        + $" · bundle={allClear.BundleId} → 다음 게이트");
                }
                else
                {
                    LogGate($"올클 Exact 후보 없음 · occ≤{_pool.AllClearMaxOccupied}");
                }
            }
            else
            {
                LogGate(onCooldown
                    ? "올클 스킵 · 쿨다운"
                    : $"올클 스킵 · occ>{_pool.AllClearMaxOccupied} (occ={occupied})");
            }

            Stopwatch hospitalitySw = Stopwatch.StartNew();
            AreaBundleSelectionResult hospitality = TrySelectHospitality(
                board, boardArea, list, reasonPrefix, out List<HospitalityHole> hospitalityHoles);
            LogPerf("Hospitality", hospitalitySw.Elapsed.TotalMilliseconds);
            if (hospitality != null)
            {
                double hospRoll = _rng.NextDouble();
                if (hospRoll < _pool.HospitalityProbability)
                {
                    bool threeCellOnly = OpportunityDetector.IsHalfWeightOnlyFit(
                        hospitality.BlockIds, hospitalityHoles);
                    if (!threeCellOnly)
                    {
                        LogGate($"접대 확률 통과 · roll={hospRoll:F2} < p={_pool.HospitalityProbability:P0}"
                            + $" · bundle={hospitality.BundleId}");
                        return hospitality;
                    }

                    double threeRoll = _rng.NextDouble();
                    if (threeRoll < _pool.HospitalityThreeCellProbability)
                    {
                        LogGate($"접대 3칸 추가확률 통과 · roll={threeRoll:F2}"
                            + $" < p={_pool.HospitalityThreeCellProbability:P0}"
                            + $" · bundle={hospitality.BundleId}");
                        return hospitality;
                    }

                    LogGate($"접대 3칸 추가확률 낙첨 · roll={threeRoll:F2}"
                        + $" ≥ p={_pool.HospitalityThreeCellProbability:P0} → 다음 게이트");
                }
                else
                {
                    LogGate($"접대 확률 낙첨 · roll={hospRoll:F2} ≥ p={_pool.HospitalityProbability:P0}"
                        + $" · bundle={hospitality.BundleId} → 다음 게이트");
                }
            }
            else
            {
                LogGate("접대 후보 없음");
            }

            ShapeWeightProfile profile = boardArea > _pool.SurvivalAreaMax
                ? ShapeWeightProfile.Clean
                : ShapeWeightProfile.Main;
            if (profile == ShapeWeightProfile.Clean)
            {
                LogGate($"Normal 모드 Clean 통과 · boardArea={boardArea:F1}"
                    + $" > survivalMax={_pool.SurvivalAreaMax:F1}");
            }
            else
            {
                LogGate($"Normal 모드 Main 진입 · boardArea={boardArea:F1}"
                    + $" ≤ survivalMax={_pool.SurvivalAreaMax:F1}");
            }

            Stopwatch scoreSw = Stopwatch.StartNew();
            List<ScoredCandidate> scored = ScoreSurvivors(board, list, profile);
            LogPerf($"ScoreSurvivors n={scored.Count}", scoreSw.Elapsed.TotalMilliseconds);
            if (scored.Count == 0)
            {
                LogGate("Normal Area 후보 없음 · 완주+라인클리어≥1");
                return null;
            }

            Stopwatch deathSw = Stopwatch.StartNew();
            AreaBundleSelectionResult areaPick = PickAreaWithDeathReject(
                scored, AreaBundleTier.Normal, board, boardArea, reasonPrefix, profile);
            LogPerf("DeathReject+ToResult", deathSw.Elapsed.TotalMilliseconds);
            if (areaPick != null && profile == ShapeWeightProfile.Clean)
            {
                TryQueueCleanChain(board, list, areaPick);
            }

            return areaPick;
        }

        private void TryQueueCleanChain(
            BoardGrid board,
            IReadOnlyList<AreaBundleEntry> list,
            AreaBundleSelectionResult current)
        {
            Stopwatch chainTotal = Stopwatch.StartNew();
            double chainRoll = _rng.NextDouble();
            if (chainRoll >= _pool.CleanChainProbability)
            {
                LogGate($"Clean 체이닝 확률 낙첨 · roll={chainRoll:F2}"
                    + $" ≥ p={_pool.CleanChainProbability:P0}");
                return;
            }

            LogGate($"Clean 체이닝 확률 통과 · roll={chainRoll:F2}"
                + $" < p={_pool.CleanChainProbability:P0}");

            Stopwatch afterSw = Stopwatch.StartNew();
            SequenceOutcomeEstimator.SequenceOutcome afterOutcome = SequenceOutcomeEstimator.Estimate(
                board, current.Pieces, _pool.OutcomeBeamWidth);
            if (!afterOutcome.SequenceFound || afterOutcome.FinalBoard == null)
            {
                LogPerf("CleanChain afterBest(beam)", afterSw.Elapsed.TotalMilliseconds);
                LogGate("Clean 체이닝 실패 · 최적 시퀀스 보드 없음");
                return;
            }

            BoardGrid afterBest = afterOutcome.FinalBoard;
            float bestArea = AreaScoreCalculator.ScoreTotal(afterBest, _pool.AreaScore);
            LogPerf("CleanChain afterBest(beam)", afterSw.Elapsed.TotalMilliseconds);

            LogGate($"Clean 체이닝 최적 보드 확정 · predArea={bestArea:F1} boardArea={bestArea:F1}");

            Stopwatch scoreSw = Stopwatch.StartNew();
            List<ScoredCandidate> scored = ScoreSurvivors(afterBest, list, ShapeWeightProfile.Clean);
            LogPerf($"CleanChain ScoreSurvivors n={scored.Count}", scoreSw.Elapsed.TotalMilliseconds);
            if (scored.Count == 0)
            {
                LogGate("Clean 체이닝 실패 · 이어질 Area 후보 없음(완주+클리어≥1)");
                return;
            }

            Stopwatch deathSw = Stopwatch.StartNew();
            AreaBundleSelectionResult next = PickAreaWithDeathReject(
                scored,
                AreaBundleTier.Normal,
                afterBest,
                bestArea,
                "Clean chain",
                ShapeWeightProfile.Clean);
            LogPerf("CleanChain DeathReject+ToResult", deathSw.Elapsed.TotalMilliseconds);
            if (next == null)
            {
                LogGate("Clean 체이닝 실패 · 다음 패 선택 실패");
                return;
            }

            _queuedCleanChain = next;
            LogGate($"Clean 체이닝 예약 완료 · next={next.BundleId}"
                + $" blocks=[{string.Join(",", next.BlockIds)}]");
            LogPerf("CleanChain total", chainTotal.Elapsed.TotalMilliseconds);
        }

        private static void LogGate(string message)
        {
            Debug.Log($"<color=#80CBC4>[AreaBundle:Gate] {message}</color>");
        }

        private static void LogPerf(string label, double ms)
        {
            Debug.Log($"<color=#FFAB91>[AreaBundle:Perf] {label}={ms:F1}ms ({ms / 1000.0:F3}s)</color>");
        }

        private AreaBundleSelectionResult TrySelectHospitality(
            BoardGrid board,
            float boardArea,
            IReadOnlyList<AreaBundleEntry> list,
            string reasonPrefix,
            out List<HospitalityHole> holes)
        {
            holes = OpportunityDetector.FindQualifyingHoles(
                board, _pool.HospitalityContourMinFill);
            if (holes.Count == 0)
            {
                return null;
            }

            List<AreaBundleEntry> matching = new();
            foreach (AreaBundleEntry entry in list)
            {
                if (ContainsSmallL(entry))
                {
                    continue;
                }

                if (OpportunityDetector.SumFittingWeight(entry, holes) > 0f)
                {
                    matching.Add(entry);
                }
            }

            if (matching.Count == 0)
            {
                return null;
            }

            List<AreaBundleEntry> candidates = SampleCandidates(matching);
            AreaBundleEntry best = null;
            List<IReadOnlyList<Vector2Int>> bestPieces = null;
            float bestPred = float.NegativeInfinity;
            float bestFitWeight = 0f;
            double beamMs = 0;

            foreach (AreaBundleEntry entry in candidates)
            {
                List<IReadOnlyList<Vector2Int>> pieces = AreaBundlePieces.Build(entry);
                Stopwatch beamSw = Stopwatch.StartNew();
                SequenceOutcomeEstimator.SequenceOutcome outcome = SequenceOutcomeEstimator.Estimate(
                    board, pieces, _pool.OutcomeBeamWidth);
                beamMs += beamSw.Elapsed.TotalMilliseconds;
                if (!outcome.SequenceFound || outcome.FinalBoard == null)
                {
                    continue;
                }

                float predicted = AreaScoreCalculator.ScoreTotal(outcome.FinalBoard, _pool.AreaScore);
                float fitWeight = OpportunityDetector.SumFittingWeight(entry, holes);
                int holeCmp = best == null ? 1 : OpportunityDetector.CompareHoleCoverage(entry, best, holes);
                bool better = best == null
                    || holeCmp > 0
                    || (holeCmp == 0 && predicted > bestPred);

                if (better)
                {
                    best = entry;
                    bestPieces = pieces;
                    bestPred = predicted;
                    bestFitWeight = fitWeight;
                }
            }

            LogPerf($"Hospitality beam candidates={candidates.Count}", beamMs);

            if (best == null)
            {
                return null;
            }

            if (_pool.MaxAreaRefineTopK > 0)
            {
                Stopwatch refineSw = Stopwatch.StartNew();
                float refined = AreaBundleMetrics.MaxAreaAfterFullSequence(
                    board, bestPieces, _pool.MaxSequencesPerBundle, out bool any, _pool.AreaScore);
                LogPerf("Hospitality MaxArea refine winner", refineSw.Elapsed.TotalMilliseconds);
                if (any)
                {
                    bestPred = refined;
                }
            }

            int bestSeq = AreaBundleMetrics.CountSequences(board, bestPieces, _pool.MaxSequencesPerBundle);
            bool threeCellOnly = OpportunityDetector.IsHalfWeightOnlyFit(best.Ids, holes);
            string holeSummary = FormatHoleSummary(holes);
            string pExtra = threeCellOnly
                ? $"×{_pool.HospitalityThreeCellProbability:P0}"
                : string.Empty;
            return new AreaBundleSelectionResult(
                bestPieces,
                best.Ids,
                AreaBundleTier.Hospitality,
                best.BundleId,
                boardArea,
                bestPred,
                bestSeq,
                isKillHand: false,
                reason: $"{reasonPrefix} · Hospitality holes={holeSummary} fitW={bestFitWeight:F1}"
                    + $" p={_pool.HospitalityProbability:P0}{pExtra}"
                    + $" bundle={best.BundleId} pred={bestPred:F1}",
                explainSteps: CaptureExplain(board, bestPieces));
        }

        private static string FormatHoleSummary(IReadOnlyList<HospitalityHole> holes)
        {
            List<string> parts = new(holes.Count);
            for (int i = 0; i < holes.Count; ++i)
            {
                HospitalityHole h = holes[i];
                parts.Add($"{h.Cells.Count}@{h.ContourFill:P0}");
            }

            return $"[{string.Join(",", parts)}]";
        }

        private AreaBundleSelectionResult TrySelectAllClearExact(
            BoardGrid board,
            float boardArea,
            string reasonPrefix)
        {
            AreaBundleEntry best = null;
            List<IReadOnlyList<Vector2Int>> bestPieces = null;
            float bestPred = float.NegativeInfinity;

            foreach (AreaBundleEntry entry in _pool.AllClearBundles)
            {
                if (ContainsSmallL(entry))
                {
                    continue;
                }

                List<IReadOnlyList<Vector2Int>> pieces = AreaBundlePieces.Build(entry);
                if (!AreaBundleMetrics.CanEmptyBoard(board, pieces, _pool.MaxSequencesPerBundle))
                {
                    continue;
                }

                float predicted = AreaBundleMetrics.MaxAreaAfterFullSequence(
                    board, pieces, _pool.MaxSequencesPerBundle, out bool any, _pool.AreaScore);
                if (!any)
                {
                    continue;
                }

                if (best == null || predicted > bestPred)
                {
                    best = entry;
                    bestPieces = pieces;
                    bestPred = predicted;
                }
            }

            if (best == null)
            {
                return null;
            }

            int bestSeq = AreaBundleMetrics.CountSequences(board, bestPieces, _pool.MaxSequencesPerBundle);
            return new AreaBundleSelectionResult(
                bestPieces,
                best.Ids,
                AreaBundleTier.AllClear,
                best.BundleId,
                boardArea,
                bestPred,
                bestSeq,
                isKillHand: false,
                reason: $"{reasonPrefix} · AllClear fixed-pool Exact p={_pool.AllClearProbability:P0}"
                    + $" occ≤{_pool.AllClearMaxOccupied} bundle={best.BundleId}",
                explainSteps: CaptureExplain(board, bestPieces));
        }

        private List<ScoredCandidate> ScoreSurvivors(
            BoardGrid board,
            IReadOnlyList<AreaBundleEntry> list,
            ShapeWeightProfile profile)
        {
            List<AreaBundleEntry> candidates = SampleCandidates(list);
            List<ScoredCandidate> scored = new(candidates.Count);
            double clearMs = 0;
            double scoreMs = 0;

            foreach (AreaBundleEntry entry in candidates)
            {
                List<IReadOnlyList<Vector2Int>> pieces = AreaBundlePieces.Build(entry);
                Stopwatch clearSw = Stopwatch.StartNew();
                SequenceOutcomeEstimator.SequenceOutcome outcome = SequenceOutcomeEstimator.Estimate(
                    board, pieces, _pool.OutcomeBeamWidth);
                clearMs += clearSw.Elapsed.TotalMilliseconds;
                if (!outcome.SequenceFound || outcome.TotalClears < 1 || outcome.FinalBoard == null)
                {
                    continue;
                }

                Stopwatch scoreSw = Stopwatch.StartNew();
                float predicted = AreaScoreCalculator.ScoreTotal(outcome.FinalBoard, _pool.AreaScore);
                scoreMs += scoreSw.Elapsed.TotalMilliseconds;

                scored.Add(new ScoredCandidate(
                    entry, pieces, outcome.TotalClears, outcome.BoardEmptied, predicted, sequenceCount: 0));
            }

            LogPerf($"  ScoreSurvivors.OutcomeBeam candidates={candidates.Count} kept={scored.Count}", clearMs);
            LogPerf($"  ScoreSurvivors.BeamAreaScore kept={scored.Count}", scoreMs);

            RefineTopKWithMaxArea(board, scored, profile);
            return scored;
        }

        /// <summary>
        /// 빔 Area 근사 상위 K만 MaxArea로 정밀화. Death 정렬용 effective 기준으로 자른다.
        /// </summary>
        private void RefineTopKWithMaxArea(
            BoardGrid board,
            List<ScoredCandidate> scored,
            ShapeWeightProfile profile)
        {
            int k = _pool.MaxAreaRefineTopK;
            if (k <= 0 || scored.Count == 0)
            {
                return;
            }

            scored.Sort((a, b) =>
            {
                float ea = a.PredictedArea * _pool.MeanShapeWeight(a.Entry.Ids, profile);
                float eb = b.PredictedArea * _pool.MeanShapeWeight(b.Entry.Ids, profile);
                return eb.CompareTo(ea);
            });

            if (k > scored.Count)
            {
                k = scored.Count;
            }

            double maxAreaMs = 0;
            int refined = 0;
            for (int i = 0; i < k; ++i)
            {
                ScoredCandidate candidate = scored[i];
                Stopwatch maxAreaSw = Stopwatch.StartNew();
                float predicted = AreaBundleMetrics.MaxAreaAfterFullSequence(
                    board,
                    candidate.Pieces,
                    _pool.MaxSequencesPerBundle,
                    out bool any,
                    _pool.AreaScore);
                maxAreaMs += maxAreaSw.Elapsed.TotalMilliseconds;
                if (!any)
                {
                    continue;
                }

                scored[i] = new ScoredCandidate(
                    candidate.Entry,
                    candidate.Pieces,
                    candidate.TotalClears,
                    candidate.BoardEmptied,
                    predicted,
                    candidate.SequenceCount);
                ++refined;
            }

            LogPerf($"  ScoreSurvivors.MaxAreaRefine topK={k} refined={refined}", maxAreaMs);
        }

        private AreaBundleSelectionResult TrySelectByMaxArea(
            BoardGrid board,
            float boardArea,
            IReadOnlyList<AreaBundleEntry> list,
            AreaBundleTier tier,
            string reasonPrefix)
        {
            Stopwatch scoreSw = Stopwatch.StartNew();
            List<AreaBundleEntry> candidates = SampleCandidates(list);
            List<ScoredCandidate> scored = new(candidates.Count);
            double beamMs = 0;
            double areaMs = 0;

            foreach (AreaBundleEntry entry in candidates)
            {
                List<IReadOnlyList<Vector2Int>> pieces = AreaBundlePieces.Build(entry);
                Stopwatch beamSw = Stopwatch.StartNew();
                SequenceOutcomeEstimator.SequenceOutcome outcome = SequenceOutcomeEstimator.Estimate(
                    board, pieces, _pool.OutcomeBeamWidth);
                beamMs += beamSw.Elapsed.TotalMilliseconds;
                if (!outcome.SequenceFound || outcome.FinalBoard == null)
                {
                    continue;
                }

                Stopwatch areaSw = Stopwatch.StartNew();
                float predicted = AreaScoreCalculator.ScoreTotal(outcome.FinalBoard, _pool.AreaScore);
                areaMs += areaSw.Elapsed.TotalMilliseconds;

                scored.Add(new ScoredCandidate(
                    entry, pieces, totalClears: 0, boardEmptied: false, predicted, sequenceCount: 0));
            }

            LogPerf($"Easy beam candidates={candidates.Count} kept={scored.Count}", beamMs);
            LogPerf($"Easy BeamAreaScore kept={scored.Count}", areaMs);
            RefineTopKWithMaxArea(board, scored, ShapeWeightProfile.Main);
            LogPerf($"Easy MaxAreaScan total", scoreSw.Elapsed.TotalMilliseconds);

            if (scored.Count == 0)
            {
                return null;
            }

            Stopwatch deathSw = Stopwatch.StartNew();
            AreaBundleSelectionResult picked = PickAreaWithDeathReject(
                scored, tier, board, boardArea, reasonPrefix, ShapeWeightProfile.Main);
            LogPerf("Easy DeathReject+ToResult", deathSw.Elapsed.TotalMilliseconds);
            return picked;
        }

        private AreaBundleSelectionResult PickAreaWithDeathReject(
            List<ScoredCandidate> scored,
            AreaBundleTier tier,
            BoardGrid board,
            float boardArea,
            string reasonPrefix,
            ShapeWeightProfile profile)
        {
            scored.Sort((a, b) =>
            {
                float ea = a.PredictedArea * _pool.MeanShapeWeight(a.Entry.Ids, profile);
                float eb = b.PredictedArea * _pool.MeanShapeWeight(b.Entry.Ids, profile);
                return eb.CompareTo(ea);
            });

            ScoredCandidate first = scored[0];
            int tries = _pool.DeathRejectMaxTries;
            if (tries > scored.Count)
            {
                tries = scored.Count;
            }

            double deathMs = 0;
            int deathChecks = 0;
            for (int i = 0; i < tries; ++i)
            {
                ScoredCandidate candidate = scored[i];
                Stopwatch deathSw = Stopwatch.StartNew();
                float deathPercent = AreaBundleMetrics.CountDeathPercent(
                    board,
                    candidate.Pieces,
                    _pool.DeathBranchBudget,
                    out int deathBranches,
                    out bool budgetExceeded);
                deathMs += deathSw.Elapsed.TotalMilliseconds;
                ++deathChecks;

                bool reject = !budgetExceeded
                    && deathBranches > 0
                    && deathPercent > _pool.DeathRejectPercent;
                if (reject)
                {
                    LogGate($"Death 배제 · bundle={candidate.Entry.BundleId}"
                        + $" death={deathPercent:F0}%/{deathBranches} > {_pool.DeathRejectPercent:F0}%");
                    continue;
                }

                if (budgetExceeded)
                {
                    LogGate($"Death 예산초과 통과 · bundle={candidate.Entry.BundleId}"
                        + $" branches>{_pool.DeathBranchBudget}");
                }

                LogPerf($"Death% checks={deathChecks}", deathMs);
                float meanW = _pool.MeanShapeWeight(candidate.Entry.Ids, profile);
                float effective = candidate.PredictedArea * meanW;
                string modeTag = profile == ShapeWeightProfile.Clean ? "Clean" : "Main";
                return ToResult(
                    candidate,
                    tier,
                    board,
                    boardArea,
                    $"{reasonPrefix} · {modeTag} maxArea bundle={candidate.Entry.BundleId}"
                        + $" pred={candidate.PredictedArea:F1}×w={meanW:F2}→{effective:F1}",
                    profile);
            }

            LogPerf($"Death% checks={deathChecks}", deathMs);
            float firstMeanW = _pool.MeanShapeWeight(first.Entry.Ids, profile);
            float firstEffective = first.PredictedArea * firstMeanW;
            string fallbackMode = profile == ShapeWeightProfile.Clean ? "Clean" : "Main";
            LogGate($"Death 배제 전부 실패 → 1등 폴백 · bundle={first.Entry.BundleId}");
            return ToResult(
                first,
                tier,
                board,
                boardArea,
                $"{reasonPrefix} · {fallbackMode} maxArea bundle={first.Entry.BundleId}"
                    + $" pred={first.PredictedArea:F1}×w={firstMeanW:F2}→{firstEffective:F1}",
                profile);
        }

        private AreaBundleSelectionResult ToResult(
            ScoredCandidate pick,
            AreaBundleTier tier,
            BoardGrid board,
            float boardArea,
            string reason,
            ShapeWeightProfile profile)
        {
            Stopwatch seqSw = Stopwatch.StartNew();
            int seq = AreaBundleMetrics.CountSequences(board, pick.Pieces, _pool.MaxSequencesPerBundle);
            LogPerf("CountSequences", seqSw.Elapsed.TotalMilliseconds);

            Stopwatch explainSw = Stopwatch.StartNew();
            List<AreaBundleExplainStep> explain = CaptureExplain(board, pick.Pieces);
            LogPerf("CaptureExplain", explainSw.Elapsed.TotalMilliseconds);

            return new AreaBundleSelectionResult(
                pick.Pieces,
                pick.Entry.Ids,
                tier,
                pick.Entry.BundleId,
                boardArea,
                pick.PredictedArea,
                seq,
                isKillHand: false,
                reason: reason,
                explainSteps: explain,
                profile: profile);
        }

        private List<AreaBundleExplainStep> CaptureExplain(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            if (!AreaBundleMetrics.TryGetBestSequenceExplain(
                    board,
                    pieces,
                    _pool.MaxSequencesPerBundle,
                    _pool.AreaScore,
                    out _,
                    out _,
                    out List<AreaBundleExplainStep> steps))
            {
                return null;
            }

            return steps;
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
                if (e != null && !ContainsSmallL(e))
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
                if (e != null && !ContainsSmallL(e))
                {
                    total += e.Weight;
                }
            }

            if (total <= 0)
            {
                foreach (AreaBundleEntry e in list)
                {
                    if (e != null)
                    {
                        return e;
                    }
                }

                return list[0];
            }

            int roll = _rng.Next(total);
            foreach (AreaBundleEntry e in list)
            {
                if (e == null || ContainsSmallL(e))
                {
                    continue;
                }

                roll -= e.Weight;
                if (roll < 0)
                {
                    return e;
                }
            }

            foreach (AreaBundleEntry e in list)
            {
                if (e != null && !ContainsSmallL(e))
                {
                    return e;
                }
            }

            return list[list.Count - 1];
        }

        private static bool ContainsSmallL(AreaBundleEntry entry) =>
            HospitalityPiecePolicy.IsSmallL(entry.Id0)
            || HospitalityPiecePolicy.IsSmallL(entry.Id1)
            || HospitalityPiecePolicy.IsSmallL(entry.Id2);

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
