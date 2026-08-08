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
            SearchMaxArea(board, pieces, signatures, used, 0, sequenceCap, tuning, ref found, ref best, ref any);
            return any ? best : float.NegativeInfinity;
        }

        public static int CountDeaths(BoardGrid board, IReadOnlyList<IReadOnlyList<Vector2Int>> pieces)
        {
            string[] signatures = BuildSignatures(pieces);
            bool[] used = new bool[pieces.Count];
            return CountDeathsRecursive(board, pieces, signatures, used);
        }

        private static int CountDeathsRecursive(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            string[] signatures,
            bool[] used)
        {
            int remaining = 0;
            for (int i = 0; i < used.Length; ++i)
            {
                if (!used[i])
                {
                    ++remaining;
                }
            }

            if (remaining == 0)
            {
                return 0;
            }

            int deaths = 0;
            HashSet<string> tried = new();
            int size = board.BoardSize;
            Vector2Int pivot = Vector2Int.zero;

            for (int i = 0; i < pieces.Count; ++i)
            {
                if (used[i] || !tried.Add(signatures[i]))
                {
                    continue;
                }

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
                        used[i] = true;

                        if (remaining == 1)
                        {
                        }
                        else if (!PlacementSolver.FullSequenceExists(next, RemainingPieces(pieces, used)))
                        {
                            ++deaths;
                        }
                        else
                        {
                            deaths += CountDeathsRecursive(next, pieces, signatures, used);
                        }

                        used[i] = false;
                    }
                }
            }

            return deaths;
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
            ref int found,
            ref float best,
            ref bool any)
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
                        SearchMaxArea(next, pieces, signatures, used, placedCount + 1, cap, tuning, ref found, ref best, ref any);
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
