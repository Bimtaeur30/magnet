using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    public static class AreaScoreCalculator
    {
        private const int BridgeSplitMinPartSize = 4;

        private static readonly Vector2Int[] Cardinals =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1)
        };

        public static AreaScoreResult Score(BoardGrid board, AreaScoreTuning tuning)
        {
            int n = board.BoardSize;
            int cellCount = n * n;
            bool[,] visited = new bool[n, n];
            List<AreaComponentScore> components = new();
            float baseTotal = 0f;

            for (int x = 0; x < n; ++x)
            {
                for (int y = 0; y < n; ++y)
                {
                    if (visited[x, y] || board.IsOccupied(new Vector2Int(x, y)))
                    {
                        continue;
                    }

                    List<Vector2Int> emptyCells = Flood(board, visited, x, y, occupied: false);
                    AreaComponentScore emptyComponent = ScoreComponent(emptyCells, occupied: false, cellCount, tuning);
                    components.Add(emptyComponent);
                    baseTotal += emptyComponent.Total;
                }
            }

            AddOccupiedBridgeSplitComponents(board, visited, components, ref baseTotal, cellCount, tuning);

            int cornerRectArea = MinCornerCoverRectArea(board);
            float cornerRectPenalty = tuning.cornerRectPenalty * cornerRectArea;
            int areaCount = components.Count;
            float areaCountPenalty = tuning.areaCountPenalty * areaCount;
            return new AreaScoreResult(
                baseTotal - cornerRectPenalty - areaCountPenalty,
                components,
                cornerRectArea,
                baseTotal,
                cornerRectPenalty,
                areaCount,
                areaCountPenalty);
        }

        public static float ScoreTotal(BoardGrid board, AreaScoreTuning tuning) =>
            Score(board, tuning).Total;

        /// <summary>
        /// 보드 네 모서리 각각을 꼭짓점으로, 모든 찬 칸을 덮는 축정렬 직사각 면적 중 최솟값.
        /// 찬 칸이 없으면 0.
        /// </summary>
        public static int MinCornerCoverRectArea(BoardGrid board)
        {
            int n = board.BoardSize;
            int minX = n;
            int maxX = -1;
            int minY = n;
            int maxY = -1;

            for (int x = 0; x < n; ++x)
            {
                for (int y = 0; y < n; ++y)
                {
                    if (!board.IsOccupied(new Vector2Int(x, y)))
                    {
                        continue;
                    }

                    if (x < minX)
                    {
                        minX = x;
                    }

                    if (x > maxX)
                    {
                        maxX = x;
                    }

                    if (y < minY)
                    {
                        minY = y;
                    }

                    if (y > maxY)
                    {
                        maxY = y;
                    }
                }
            }

            if (maxX < 0)
            {
                return 0;
            }

            int fromBottomLeft = (maxX + 1) * (maxY + 1);
            int fromBottomRight = (n - minX) * (maxY + 1);
            int fromTopLeft = (maxX + 1) * (n - minY);
            int fromTopRight = (n - minX) * (n - minY);

            int best = fromBottomLeft;
            if (fromBottomRight < best)
            {
                best = fromBottomRight;
            }

            if (fromTopLeft < best)
            {
                best = fromTopLeft;
            }

            if (fromTopRight < best)
            {
                best = fromTopRight;
            }

            return best;
        }

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
            return new AreaComponentScore(occupied, size, baseScore);
        }

        /// <summary>
        /// size ≤ emptyTinyMaxSize(tiny로 보는 최대 크기)면 고정 패널티.
        /// 넘으면: (size − (emptyTinyMaxSize+1)) × emptyFullScore / (boardCellCount − (emptyTinyMaxSize+1)).
        /// </summary>
        public static float ScoreEmpty(int size, int boardCellCount, AreaScoreTuning tuning)
        {
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

        /// <summary>
        /// size ≤ filledTinyMaxSize(tiny로 보는 최대 크기)면 고정 패널티.
        /// 넘으면: (size − (filledTinyMaxSize+1)) × filledFullScore / (boardCellCount − (filledTinyMaxSize+1)).
        /// </summary>
        public static float ScoreFilled(int size, int boardCellCount, AreaScoreTuning tuning)
        {
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

        /// <summary>
        /// 찬 칸 Area: 4연결로 묶은 뒤, 한 칸을 제거했을 때 양쪽이 각각
        /// <see cref="BridgeSplitMinPartSize"/> 이상이면 그 칸(다리)에서 끊는다.
        /// 짧은 돌출은 한 Area로 유지. 빈 칸은 이 경로를 쓰지 않는다.
        /// </summary>
        private static void AddOccupiedBridgeSplitComponents(
            BoardGrid board,
            bool[,] visited,
            List<AreaComponentScore> components,
            ref float baseTotal,
            int cellCount,
            AreaScoreTuning tuning)
        {
            int n = board.BoardSize;
            for (int x = 0; x < n; ++x)
            {
                for (int y = 0; y < n; ++y)
                {
                    if (visited[x, y] || !board.IsOccupied(new Vector2Int(x, y)))
                    {
                        continue;
                    }

                    List<Vector2Int> raw = Flood(board, visited, x, y, occupied: true);
                    List<List<Vector2Int>> parts = SplitAtBridges(raw, BridgeSplitMinPartSize);
                    for (int i = 0; i < parts.Count; ++i)
                    {
                        AreaComponentScore component = ScoreComponent(parts[i], occupied: true, cellCount, tuning);
                        components.Add(component);
                        baseTotal += component.Total;
                    }
                }
            }
        }

        /// <summary>
        /// 관절점(다리 칸)을 찾아, 끊으면 큰 덩어리가 둘 이상일 때만 분할을 재귀 적용한다.
        /// </summary>
        private static List<List<Vector2Int>> SplitAtBridges(List<Vector2Int> cells, int minPartSize)
        {
            if (cells.Count < minPartSize * 2 + 1)
            {
                return new List<List<Vector2Int>>(1) { cells };
            }

            List<Vector2Int> ordered = new(cells);
            ordered.Sort(static (a, b) =>
            {
                int cx = a.x.CompareTo(b.x);
                return cx != 0 ? cx : a.y.CompareTo(b.y);
            });

            HashSet<Vector2Int> set = new(cells);
            for (int i = 0; i < ordered.Count; ++i)
            {
                Vector2Int cut = ordered[i];
                List<List<Vector2Int>> parts = FloodPartsExcluding(set, cut);
                int largeCount = 0;
                for (int p = 0; p < parts.Count; ++p)
                {
                    if (parts[p].Count >= minPartSize)
                    {
                        ++largeCount;
                    }
                }

                if (largeCount < 2)
                {
                    continue;
                }

                List<List<Vector2Int>> result = new(parts.Count + 1);
                for (int p = 0; p < parts.Count; ++p)
                {
                    List<List<Vector2Int>> nested = SplitAtBridges(parts[p], minPartSize);
                    result.AddRange(nested);
                }

                result.Add(new List<Vector2Int>(1) { cut });
                return result;
            }

            return new List<List<Vector2Int>>(1) { cells };
        }

        private static List<List<Vector2Int>> FloodPartsExcluding(HashSet<Vector2Int> set, Vector2Int exclude)
        {
            HashSet<Vector2Int> seen = new();
            List<List<Vector2Int>> parts = new();

            foreach (Vector2Int start in set)
            {
                if (start == exclude || !seen.Add(start))
                {
                    continue;
                }

                List<Vector2Int> part = new();
                Queue<Vector2Int> queue = new();
                queue.Enqueue(start);

                while (queue.Count > 0)
                {
                    Vector2Int cur = queue.Dequeue();
                    part.Add(cur);

                    foreach (Vector2Int d in Cardinals)
                    {
                        Vector2Int next = new(cur.x + d.x, cur.y + d.y);
                        if (next == exclude || !set.Contains(next) || !seen.Add(next))
                        {
                            continue;
                        }

                        queue.Enqueue(next);
                    }
                }

                parts.Add(part);
            }

            return parts;
        }

        /// <summary>
        /// startX와 startY부터 시작해서 그 곳이 visited가 false라면 Area를 하나 만들어서 반환한다. 만약 텅 빈 보드라면 처음에
        /// Area가 0,0에서부터 시작해서 BFS를 사용해 끝까지 하나의 Area로 묶은 후 좌표들을 반환.
        /// </summary>
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
    }
}
