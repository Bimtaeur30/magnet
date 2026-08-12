using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockBlast;
using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    public readonly struct HospitalityHole
    {
        public HospitalityHole(float contourFill, IReadOnlyList<Vector2Int> cells, HashSet<int> fittingIds)
        {
            ContourFill = contourFill;
            Cells = cells;
            FittingIds = fittingIds;
        }

        public float ContourFill { get; }
        public IReadOnlyList<Vector2Int> Cells { get; }
        public HashSet<int> FittingIds { get; }
    }

    public static class OpportunityDetector
    {
        private const int MinHospitalityHoleSize = 3;
        private const int MaxHospitalityHoleSize = 5;

        private static readonly Vector2Int[] Cardinals =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1)
        };

        private static readonly Vector2Int[] Neighbors8 =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
            new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)
        };

        public static List<HospitalityHole> FindQualifyingHoles(BoardGrid board, float minContourFill)
        {
            List<List<Vector2Int>> emptyRegions = CollectEmptyRegions(board);
            List<HospitalityHole> holes = new();

            foreach (List<Vector2Int> cells in emptyRegions)
            {
                if (cells.Count < MinHospitalityHoleSize || cells.Count > MaxHospitalityHoleSize)
                {
                    continue;
                }

                if (!TryContourFill(board, cells, out float fill) || fill < minContourFill)
                {
                    continue;
                }

                HashSet<int> fitting = FindExactFitIds(cells);
                if (fitting.Count == 0)
                {
                    continue;
                }

                holes.Add(new HospitalityHole(fill, cells, fitting));
            }

            holes.Sort((a, b) => b.ContourFill.CompareTo(a.ContourFill));
            return holes;
        }

        public static float SumFittingWeight(AreaBundleEntry entry, IReadOnlyList<HospitalityHole> holes)
        {
            float sum = 0f;
            int[] ids = entry.Ids;
            for (int i = 0; i < ids.Length; ++i)
            {
                if (FitsAnyHole(ids[i], holes))
                {
                    sum += HospitalityPiecePolicy.FitWeight(ids[i]);
                }
            }

            return sum;
        }

        public static bool IsHalfWeightOnlyFit(IReadOnlyList<int> ids, IReadOnlyList<HospitalityHole> holes)
        {
            bool anyFit = false;
            for (int i = 0; i < ids.Count; ++i)
            {
                int id = ids[i];
                if (!FitsAnyHole(id, holes))
                {
                    continue;
                }

                anyFit = true;
                if (HospitalityPiecePolicy.FitWeight(id) >= HospitalityPiecePolicy.FullWeight)
                {
                    return false;
                }
            }

            return anyFit;
        }

        public static int CompareHoleCoverage(AreaBundleEntry a, AreaBundleEntry b, IReadOnlyList<HospitalityHole> holes)
        {
            int weightCmp = SumFittingWeight(a, holes).CompareTo(SumFittingWeight(b, holes));
            if (weightCmp != 0)
            {
                return weightCmp;
            }

            for (int h = 0; h < holes.Count; ++h)
            {
                float wa = SumWeightFittingHole(a, holes[h]);
                float wb = SumWeightFittingHole(b, holes[h]);
                int cmp = wa.CompareTo(wb);
                if (cmp != 0)
                {
                    return cmp;
                }
            }

            return 0;
        }

        private static float SumWeightFittingHole(AreaBundleEntry entry, HospitalityHole hole)
        {
            float sum = 0f;
            int[] ids = entry.Ids;
            for (int i = 0; i < ids.Length; ++i)
            {
                if (hole.FittingIds.Contains(ids[i]))
                {
                    sum += HospitalityPiecePolicy.FitWeight(ids[i]);
                }
            }

            return sum;
        }

        private static bool FitsAnyHole(int id, IReadOnlyList<HospitalityHole> holes)
        {
            if (!HospitalityPiecePolicy.IsAllowed(id))
            {
                return false;
            }

            for (int i = 0; i < holes.Count; ++i)
            {
                if (holes[i].FittingIds.Contains(id))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<List<Vector2Int>> CollectEmptyRegions(BoardGrid board)
        {
            int n = board.BoardSize;
            bool[,] visited = new bool[n, n];
            List<List<Vector2Int>> regions = new();

            for (int x = 0; x < n; ++x)
            {
                for (int y = 0; y < n; ++y)
                {
                    if (visited[x, y] || board.IsOccupied(new Vector2Int(x, y)))
                    {
                        continue;
                    }

                    regions.Add(FloodEmpty(board, visited, x, y));
                }
            }

            return regions;
        }

        private static List<Vector2Int> FloodEmpty(BoardGrid board, bool[,] visited, int startX, int startY)
        {
            int n = board.BoardSize;
            List<Vector2Int> cells = new();
            Queue<Vector2Int> queue = new();
            queue.Enqueue(new Vector2Int(startX, startY));
            visited[startX, startY] = true;

            while (queue.Count > 0)
            {
                Vector2Int cur = queue.Dequeue();
                cells.Add(cur);

                foreach (Vector2Int d in Cardinals)
                {
                    int nx = cur.x + d.x;
                    int ny = cur.y + d.y;
                    if (nx < 0 || ny < 0 || nx >= n || ny >= n || visited[nx, ny])
                    {
                        continue;
                    }

                    if (board.IsOccupied(new Vector2Int(nx, ny)))
                    {
                        continue;
                    }

                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }

            return cells;
        }

        private static bool TryContourFill(BoardGrid board, List<Vector2Int> holeCells, out float fill)
        {
            HashSet<Vector2Int> hole = new(holeCells);
            HashSet<Vector2Int> contour = new();

            foreach (Vector2Int cell in holeCells)
            {
                foreach (Vector2Int d in Neighbors8)
                {
                    Vector2Int n = cell + d;
                    if (!board.IsInBounds(n) || hole.Contains(n))
                    {
                        continue;
                    }

                    contour.Add(n);
                }
            }

            if (contour.Count == 0)
            {
                fill = 0f;
                return false;
            }

            int occupied = 0;
            foreach (Vector2Int c in contour)
            {
                if (board.IsOccupied(c))
                {
                    ++occupied;
                }
            }

            fill = (float)occupied / contour.Count;
            return true;
        }

        private static HashSet<int> FindExactFitIds(List<Vector2Int> holeCells)
        {
            HashSet<int> ids = new();
            int holeSize = holeCells.Count;

            for (int id = BlockBlastCatalog.MinId; id <= BlockBlastCatalog.MaxId; ++id)
            {
                if (!HospitalityPiecePolicy.IsAllowed(id))
                {
                    continue;
                }

                IReadOnlyList<Vector2Int> offsets = BlockBlastCatalog.GetOffsets(id);
                if (offsets == null || offsets.Count != holeSize)
                {
                    continue;
                }

                if (ExactFits(offsets, holeCells))
                {
                    ids.Add(id);
                }
            }

            return ids;
        }

        private static bool ExactFits(IReadOnlyList<Vector2Int> offsets, List<Vector2Int> holeCells)
        {
            HashSet<Vector2Int> hole = new(holeCells);
            Vector2Int first = offsets[0];

            foreach (Vector2Int anchor in holeCells)
            {
                Vector2Int pivot = anchor - first;
                HashSet<Vector2Int> covered = new(offsets.Count);
                bool ok = true;

                for (int i = 0; i < offsets.Count; ++i)
                {
                    Vector2Int cell = pivot + offsets[i];
                    if (!hole.Contains(cell))
                    {
                        ok = false;
                        break;
                    }

                    covered.Add(cell);
                }

                if (ok && covered.Count == hole.Count)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
