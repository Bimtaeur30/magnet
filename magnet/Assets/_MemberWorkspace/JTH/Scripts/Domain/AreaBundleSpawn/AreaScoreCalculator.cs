using System.Collections.Generic;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    public static class AreaScoreCalculator
    {
        /// <summary>이 깊이 이하 홈/만은 절단하지 않음. 0 = 다중 run 갭이면 무조건 절단.</summary>
        public const int MaxNotchDepth = 0;

        private static readonly Vector2Int[] Cardinals =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1)
        };

        public static AreaScoreResult Score(BoardGrid board, AreaScoreTuning tuning)
        {
            IReadOnlyList<AreaPartition> partitions = Partition(board);
            List<AreaComponentScore> components = new(partitions.Count);
            float baseTotal = 0f;
            int cellCount = board.BoardSize * board.BoardSize;

            for (int i = 0; i < partitions.Count; ++i)
            {
                AreaPartition part = partitions[i];
                AreaComponentScore component = ScoreComponent(
                    part.Size,
                    part.Occupied,
                    cellCount,
                    tuning);
                components.Add(component);
                baseTotal += component.Total;
            }

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
        /// 빈 칸: 4연결. 찬 칸: 4연결 후 깊은 홈(깊이 &gt; MaxNotchDepth)만 축절단.
        /// </summary>
        public static IReadOnlyList<AreaPartition> Partition(BoardGrid board)
        {
            int n = board.BoardSize;
            bool[,] visited = new bool[n, n];
            List<AreaPartition> result = new();

            for (int x = 0; x < n; ++x)
            {
                for (int y = 0; y < n; ++y)
                {
                    if (visited[x, y] || board.IsOccupied(new Vector2Int(x, y)))
                    {
                        continue;
                    }

                    List<Vector2Int> emptyCells = Flood(board, visited, x, y, occupied: false);
                    result.Add(new AreaPartition(occupied: false, emptyCells));
                }
            }

            for (int x = 0; x < n; ++x)
            {
                for (int y = 0; y < n; ++y)
                {
                    if (visited[x, y] || !board.IsOccupied(new Vector2Int(x, y)))
                    {
                        continue;
                    }

                    List<Vector2Int> raw = Flood(board, visited, x, y, occupied: true);
                    List<List<Vector2Int>> parts = new();
                    SplitOccupiedOrtho(raw, parts);
                    for (int i = 0; i < parts.Count; ++i)
                    {
                        result.Add(new AreaPartition(occupied: true, parts[i]));
                    }
                }
            }

            return result;
        }

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
            int size,
            bool occupied,
            int boardCellCount,
            AreaScoreTuning tuning)
        {
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
        /// 깊은 홈(깊이 &gt; MaxNotchDepth)이 없을 때까지 축정렬 절단.
        /// 얕은 홈(≤ MaxNotchDepth)은 한 Area로 유지.
        /// </summary>
        private static void SplitOccupiedOrtho(List<Vector2Int> cells, List<List<Vector2Int>> sink)
        {
            if (cells.Count == 0)
            {
                return;
            }

            if (!TryFindDeepBalancedCut(cells, out bool splitByX, out int cutAfter))
            {
                sink.Add(cells);
                return;
            }

            List<Vector2Int> left = new(cells.Count);
            List<Vector2Int> right = new(cells.Count);
            for (int i = 0; i < cells.Count; ++i)
            {
                Vector2Int cell = cells[i];
                bool toLeft = splitByX ? cell.x <= cutAfter : cell.y <= cutAfter;
                if (toLeft)
                {
                    left.Add(cell);
                }
                else
                {
                    right.Add(cell);
                }
            }

            List<List<Vector2Int>> leftParts = FloodWithin(left);
            for (int i = 0; i < leftParts.Count; ++i)
            {
                SplitOccupiedOrtho(leftParts[i], sink);
            }

            List<List<Vector2Int>> rightParts = FloodWithin(right);
            for (int i = 0; i < rightParts.Count; ++i)
            {
                SplitOccupiedOrtho(rightParts[i], sink);
            }
        }

        /// <summary>모든 행·열에서 찬 칸이 한 구간이면 true.</summary>
        public static bool IsOrthoConvex(IReadOnlyList<Vector2Int> cells)
        {
            if (cells.Count <= 1)
            {
                return true;
            }

            BuildLineMaps(cells, out Dictionary<int, List<int>> xsByY, out Dictionary<int, List<int>> ysByX);

            foreach (KeyValuePair<int, List<int>> pair in xsByY)
            {
                if (CountRuns(pair.Value) > 1)
                {
                    return false;
                }
            }

            foreach (KeyValuePair<int, List<int>> pair in ysByX)
            {
                if (CountRuns(pair.Value) > 1)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 깊이 &gt; MaxNotchDepth인 다중 run 갭만 후보로, 절단 후 min(left,right) 최대 cut을 고른다.
        /// </summary>
        private static bool TryFindDeepBalancedCut(
            IReadOnlyList<Vector2Int> cells,
            out bool splitByX,
            out int cutAfter)
        {
            splitByX = true;
            cutAfter = 0;
            int bestScore = 0;
            bool found = false;

            BuildLineMaps(cells, out Dictionary<int, List<int>> xsByY, out Dictionary<int, List<int>> ysByX);
            HashSet<Vector2Int> set = new(cells);

            foreach (KeyValuePair<int, List<int>> pair in xsByY)
            {
                ConsiderDeepGapsAlongLine(
                    cells,
                    set,
                    pair.Key,
                    pair.Value,
                    splitByXCandidate: true,
                    ref bestScore,
                    ref found,
                    ref splitByX,
                    ref cutAfter);
            }

            foreach (KeyValuePair<int, List<int>> pair in ysByX)
            {
                ConsiderDeepGapsAlongLine(
                    cells,
                    set,
                    pair.Key,
                    pair.Value,
                    splitByXCandidate: false,
                    ref bestScore,
                    ref found,
                    ref splitByX,
                    ref cutAfter);
            }

            return found;
        }

        private static void ConsiderDeepGapsAlongLine(
            IReadOnlyList<Vector2Int> cells,
            HashSet<Vector2Int> set,
            int lineCoord,
            List<int> coords,
            bool splitByXCandidate,
            ref int bestScore,
            ref bool found,
            ref bool splitByX,
            ref int cutAfter)
        {
            coords.Sort();
            List<(int min, int max)> runs = BuildRuns(coords);
            if (runs.Count < 2)
            {
                return;
            }

            for (int r = 0; r < runs.Count - 1; ++r)
            {
                int gapMin = runs[r].max + 1;
                int gapMax = runs[r + 1].min - 1;
                if (gapMin > gapMax)
                {
                    continue;
                }

                int depth = MeasureGapDepth(set, splitByXCandidate, lineCoord, gapMin, gapMax);
                if (depth <= MaxNotchDepth)
                {
                    continue;
                }

                int mid = (runs[r].max + runs[r + 1].min) / 2;
                int left = 0;
                int right = 0;
                for (int i = 0; i < cells.Count; ++i)
                {
                    Vector2Int cell = cells[i];
                    int value = splitByXCandidate ? cell.x : cell.y;
                    if (value <= mid)
                    {
                        ++left;
                    }
                    else
                    {
                        ++right;
                    }
                }

                int score = left < right ? left : right;
                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                found = true;
                splitByX = splitByXCandidate;
                cutAfter = mid;
            }
        }

        /// <summary>
        /// 갭이 비어 있는 연속 줄 수(현재 줄 포함). 모양이 끝나거나 갭이 메워지면 중단.
        /// </summary>
        private static int MeasureGapDepth(
            HashSet<Vector2Int> set,
            bool gapAlongX,
            int lineCoord,
            int gapMin,
            int gapMax)
        {
            int depthNeg = CountEmptyGapExtent(set, gapAlongX, lineCoord, gapMin, gapMax, -1);
            int depthPos = CountEmptyGapExtent(set, gapAlongX, lineCoord, gapMin, gapMax, +1);
            return depthNeg > depthPos ? depthNeg : depthPos;
        }

        private static int CountEmptyGapExtent(
            HashSet<Vector2Int> set,
            bool gapAlongX,
            int startLine,
            int gapMin,
            int gapMax,
            int lineStep)
        {
            int depth = 0;
            for (int line = startLine; ; line += lineStep)
            {
                bool anyOnLine = false;
                bool gapOccupied = false;
                foreach (Vector2Int cell in set)
                {
                    int lineValue = gapAlongX ? cell.y : cell.x;
                    if (lineValue != line)
                    {
                        continue;
                    }

                    anyOnLine = true;
                    int gapValue = gapAlongX ? cell.x : cell.y;
                    if (gapValue >= gapMin && gapValue <= gapMax)
                    {
                        gapOccupied = true;
                        break;
                    }
                }

                if (!anyOnLine)
                {
                    break;
                }

                if (gapOccupied)
                {
                    break;
                }

                ++depth;
            }

            return depth;
        }

        private static void BuildLineMaps(
            IReadOnlyList<Vector2Int> cells,
            out Dictionary<int, List<int>> xsByY,
            out Dictionary<int, List<int>> ysByX)
        {
            xsByY = new Dictionary<int, List<int>>();
            ysByX = new Dictionary<int, List<int>>();
            for (int i = 0; i < cells.Count; ++i)
            {
                Vector2Int cell = cells[i];
                if (!xsByY.TryGetValue(cell.y, out List<int> xs))
                {
                    xs = new List<int>();
                    xsByY[cell.y] = xs;
                }

                xs.Add(cell.x);

                if (!ysByX.TryGetValue(cell.x, out List<int> ys))
                {
                    ys = new List<int>();
                    ysByX[cell.x] = ys;
                }

                ys.Add(cell.y);
            }
        }

        private static int CountRuns(List<int> coords)
        {
            if (coords.Count == 0)
            {
                return 0;
            }

            coords.Sort();
            return BuildRuns(coords).Count;
        }

        private static List<(int min, int max)> BuildRuns(List<int> sortedUniqueOrDup)
        {
            List<(int min, int max)> runs = new();
            if (sortedUniqueOrDup.Count == 0)
            {
                return runs;
            }

            int runMin = sortedUniqueOrDup[0];
            int runMax = sortedUniqueOrDup[0];
            for (int i = 1; i < sortedUniqueOrDup.Count; ++i)
            {
                int v = sortedUniqueOrDup[i];
                if (v == runMax || v == runMax + 1)
                {
                    runMax = v;
                    continue;
                }

                runs.Add((runMin, runMax));
                runMin = v;
                runMax = v;
            }

            runs.Add((runMin, runMax));
            return runs;
        }

        private static List<List<Vector2Int>> FloodWithin(List<Vector2Int> cells)
        {
            HashSet<Vector2Int> set = new(cells);
            HashSet<Vector2Int> seen = new();
            List<List<Vector2Int>> parts = new();

            for (int i = 0; i < cells.Count; ++i)
            {
                Vector2Int start = cells[i];
                if (!seen.Add(start))
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
                        if (!set.Contains(next) || !seen.Add(next))
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
