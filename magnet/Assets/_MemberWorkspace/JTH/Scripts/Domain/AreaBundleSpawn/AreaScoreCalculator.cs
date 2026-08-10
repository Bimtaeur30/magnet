using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    public static class AreaScoreCalculator
    {
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
                    if (visited[x, y])
                    {
                        continue;
                    }

                    bool occupied = board.IsOccupied(new Vector2Int(x, y));
                    List<Vector2Int> cells = Flood(board, visited, x, y, occupied);
                    AreaComponentScore component = ScoreComponent(cells, occupied, cellCount, tuning);
                    components.Add(component);
                    baseTotal += component.Total;
                }
            }

            int rectCount = CountRectangles(board);
            float rectPenalty = tuning.rectCountPenalty * rectCount;
            int areaCount = components.Count;
            float areaCountPenalty = tuning.areaCountPenalty * areaCount;
            return new AreaScoreResult(
                baseTotal - rectPenalty - areaCountPenalty,
                components,
                rectCount,
                baseTotal,
                rectPenalty,
                areaCount,
                areaCountPenalty);
        }

        public static float ScoreTotal(BoardGrid board, AreaScoreTuning tuning) =>
            Score(board, tuning).Total;

        public static int CountRectangles(BoardGrid board)
        {
            int n = board.BoardSize;
            bool[,] occupiedMask = new bool[n, n];

            for (int x = 0; x < n; ++x)
            {
                for (int y = 0; y < n; ++y)
                {
                    occupiedMask[x, y] = board.IsOccupied(new Vector2Int(x, y));
                }
            }

            return PartitionCount(occupiedMask);
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

        private static int PartitionCount(bool[,] mask)
        {
            int n = mask.GetLength(0);
            int[,] prefix = new int[n + 1, n + 1];
            int count = 0;

            while (TryFindBestRectangle(mask, prefix, n, out int x0, out int y0, out int width, out int height))
            {
                Carve(mask, x0, y0, width, height);
                ++count;
            }

            return count;
        }

        private static bool TryFindBestRectangle(
            bool[,] mask,
            int[,] prefix,
            int n,
            out int bestX,
            out int bestY,
            out int bestW,
            out int bestH)
        {
            RebuildPrefix(mask, prefix, n);

            bestX = 0;
            bestY = 0;
            bestW = 0;
            bestH = 0;
            int bestArea = 0;
            bool found = false;

            for (int y0 = 0; y0 < n; ++y0)
            {
                for (int x0 = 0; x0 < n; ++x0)
                {
                    if (!mask[x0, y0])
                    {
                        continue;
                    }

                    for (int y1 = y0; y1 < n; ++y1)
                    {
                        for (int x1 = x0; x1 < n; ++x1)
                        {
                            int width = x1 - x0 + 1;
                            int height = y1 - y0 + 1;
                            int area = width * height;
                            if (area < bestArea)
                            {
                                continue;
                            }

                            if (RectSum(prefix, x0, y0, x1, y1) != area)
                            {
                                continue;
                            }

                            if (!found || IsBetter(area, y0, x0, width, bestArea, bestY, bestX, bestW))
                            {
                                found = true;
                                bestArea = area;
                                bestX = x0;
                                bestY = y0;
                                bestW = width;
                                bestH = height;
                            }
                        }
                    }
                }
            }

            return found;
        }

        private static bool IsBetter(
            int area, int y, int x, int width,
            int bestArea, int bestY, int bestX, int bestWidth)
        {
            if (area != bestArea)
            {
                return area > bestArea;
            }

            if (y != bestY)
            {
                return y < bestY;
            }

            if (x != bestX)
            {
                return x < bestX;
            }

            return width > bestWidth;
        }

        private static void RebuildPrefix(bool[,] mask, int[,] prefix, int n)
        {
            for (int x = 0; x <= n; ++x)
            {
                prefix[x, 0] = 0;
            }

            for (int y = 0; y <= n; ++y)
            {
                prefix[0, y] = 0;
            }

            for (int x = 0; x < n; ++x)
            {
                for (int y = 0; y < n; ++y)
                {
                    prefix[x + 1, y + 1] = (mask[x, y] ? 1 : 0)
                        + prefix[x, y + 1]
                        + prefix[x + 1, y]
                        - prefix[x, y];
                }
            }
        }

        private static int RectSum(int[,] prefix, int x0, int y0, int x1, int y1) =>
            prefix[x1 + 1, y1 + 1]
            - prefix[x0, y1 + 1]
            - prefix[x1 + 1, y0]
            + prefix[x0, y0];

        private static void Carve(bool[,] mask, int x0, int y0, int width, int height)
        {
            for (int x = x0; x < x0 + width; ++x)
            {
                for (int y = y0; y < y0 + height; ++y)
                {
                    mask[x, y] = false;
                }
            }
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
