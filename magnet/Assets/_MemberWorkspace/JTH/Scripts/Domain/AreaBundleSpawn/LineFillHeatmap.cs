using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    /// <summary>
    /// 행·열마다 (라인길이 − 빈칸수)를, 찬 칸에 맞닿은 빈 칸에만 가산한 히트맵.
    /// 완전 빈 줄은 0. 한 칸은 가로+세로 합.
    /// </summary>
    public static class LineFillHeatmap
    {
        public static int[,] Build(BoardGrid board)
        {
            int n = board.BoardSize;
            int[,] heat = new int[n, n];

            for (int y = 0; y < n; ++y)
            {
                ApplyLine(board, heat, horizontal: true, lineIndex: y);
            }

            for (int x = 0; x < n; ++x)
            {
                ApplyLine(board, heat, horizontal: false, lineIndex: x);
            }

            return heat;
        }

        public static int SumCells(int[,] heat, Vector2Int pivot, System.Collections.Generic.IReadOnlyList<Vector2Int> offsets)
        {
            int sum = 0;
            for (int i = 0; i < offsets.Count; ++i)
            {
                Vector2Int c = pivot + offsets[i];
                sum += heat[c.x, c.y];
            }

            return sum;
        }

        /// <summary>
        /// Σ heat − emptyPenalty × (heat==0 칸 수). 허공에 걸친 큰 피스를 깎는다.
        /// </summary>
        public static float ScorePlacement(
            int[,] heat,
            Vector2Int pivot,
            System.Collections.Generic.IReadOnlyList<Vector2Int> offsets,
            float emptyPenalty)
        {
            float sum = 0f;
            int zeroCount = 0;
            for (int i = 0; i < offsets.Count; ++i)
            {
                Vector2Int c = pivot + offsets[i];
                int h = heat[c.x, c.y];
                sum += h;
                if (h == 0)
                {
                    ++zeroCount;
                }
            }

            if (emptyPenalty <= 0f || zeroCount == 0)
            {
                return sum;
            }

            return sum - emptyPenalty * zeroCount;
        }

        /// <summary>절대 칸 목록용. HandCompare와 동일 식.</summary>
        public static float ScoreCells(
            int[,] heat,
            System.Collections.Generic.IReadOnlyList<Vector2Int> cells,
            float emptyPenalty)
        {
            float sum = 0f;
            int zeroCount = 0;
            for (int i = 0; i < cells.Count; ++i)
            {
                Vector2Int c = cells[i];
                int h = heat[c.x, c.y];
                sum += h;
                if (h == 0)
                {
                    ++zeroCount;
                }
            }

            if (emptyPenalty <= 0f || zeroCount == 0)
            {
                return sum;
            }

            return sum - emptyPenalty * zeroCount;
        }

        private static void ApplyLine(BoardGrid board, int[,] heat, bool horizontal, int lineIndex)
        {
            int n = board.BoardSize;
            int empty = 0;
            for (int i = 0; i < n; ++i)
            {
                Vector2Int cell = horizontal
                    ? new Vector2Int(i, lineIndex)
                    : new Vector2Int(lineIndex, i);
                if (!board.IsOccupied(cell))
                {
                    ++empty;
                }
            }

            if (empty == 0 || empty == n)
            {
                return;
            }

            int score = n - empty;
            for (int i = 0; i < n; ++i)
            {
                Vector2Int cell = horizontal
                    ? new Vector2Int(i, lineIndex)
                    : new Vector2Int(lineIndex, i);
                if (board.IsOccupied(cell))
                {
                    continue;
                }

                bool touchFilled = false;
                if (i > 0)
                {
                    Vector2Int left = horizontal
                        ? new Vector2Int(i - 1, lineIndex)
                        : new Vector2Int(lineIndex, i - 1);
                    if (board.IsOccupied(left))
                    {
                        touchFilled = true;
                    }
                }

                if (i < n - 1)
                {
                    Vector2Int right = horizontal
                        ? new Vector2Int(i + 1, lineIndex)
                        : new Vector2Int(lineIndex, i + 1);
                    if (board.IsOccupied(right))
                    {
                        touchFilled = true;
                    }
                }

                if (touchFilled)
                {
                    heat[cell.x, cell.y] += score;
                }
            }
        }
    }
}
