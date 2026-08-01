using System.Collections.Generic;
using System.Diagnostics;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockBlast
{
    /// <summary>
    /// BlockBlast! 역공학 핸드오프 기반 블록 선택 알고리즘.
    /// 파이프라인(핸드오프 §3·§5·§11):
    ///   base(7 random-no-death)
    ///   → AlgoFillSortEdgeTrait: round 2부터 90% 확률로 1370 교체
    ///   → ContinueSameMoreRoundLimitTrait: 최근 2라운드와 2개 이상 중복 시 교체(2100)
    ///   → delCurrentSameBlock: 직전 트리플과 동일하면 가운데 블록 교체.
    /// 1370(all-combination fill)은 네이티브(libtask.so) 미복원이라
    /// "완주 가능 시퀀스 존재 + 라인 클리어 선호" 조합 탐색으로 근사한다.
    /// </summary>
    public sealed class BlockBlastAlgorithm
    {
        public const int AlgoRandomNoDeath = 7;
        public const int AlgoAllCombinationFill = 1370;
        public const int AlgoRoundLimitReplace = 2100;

        private const double FillSortEdgeReplaceProbability = 0.9;
        private const int RoundLimitOverlapThreshold = 2;
        private const int HistoryRounds = 2;
        private const int RoundLimitRetryCount = 8;
        private const int TripleSize = 3;

        /// <summary>randomNoDie 원본의 100ms 탐색 제한 (핸드오프 §11).</summary>
        private const long SearchBudgetMs = 100;

        /// <summary>시간 예산과 별개의 조합 수 상한 — 프레임 스파이크 안전장치.</summary>
        private const int ComboScanCap = 150;

        /// <summary>1370 근사의 가중치 샘플링 시도 횟수 상한.</summary>
        private const int FillSampleCount = 120;

        private readonly System.Random _random;
        private readonly List<int[]> _recentTriples = new();
        private int _round;

        public BlockBlastAlgorithm(System.Random random)
        {
            _random = random;
        }

        public BlockBlastSelection Select(BoardGrid board)
        {
            ++_round;

            const int baseAlgoId = AlgoRandomNoDeath;
            int actualAlgoId = baseAlgoId;
            List<string> reasons = new();

            // AlgoFillSortEdgeTrait — classRoundNum > 1부터 관여 (§5.1)
            if (_round > 1 && _random.NextDouble() < FillSortEdgeReplaceProbability)
            {
                actualAlgoId = AlgoAllCombinationFill;
            }

            int[] triple = null;
            if (actualAlgoId == AlgoAllCombinationFill)
            {
                triple = TrySelectAllCombinationFill(board, reasons);
                if (triple == null)
                {
                    actualAlgoId = AlgoRandomNoDeath;
                    reasons.Add("1370 탐색 실패 → random-no-death(7) 강등");
                }
            }
            else
            {
                reasons.Add(_round == 1
                    ? "round 1 — base random-no-death(7)"
                    : "FillSortEdge 확률 탈락(10%) — random-no-death(7)");
            }

            triple ??= SelectRandomNoDie(board, reasons);

            // ContinueSameMoreRoundLimitTrait (§5.2)
            if (TryGetRoundLimitOverlap(triple, out int[] overlappedPrev))
            {
                int[] replaced = ReplaceRepeatedBlocks(board, triple, overlappedPrev);
                if (replaced != null)
                {
                    triple = replaced;
                    actualAlgoId = AlgoRoundLimitReplace;
                    reasons.Add("반복 방지: 최근 2라운드와 2개 이상 중복 → 교체(2100)");
                }
            }

            // delCurrentSameBlock (§11)
            if (_recentTriples.Count > 0 && SameMultiset(triple, _recentTriples[^1]))
            {
                triple[1] = ProduceRandomId(board, triple);
                reasons.Add("delCurrentSameBlock: 직전 트리플과 동일 → 가운데 블록 교체");
            }

            RecordHistory(triple);

            return new BlockBlastSelection(
                _round, baseAlgoId, actualAlgoId, triple, BuildPieces(triple), string.Join(" · ", reasons));
        }

        /// <summary>
        /// 1370 근사 — 셀 수 가중치로 트리플을 샘플링(중복 허용, 3개 동일 금지)해
        /// "라인 클리어가 나오는 완주 조합"을 우선 채택, 없으면 "완주만 가능한 조합", 그마저 없으면 null.
        /// 순서 조합 스캔은 완주 검증을 쉽게 통과하는 소형 블록으로 쏠려서(원작 대비 대형 블록 실종)
        /// 가중치 샘플링으로 교체 — 대형·장블록도 원작 체감 빈도로 등장한다.
        /// </summary>
        private int[] TrySelectAllCombinationFill(BoardGrid board, List<string> reasons)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int[] fullSequenceFallback = null;

            for (int attempt = 0; attempt < FillSampleCount; ++attempt)
            {
                if (stopwatch.ElapsedMilliseconds > SearchBudgetMs)
                {
                    break;
                }

                int[] triple = SampleWeightedTriple();
                List<IReadOnlyList<Vector2Int>> pieces = BuildPieces(triple);

                if (!PlacementSolver.FullSequenceExists(board, pieces))
                {
                    continue;
                }

                if (PlacementSolver.ComboMaintainable(board, pieces))
                {
                    reasons.Add($"1370 근사: 클리어 가능 완주 조합 채택 (샘플 {attempt + 1}회)");
                    return triple;
                }

                fullSequenceFallback ??= triple;
            }

            if (fullSequenceFallback != null)
            {
                reasons.Add("1370 근사: 클리어 조합 없음 → 완주 가능 조합 채택");
                return fullSequenceFallback;
            }

            return null;
        }

        /// <summary>가중치 독립 추첨 3회 (중복 허용) — 3개 전부 동일한 트리플만 재추첨.</summary>
        private int[] SampleWeightedTriple()
        {
            int[] triple = new int[TripleSize];
            do
            {
                for (int i = 0; i < TripleSize; ++i)
                {
                    triple[i] = SampleWeightedId();
                }
            }
            while (triple[0] == triple[1] && triple[1] == triple[2]);

            return triple;
        }

        private int SampleWeightedId()
        {
            float roll = (float)(_random.NextDouble() * BlockBlastCatalog.FillPoolWeightTotal);
            float[] weights = BlockBlastCatalog.FillPoolWeights;
            int[] ids = BlockBlastCatalog.FillPoolIds;

            for (int i = 0; i < weights.Length; ++i)
            {
                roll -= weights[i];
                if (roll <= 0f)
                {
                    return ids[i];
                }
            }

            return ids[^1];
        }

        /// <summary>
        /// randomNoDie (§11) — 풀(2..30)을 셔플해 완주 가능 조합을 탐색.
        /// 실패 시 [1, random-placeable, 1] fallback (placeable 없으면 [1,1,1]).
        /// </summary>
        private int[] SelectRandomNoDie(BoardGrid board, List<string> reasons)
        {
            int[] pool = BlockBlastCatalog.BuildNoDiePool();
            Shuffle(pool);

            int[] found = ScanCombos(board, pool);
            if (found != null)
            {
                return found;
            }

            int[] fallback = { 1, 1, 1 };
            fallback[1] = ProduceRandomId(board, fallback);
            reasons.Add("randomNoDie 실패 → fallback [1, random-placeable, 1]");
            return fallback;
        }

        /// <summary>
        /// 셔플된 풀의 3-조합을 순회하며 완주 가능(FullSequence) 첫 조합을 찾는다.
        /// 시간·개수 예산 초과 시 중단하고 null.
        /// </summary>
        private static int[] ScanCombos(BoardGrid board, int[] pool)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int scanned = 0;

            for (int i = 0; i < pool.Length - 2; ++i)
            {
                for (int j = i + 1; j < pool.Length - 1; ++j)
                {
                    for (int k = j + 1; k < pool.Length; ++k)
                    {
                        if (scanned >= ComboScanCap || stopwatch.ElapsedMilliseconds > SearchBudgetMs)
                        {
                            return null;
                        }

                        ++scanned;
                        int[] triple = { pool[i], pool[j], pool[k] };
                        if (PlacementSolver.FullSequenceExists(board, BuildPieces(triple)))
                        {
                            return triple;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>최근 최대 2라운드 트리플과 2개 이상(다중집합) 겹치는지 검사 (§5.2).</summary>
        private bool TryGetRoundLimitOverlap(int[] triple, out int[] overlappedPrev)
        {
            overlappedPrev = null;
            foreach (int[] prev in _recentTriples)
            {
                if (CountMultisetIntersection(triple, prev) >= RoundLimitOverlapThreshold)
                {
                    overlappedPrev = prev;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 직전 라운드와 겹치는 블록만 no-die 풀 재추첨으로 교체한다.
        /// 완주 가능 조합을 우선하되, 예산 내 실패 시 마지막 후보를 그대로 쓴다
        /// (원본도 random-no-death fallback으로 강등하는 지점 — §5.2).
        /// </summary>
        private int[] ReplaceRepeatedBlocks(BoardGrid board, int[] triple, int[] overlappedPrev)
        {
            List<int> repeatedSlots = new();
            List<int> prevRemainder = new(overlappedPrev);
            for (int slot = 0; slot < TripleSize; ++slot)
            {
                if (prevRemainder.Remove(triple[slot]))
                {
                    repeatedSlots.Add(slot);
                }
            }

            if (repeatedSlots.Count == 0)
            {
                return null;
            }

            int[] pool = BlockBlastCatalog.BuildNoDiePool();
            int[] lastCandidate = null;

            for (int attempt = 0; attempt < RoundLimitRetryCount; ++attempt)
            {
                Shuffle(pool);
                int[] candidate = (int[])triple.Clone();
                int poolCursor = 0;

                foreach (int slot in repeatedSlots)
                {
                    while (poolCursor < pool.Length
                        && (Contains(candidate, pool[poolCursor]) || Contains(overlappedPrev, pool[poolCursor])))
                    {
                        ++poolCursor;
                    }

                    if (poolCursor >= pool.Length)
                    {
                        break;
                    }

                    candidate[slot] = pool[poolCursor];
                    ++poolCursor;
                }

                lastCandidate = candidate;
                if (PlacementSolver.FullSequenceExists(board, BuildPieces(candidate)))
                {
                    return candidate;
                }
            }

            return lastCandidate;
        }

        /// <summary>
        /// produceRandomId (§11) — 현재 트리플에 없는 배치 가능 ID(2..42) 중 균등 추첨.
        /// 후보가 없으면 1.
        /// </summary>
        private int ProduceRandomId(BoardGrid board, int[] currentTriple)
        {
            List<int> candidates = new();
            for (int id = BlockBlastCatalog.RandomPoolMin; id <= BlockBlastCatalog.RandomPoolMax; ++id)
            {
                if (Contains(currentTriple, id))
                {
                    continue;
                }

                if (PlacementService.CanPlaceAnywhere(BlockBlastCatalog.GetOffsets(id), board))
                {
                    candidates.Add(id);
                }
            }

            return candidates.Count == 0 ? 1 : candidates[_random.Next(candidates.Count)];
        }

        /// <summary>
        /// 외부(하이브리드 특수 티어)가 확정한 트리플을 히스토리에 반영한다.
        /// 반복 억제 트레이트는 우회하되(솔버 보장 보호) 기록은 남겨 다음 base 선택이
        /// 같은 조합을 반복하지 않게 하고, 라운드 카운터도 진행시켜 FillSortEdge 게이트를 유지한다.
        /// </summary>
        public void RecordExternalRound(int[] triple)
        {
            ++_round;
            RecordHistory(triple);
        }

        private void RecordHistory(int[] triple)
        {
            _recentTriples.Add((int[])triple.Clone());
            if (_recentTriples.Count > HistoryRounds)
            {
                _recentTriples.RemoveAt(0);
            }
        }

        private static List<IReadOnlyList<Vector2Int>> BuildPieces(int[] triple)
        {
            List<IReadOnlyList<Vector2Int>> pieces = new(triple.Length);
            foreach (int id in triple)
            {
                pieces.Add(BlockBlastCatalog.GetOffsets(id));
            }

            return pieces;
        }

        private static int CountMultisetIntersection(int[] a, int[] b)
        {
            List<int> remainder = new(b);
            int count = 0;
            foreach (int id in a)
            {
                if (remainder.Remove(id))
                {
                    ++count;
                }
            }

            return count;
        }

        private static bool SameMultiset(int[] a, int[] b)
        {
            return a.Length == b.Length && CountMultisetIntersection(a, b) == a.Length;
        }

        private static bool Contains(int[] values, int target)
        {
            foreach (int value in values)
            {
                if (value == target)
                {
                    return true;
                }
            }

            return false;
        }

        private void Shuffle(int[] values)
        {
            for (int i = values.Length - 1; i > 0; --i)
            {
                int j = _random.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
        }
    }
}
