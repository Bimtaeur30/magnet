using System.Collections.Generic;
using System.Text;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockBlast;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using UnityEngine;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    public static class AreaBundlePieces
    {
        public static List<IReadOnlyList<Vector2Int>> Build(AreaBundleEntry entry)
        {
            return new List<IReadOnlyList<Vector2Int>>(3)
            {
                BlockBlastCatalog.GetOffsets(entry.Id0),
                BlockBlastCatalog.GetOffsets(entry.Id1),
                BlockBlastCatalog.GetOffsets(entry.Id2),
            };
        }
    }

    public static class AreaBundleMetrics
    {
        public static int CountSequences(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces, int cap) =>
            PlacementSolver.CountFullSequences(board, pieces, cap);

        public static bool CanSurvive(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces) =>
            PlacementSolver.FullSequenceExists(board, pieces);

        public static bool CanEmptyBoard(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            int sequenceCap)
        {
            if (pieces == null || pieces.Count == 0)
            {
                return false;
            }

            string[] signatures = BuildSignatures(pieces);
            bool[] used = new bool[pieces.Count];
            int found = 0;
            return SearchEmpty(board, pieces, signatures, used, 0, sequenceCap, ref found);
        }

        public static float MaxAreaAfterFullSequence(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            int sequenceCap,
            out bool any,
            AreaScoreTuning tuning = null)
        {
            any = false;
            float best = float.NegativeInfinity;
            string[] signatures = BuildSignatures(pieces);
            bool[] used = new bool[pieces.Count];
            int found = 0;
            BoardGrid bestBoard = null;
            List<AreaBundleExplainStep> path = new(pieces.Count);
            List<AreaBundleExplainStep> bestPath = null;
            SearchMaxArea(
                board,
                pieces,
                signatures,
                used,
                0,
                sequenceCap,
                tuning,
                path,
                ref found,
                ref best,
                ref any,
                ref bestBoard,
                ref bestPath);
            return any ? best : float.NegativeInfinity;
        }

        /// <summary>
        /// MaxArea 우승 시퀀스를 적용한 보드. 없으면 false.
        /// </summary>
        public static bool TryGetBoardAfterBestSequence(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            int sequenceCap,
            AreaScoreTuning tuning,
            out BoardGrid afterBest,
            out float bestArea)
        {
            return TryGetBestSequenceExplain(
                board, pieces, sequenceCap, tuning, out afterBest, out bestArea, out _);
        }

        /// <summary>
        /// MaxArea 우승 시퀀스 + 시뮬 배치 스텝. 없으면 false.
        /// </summary>
        public static bool TryGetBestSequenceExplain(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            int sequenceCap,
            AreaScoreTuning tuning,
            out BoardGrid afterBest,
            out float bestArea,
            out List<AreaBundleExplainStep> explainSteps)
        {
            afterBest = null;
            bestArea = float.NegativeInfinity;
            explainSteps = null;
            bool any = false;
            string[] signatures = BuildSignatures(pieces);
            bool[] used = new bool[pieces.Count];
            int found = 0;
            BoardGrid bestBoard = null;
            List<AreaBundleExplainStep> path = new(pieces.Count);
            List<AreaBundleExplainStep> bestPath = null;
            SearchMaxArea(
                board,
                pieces,
                signatures,
                used,
                0,
                sequenceCap,
                tuning,
                path,
                ref found,
                ref bestArea,
                ref any,
                ref bestBoard,
                ref bestPath);
            if (!any || bestBoard == null)
            {
                return false;
            }

            afterBest = bestBoard;
            explainSteps = bestPath ?? new List<AreaBundleExplainStep>();
            return true;
        }

        /// <summary>
        /// Death %. <paramref name="branchBudget"/> &gt; 0 이면 분모가 예산을 넘는 순간 중단.
        /// Normal/Easy 배제용. 로그 미사용.
        /// </summary>
        /// <param name="budgetExceeded">예산 초과로 조기 종료(선택 시 통과 처리).</param>
        public static float CountDeathPercent(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            int branchBudget,
            out int branches,
            out bool budgetExceeded)
        {
            branches = 0;
            budgetExceeded = false;
            if (pieces == null || pieces.Count == 0)
            {
                return 0f;
            }

            string[] signatures = BuildSignatures(pieces);
            bool[] used = new bool[pieces.Count];
            int deaths = 0;
            AccumulateDeaths(
                board, pieces, signatures, used, branchBudget, ref deaths, ref branches, ref budgetExceeded);
            if (branches <= 0)
            {
                return 0f;
            }

            return 100f * deaths / branches;
        }

        private static void AccumulateDeaths(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            string[] signatures,
            bool[] used,
            int branchBudget,
            ref int deaths,
            ref int branches,
            ref bool budgetExceeded)
        {
            if (budgetExceeded)
            {
                return;
            }

            int remaining = 0;
            for (int i = 0; i < used.Length; ++i)
            {
                if (!used[i])
                {
                    ++remaining;
                }
            }

            if (remaining <= 1)
            {
                return;
            }

            HashSet<string> tried = new();
            int size = board.BoardSize;
            Vector2Int pivot = Vector2Int.zero;

            for (int i = 0; i < pieces.Count; ++i)
            {
                if (budgetExceeded)
                {
                    return;
                }

                if (used[i] || !tried.Add(signatures[i]))
                {
                    continue;
                }

                IReadOnlyList<Vector2Int> offsets = pieces[i];
                for (int x = 0; x < size; ++x)
                {
                    for (int y = 0; y < size; ++y)
                    {
                        if (budgetExceeded)
                        {
                            return;
                        }

                        pivot.x = x;
                        pivot.y = y;
                        if (!PlacementService.CanPlace(offsets, pivot, board))
                        {
                            continue;
                        }

                        BoardGrid next = board.Clone();
                        PlacementSimulator.PlaceAndClear(next, offsets, pivot);
                        used[i] = true;
                        ++branches;

                        if (branchBudget > 0 && branches > branchBudget)
                        {
                            budgetExceeded = true;
                            used[i] = false;
                            return;
                        }

                        if (!PlacementSolver.FullSequenceExists(next, RemainingPieces(pieces, used)))
                        {
                            ++deaths;
                        }
                        else
                        {
                            AccumulateDeaths(
                                next,
                                pieces,
                                signatures,
                                used,
                                branchBudget,
                                ref deaths,
                                ref branches,
                                ref budgetExceeded);
                        }

                        used[i] = false;
                        if (budgetExceeded)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private static List<IReadOnlyList<Vector2Int>> RemainingPieces(
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            bool[] used)
        {
            List<IReadOnlyList<Vector2Int>> list = new(pieces.Count);
            for (int i = 0; i < pieces.Count; ++i)
            {
                if (!used[i])
                {
                    list.Add(pieces[i]);
                }
            }

            return list;
        }

        private static void SearchMaxArea(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            string[] signatures,
            bool[] used,
            int placedCount,
            int cap,
            AreaScoreTuning tuning,
            List<AreaBundleExplainStep> path,
            ref int found,
            ref float best,
            ref bool any,
            ref BoardGrid bestBoard,
            ref List<AreaBundleExplainStep> bestPath)
        {
            if (found >= cap)
            {
                return;
            }

            if (placedCount == pieces.Count)
            {
                ++found;
                any = true;
                float score = AreaScoreCalculator.ScoreTotal(board, tuning);
                if (score > best)
                {
                    best = score;
                    bestBoard = board;
                    bestPath = CopyExplainPath(path);
                }

                return;
            }

            HashSet<string> tried = new();
            int size = board.BoardSize;
            Vector2Int pivot = Vector2Int.zero;

            for (int i = 0; i < pieces.Count; ++i)
            {
                if (used[i] || !tried.Add(signatures[i]))
                {
                    continue;
                }

                used[i] = true;
                IReadOnlyList<Vector2Int> offsets = pieces[i];
                for (int x = 0; x < size; ++x)
                {
                    for (int y = 0; y < size; ++y)
                    {
                        pivot.x = x;
                        pivot.y = y;
                        if (!PlacementService.CanPlace(offsets, pivot, board))
                        {
                            continue;
                        }

                        BoardGrid next = board.Clone();
                        PlacementSimulator.PlaceAndClear(next, offsets, pivot);
                        path.Add(BuildExplainStep(i, pivot, offsets));
                        SearchMaxArea(
                            next,
                            pieces,
                            signatures,
                            used,
                            placedCount + 1,
                            cap,
                            tuning,
                            path,
                            ref found,
                            ref best,
                            ref any,
                            ref bestBoard,
                            ref bestPath);
                        path.RemoveAt(path.Count - 1);
                        if (found >= cap)
                        {
                            used[i] = false;
                            return;
                        }
                    }
                }

                used[i] = false;
            }
        }

        private static AreaBundleExplainStep BuildExplainStep(
            int pieceSlotIndex,
            Vector2Int pivot,
            IReadOnlyList<Vector2Int> offsets)
        {
            Vector2Int[] cells = new Vector2Int[offsets.Count];
            for (int i = 0; i < offsets.Count; ++i)
            {
                cells[i] = pivot + offsets[i];
            }

            return new AreaBundleExplainStep(pieceSlotIndex, pivot, cells);
        }

        private static List<AreaBundleExplainStep> CopyExplainPath(List<AreaBundleExplainStep> path)
        {
            List<AreaBundleExplainStep> copy = new(path.Count);
            for (int i = 0; i < path.Count; ++i)
            {
                copy.Add(path[i]);
            }

            return copy;
        }

        private static bool SearchEmpty(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            string[] signatures,
            bool[] used,
            int placedCount,
            int cap,
            ref int found)
        {
            if (found >= cap)
            {
                return false;
            }

            if (placedCount == pieces.Count)
            {
                ++found;
                return CountOccupiedCells(board) == 0;
            }

            HashSet<string> tried = new();
            int size = board.BoardSize;
            Vector2Int pivot = Vector2Int.zero;

            for (int i = 0; i < pieces.Count; ++i)
            {
                if (used[i] || !tried.Add(signatures[i]))
                {
                    continue;
                }

                used[i] = true;
                IReadOnlyList<Vector2Int> offsets = pieces[i];
                for (int x = 0; x < size; ++x)
                {
                    for (int y = 0; y < size; ++y)
                    {
                        pivot.x = x;
                        pivot.y = y;
                        if (!PlacementService.CanPlace(offsets, pivot, board))
                        {
                            continue;
                        }

                        BoardGrid next = board.Clone();
                        PlacementSimulator.PlaceAndClear(next, offsets, pivot);
                        if (SearchEmpty(next, pieces, signatures, used, placedCount + 1, cap, ref found))
                        {
                            used[i] = false;
                            return true;
                        }

                        if (found >= cap)
                        {
                            used[i] = false;
                            return false;
                        }
                    }
                }

                used[i] = false;
            }

            return false;
        }

        private static int CountOccupiedCells(BoardGrid board)
        {
            int n = board.BoardSize;
            int count = 0;
            Vector2Int cell = Vector2Int.zero;
            for (int x = 0; x < n; ++x)
            {
                for (int y = 0; y < n; ++y)
                {
                    cell.x = x;
                    cell.y = y;
                    if (board.IsOccupied(cell))
                    {
                        ++count;
                    }
                }
            }

            return count;
        }

        private static string[] BuildSignatures(IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            string[] signatures = new string[pieces.Count];
            StringBuilder builder = new();
            for (int i = 0; i < pieces.Count; ++i)
            {
                builder.Clear();
                foreach (Vector2Int offset in pieces[i])
                {
                    builder.Append(offset.x).Append(',').Append(offset.y).Append(';');
                }

                signatures[i] = builder.ToString();
            }

            return signatures;
        }
    }
}
