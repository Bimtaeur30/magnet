using System.Collections.Generic;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using UnityEngine;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    /// <summary>
    /// 히트맵 점수: Σ heat − emptyPenalty×(heat==0 칸).
    /// seekAllClear일 때만 올클 자리 우선·즉시 확정 (빈 보드에서는 끔).
    /// </summary>
    public static class HeatmapHandScorer
    {
        private static readonly int[][] Perms3 =
        {
            new[] { 0, 1, 2 },
            new[] { 0, 2, 1 },
            new[] { 1, 0, 2 },
            new[] { 1, 2, 0 },
            new[] { 2, 0, 1 },
            new[] { 2, 1, 0 },
        };

        public static float ScoreBest(
            BoardGrid board,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            out List<AreaBundleExplainStep> explain,
            out bool allCleared,
            bool seekAllClear = true,
            float emptyPenalty = 2f)
        {
            float best = float.NegativeInfinity;
            List<AreaBundleExplainStep> bestPath = null;
            allCleared = false;

            for (int p = 0; p < Perms3.Length; ++p)
            {
                int[] order = Perms3[p];
                float score = ScoreOrder(
                    board, pieces, order, seekAllClear, emptyPenalty,
                    out List<AreaBundleExplainStep> path, out bool emptied);
                if (emptied)
                {
                    explain = path;
                    allCleared = true;
                    return score;
                }

                if (score > best)
                {
                    best = score;
                    bestPath = path;
                }
            }

            explain = bestPath ?? new List<AreaBundleExplainStep>();
            return float.IsNegativeInfinity(best) ? 0f : best;
        }

        private static float ScoreOrder(
            BoardGrid start,
            IReadOnlyList<IReadOnlyList<Vector2Int>> pieces,
            int[] order,
            bool seekAllClear,
            float emptyPenalty,
            out List<AreaBundleExplainStep> path,
            out bool allCleared)
        {
            BoardGrid sim = start.Clone();
            path = new List<AreaBundleExplainStep>(pieces.Count);
            float total = 0f;
            allCleared = false;

            for (int step = 0; step < order.Length; ++step)
            {
                int slot = order[step];
                IReadOnlyList<Vector2Int> offsets = pieces[slot];
                int[,] heat = LineFillHeatmap.Build(sim);
                if (!TryBestPlacement(
                        sim, heat, offsets, seekAllClear, emptyPenalty,
                        out Vector2Int pivot,
                        out float gain,
                        out List<Vector2Int> cells,
                        out bool empties))
                {
                    continue;
                }

                total += gain;
                path.Add(new AreaBundleExplainStep(slot, pivot, cells));
                PlacementSimulator.PlaceAndClear(sim, offsets, pivot);
                if (seekAllClear && (empties || CountOccupied(sim) == 0))
                {
                    allCleared = true;
                    return total;
                }
            }

            return total;
        }

        private static bool TryBestPlacement(
            BoardGrid board,
            int[,] heat,
            IReadOnlyList<Vector2Int> offsets,
            bool seekAllClear,
            float emptyPenalty,
            out Vector2Int bestPivot,
            out float bestGain,
            out List<Vector2Int> bestCells,
            out bool emptiesBoard)
        {
            bestPivot = default;
            bestGain = float.NegativeInfinity;
            bestCells = null;
            emptiesBoard = false;
            int n = board.BoardSize;
            Vector2Int pivot = Vector2Int.zero;
            bool any = false;

            for (int x = 0; x < n; ++x)
            {
                for (int y = 0; y < n; ++y)
                {
                    pivot.x = x;
                    pivot.y = y;
                    if (!PlacementService.CanPlace(offsets, pivot, board))
                    {
                        continue;
                    }

                    if (seekAllClear)
                    {
                        BoardGrid probe = board.Clone();
                        PlacementSimulator.PlaceAndClear(probe, offsets, pivot);
                        if (CountOccupied(probe) == 0)
                        {
                            bestPivot = pivot;
                            bestGain = LineFillHeatmap.ScorePlacement(heat, pivot, offsets, emptyPenalty);
                            bestCells = BuildCells(pivot, offsets);
                            emptiesBoard = true;
                            return true;
                        }
                    }

                    float gain = LineFillHeatmap.ScorePlacement(heat, pivot, offsets, emptyPenalty);
                    if (!any || gain > bestGain)
                    {
                        any = true;
                        bestGain = gain;
                        bestPivot = pivot;
                        bestCells = BuildCells(pivot, offsets);
                    }
                }
            }

            return any;
        }

        private static List<Vector2Int> BuildCells(Vector2Int pivot, IReadOnlyList<Vector2Int> offsets)
        {
            List<Vector2Int> cells = new(offsets.Count);
            for (int i = 0; i < offsets.Count; ++i)
            {
                cells.Add(pivot + offsets[i]);
            }

            return cells;
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
    }
}
