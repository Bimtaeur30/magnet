using System.Collections.Generic;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using Magnet.Contracts;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Simulation
{
    /// <summary>
    /// 핸드가 정해진 직후(3피스 확정 시점) 그 핸드로 낼 수 있는 <b>최적값</b>을 미리 구한다.
    /// 최적 = 3피스를 전부 놓았을 때 지울 수 있는 라인 수의 최대치.
    /// 플레이어가 마지막 배치까지 끝냈을 때 누적 클리어 수가 이 최대치와 같으면 "퍼펙트".
    /// </summary>
    public static class HandOptimalSolver
    {
        /// <summary>탐색 노드 상한. 넘으면 IsValid=false로 끊는다(퍼펙트 판정 포기).</summary>
        public const int DefaultNodeBudget = 1000000;

        public static HandOptimalResult Solve(
            BoardGrid start,
            IReadOnlyList<ShapeBlockData> pieces,
            int nodeBudget = DefaultNodeBudget)
        {
            if (start == null || pieces == null)
            {
                return HandOptimalResult.Unsolved;
            }

            List<IReadOnlyList<Vector2Int>> shapes = new List<IReadOnlyList<Vector2Int>>(pieces.Count);
            for (int i = 0; i < pieces.Count; ++i)
            {
                ShapeBlockData piece = pieces[i];
                if (piece?.CellOffsets == null || piece.CellOffsets.Count == 0)
                {
                    continue;
                }

                shapes.Add(piece.CellOffsets);
            }

            if (shapes.Count == 0)
            {
                return HandOptimalResult.Unsolved;
            }

            // 같은 모양은 서로 바꿔 놓아도 결과가 같다. 정규 id로 묶어 전치 중복을 접는다.
            int[] canonIds = BuildCanonicalIds(shapes, out int distinctCount);
            int[] remaining = new int[distinctCount];
            for (int i = 0; i < canonIds.Length; ++i)
            {
                ++remaining[canonIds[i]];
            }

            List<IReadOnlyList<Vector2Int>> canonShapes = new List<IReadOnlyList<Vector2Int>>(distinctCount);
            for (int id = 0; id < distinctCount; ++id)
            {
                for (int i = 0; i < canonIds.Length; ++i)
                {
                    if (canonIds[i] == id)
                    {
                        canonShapes.Add(shapes[i]);
                        break;
                    }
                }
            }

            int best = -1;
            int budget = nodeBudget;
            HashSet<(ulong, int)> visited = new HashSet<(ulong, int)>();

            bool complete = Search(start, canonShapes, remaining, 0, ref best, ref budget, visited);

            return new HandOptimalResult(best, complete && best >= 0);
        }

        /// <returns>예산 안에서 탐색을 끝냈으면 true, 중간에 끊겼으면 false.</returns>
        private static bool Search(
            BoardGrid grid,
            List<IReadOnlyList<Vector2Int>> canonShapes,
            int[] remaining,
            int clearedSoFar,
            ref int best,
            ref int budget,
            HashSet<(ulong, int)> visited)
        {
            if (AllPlaced(remaining))
            {
                if (clearedSoFar > best)
                {
                    best = clearedSoFar;
                }

                return true;
            }

            if (grid.TryPackBits(out ulong bits) && !visited.Add((bits, EncodeRemaining(remaining))))
            {
                return true;
            }

            for (int id = 0; id < canonShapes.Count; ++id)
            {
                if (remaining[id] == 0)
                {
                    continue;
                }

                IReadOnlyList<Vector2Int> offsets = canonShapes[id];
                --remaining[id];

                for (int x = 0; x < grid.BoardSize; ++x)
                {
                    for (int y = 0; y < grid.BoardSize; ++y)
                    {
                        Vector2Int pivot = new Vector2Int(x, y);
                        if (!PlacementService.CanPlace(offsets, pivot, grid))
                        {
                            continue;
                        }

                        if (--budget <= 0)
                        {
                            ++remaining[id];
                            return false;
                        }

                        BoardGrid next = grid.Clone();
                        int cleared = PlacementSimulator.PlaceAndClear(next, offsets, pivot);

                        if (!Search(next, canonShapes, remaining, clearedSoFar + cleared,
                                ref best, ref budget, visited))
                        {
                            ++remaining[id];
                            return false;
                        }
                    }
                }

                ++remaining[id];
            }

            return true;
        }

        private static bool AllPlaced(int[] remaining)
        {
            for (int i = 0; i < remaining.Length; ++i)
            {
                if (remaining[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>남은 피스 구성을 작은 정수로. 피스는 최대 3개라 자리당 2비트면 충분하다.</summary>
        private static int EncodeRemaining(int[] remaining)
        {
            int code = 0;
            for (int i = 0; i < remaining.Length; ++i)
            {
                code = (code << 2) | (remaining[i] & 0x3);
            }

            return code;
        }

        /// <summary>모양이 같은 피스끼리 같은 id를 준다.</summary>
        private static int[] BuildCanonicalIds(
            List<IReadOnlyList<Vector2Int>> shapes,
            out int distinctCount)
        {
            int[] ids = new int[shapes.Count];
            distinctCount = 0;

            for (int i = 0; i < shapes.Count; ++i)
            {
                int found = -1;
                for (int j = 0; j < i; ++j)
                {
                    if (SameShape(shapes[i], shapes[j]))
                    {
                        found = ids[j];
                        break;
                    }
                }

                ids[i] = found >= 0 ? found : distinctCount++;
            }

            return ids;
        }

        private static bool SameShape(IReadOnlyList<Vector2Int> a, IReadOnlyList<Vector2Int> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            HashSet<Vector2Int> set = new HashSet<Vector2Int>();
            for (int i = 0; i < a.Count; ++i)
            {
                set.Add(a[i]);
            }

            for (int i = 0; i < b.Count; ++i)
            {
                if (!set.Contains(b[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public readonly struct HandOptimalResult
    {
        public static readonly HandOptimalResult Unsolved = new HandOptimalResult(-1, false);

        public HandOptimalResult(int maxClearedLines, bool isValid)
        {
            MaxClearedLines = maxClearedLines;
            IsValid = isValid;
        }

        /// <summary>3피스를 전부 놓았을 때 지울 수 있는 라인 수의 최대치. 전부 놓을 수 없으면 -1.</summary>
        public int MaxClearedLines { get; }

        /// <summary>탐색이 끝까지 돌아 최적값을 신뢰할 수 있는지. false면 퍼펙트 판정을 하지 않는다.</summary>
        public bool IsValid { get; }
    }
}
