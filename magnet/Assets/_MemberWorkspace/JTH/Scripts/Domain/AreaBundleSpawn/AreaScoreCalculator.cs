using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    /// <summary>
    /// 찬/빈 4-연결 Area 점수 + (점수≥0 찬 Area) 변 보너스.
    /// 수치는 <see cref="AreaScoreTuning"/> (SO)로 조정.
    /// </summary>
    public static class AreaScoreCalculator
    {
        private static readonly Vector2Int[] Cardinals =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1)
        };

        private static readonly AreaScoreTuning Fallback = AreaScoreTuning.GrillDefault();

        public static AreaScoreResult Score(BoardGrid board, AreaScoreTuning tuning = null)
        {
            tuning ??= Fallback;
            int n = board.BoardSize;
            int cellCount = n * n;
            bool[,] visited = new bool[n, n];
            List<AreaComponentScore> components = new();
            float total = 0f;

            for (int x = 0; x < n; ++x)
            {
                for (int y = 0; y < n; ++y)
                {
                    if (visited[x, y])
                    {
                        continue;
                    }

                    bool occupied = board.IsOccupied(new Vector2Int(x, y));
                    List<Vector2Int> cells = Flood(board, visited, x, y, occupied);
                    AreaComponentScore component = ScoreComponent(cells, occupied, cellCount, tuning);
                    components.Add(component);
                    total += component.Total;
                }
            }

            return new AreaScoreResult(total, components);
        }

        public static float ScoreTotal(BoardGrid board, AreaScoreTuning tuning = null) =>
            Score(board, tuning).Total;

        private static AreaComponentScore ScoreComponent(
            List<Vector2Int> cells,
            bool occupied,
            int boardCellCount,
            AreaScoreTuning tuning)
        {
            int size = cells.Count;
            float baseScore = occupied
                ? ScoreFilled(size, boardCellCount, tuning)
                : ScoreEmpty(size, boardCellCount, tuning);
            int sideCount = 0;
            float sideBonus = 0f;

            if (occupied && baseScore >= 0f)
            {
                sideCount = CountOrthogonalSides(cells);
                sideBonus = SideBonus(sideCount, tuning);
            }

            return new AreaComponentScore(occupied, size, sideCount, baseScore, sideBonus);
        }

        public static float ScoreEmpty(int size, int boardCellCount, AreaScoreTuning tuning = null)
        {
            tuning ??= Fallback;
            if (size <= tuning.emptyTinyMaxSize)
            {
                return tuning.emptyTinyPenalty;
            }

            int span = boardCellCount - (tuning.emptyTinyMaxSize + 1);
            if (span <= 0)
            {
                return 0f;
            }

            return tuning.emptyFullScore * (size - (tuning.emptyTinyMaxSize + 1)) / span;
        }

        public static float ScoreFilled(int size, int boardCellCount, AreaScoreTuning tuning = null)
        {
            tuning ??= Fallback;
            if (size <= tuning.filledTinyMaxSize)
            {
                return tuning.filledTinyPenalty;
            }

            int span = boardCellCount - (tuning.filledTinyMaxSize + 1);
            if (span <= 0)
            {
                return 0f;
            }

            return tuning.filledFullScore * (size - (tuning.filledTinyMaxSize + 1)) / span;
        }

        public static float SideBonus(int sideCount, AreaScoreTuning tuning = null)
        {
            tuning ??= Fallback;
            if (sideCount <= tuning.sideBonusIdealMax)
            {
                return tuning.sideBonusAtIdeal;
            }

            return tuning.sideBonusAtIdeal
                - tuning.sideBonusPerTwoSides * (sideCount - tuning.sideBonusIdealMax) / 2f;
        }

        private static List<Vector2Int> Flood(BoardGrid board, bool[,] visited, int startX, int startY, bool occupied)
        {
            int n = board.BoardSize;
            List<Vector2Int> cells = new();
            Queue<Vector2Int> queue = new();
            Vector2Int start = new(startX, startY);
            queue.Enqueue(start);
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

                    if (board.IsOccupied(new Vector2Int(nx, ny)) != occupied)
                    {
                        continue;
                    }

                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }

            return cells;
        }

        /// <summary>
        /// 직교 다각형의 직선 변 개수 — 경계 단위 변을 같은 방향·연속이면 하나로 합친 개수.
        /// 직·정사각형은 크기와 무관하게 4.
        /// </summary>
        public static int CountOrthogonalSides(IReadOnlyList<Vector2Int> cells)
        {
            HashSet<Vector2Int> set = new(cells);
            // 단위 경계 변: (ax, ay, orient) — orient 0=가로(점 y=ay, x=ax..ax+1), 1=세로(점 x=ax, y=ay..ay+1)
            HashSet<long> boundary = new();

            foreach (Vector2Int c in cells)
            {
                // 남 (y-1 쪽): 가로 변 (c.x, c.y, H)
                if (!set.Contains(new Vector2Int(c.x, c.y - 1)))
                {
                    boundary.Add(PackEdge(c.x, c.y, 0));
                }

                // 북: 가로 변 (c.x, c.y+1, H)
                if (!set.Contains(new Vector2Int(c.x, c.y + 1)))
                {
                    boundary.Add(PackEdge(c.x, c.y + 1, 0));
                }

                // 서: 세로 변 (c.x, c.y, V)
                if (!set.Contains(new Vector2Int(c.x - 1, c.y)))
                {
                    boundary.Add(PackEdge(c.x, c.y, 1));
                }

                // 동: 세로 변 (c.x+1, c.y, V)
                if (!set.Contains(new Vector2Int(c.x + 1, c.y)))
                {
                    boundary.Add(PackEdge(c.x + 1, c.y, 1));
                }
            }

            // 동일 직선·인접 단위 변을 Union-Find로 합쳐 변 개수 = 집합 수
            Dictionary<long, long> parent = new();
            foreach (long e in boundary)
            {
                parent[e] = e;
            }

            foreach (long e in boundary)
            {
                UnpackEdge(e, out int ax, out int ay, out int orient);
                if (orient == 0)
                {
                    // 가로: 왼쪽/오른쪽으로 한 칸 이어진 가로 변
                    TryUnion(parent, boundary, e, PackEdge(ax - 1, ay, 0));
                    TryUnion(parent, boundary, e, PackEdge(ax + 1, ay, 0));
                }
                else
                {
                    TryUnion(parent, boundary, e, PackEdge(ax, ay - 1, 1));
                    TryUnion(parent, boundary, e, PackEdge(ax, ay + 1, 1));
                }
            }

            HashSet<long> roots = new();
            foreach (long e in boundary)
            {
                roots.Add(Find(parent, e));
            }

            return roots.Count;
        }

        private static void TryUnion(Dictionary<long, long> parent, HashSet<long> boundary, long a, long b)
        {
            if (!boundary.Contains(b))
            {
                return;
            }

            long ra = Find(parent, a);
            long rb = Find(parent, b);
            if (ra != rb)
            {
                parent[rb] = ra;
            }
        }

        private static long Find(Dictionary<long, long> parent, long x)
        {
            long p = parent[x];
            if (p != x)
            {
                parent[x] = Find(parent, p);
            }

            return parent[x];
        }

        private static long PackEdge(int a, int b, int orient) => ((long)(a + 512) << 22) | ((long)(b + 512) << 11) | (uint)orient;

        private static void UnpackEdge(long packed, out int a, out int b, out int orient)
        {
            orient = (int)(packed & 1);
            b = (int)((packed >> 11) & 0x7FF) - 512;
            a = (int)((packed >> 22) & 0x7FF) - 512;
        }
    }
}
