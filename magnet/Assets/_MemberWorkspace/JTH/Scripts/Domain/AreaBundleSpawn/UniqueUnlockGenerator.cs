using System;
using System.Collections.Generic;
using JTH.Scripts.Domain.BlockBlast;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    /// <summary>
    /// Unique: 막힌 1 + 당장 가능 2 → 둘을 놓아 라인클리어 후 막힌 피스 개방.
    /// 샘플 내에서는 단독 언락 불가(강A)를 우선하고, 없으면 둘로만 열리는 후보를 쓴다.
    /// </summary>
    public static class UniqueUnlockGenerator
    {
        public sealed class Result
        {
            public Result(
                int[] ids,
                List<IReadOnlyList<Vector2Int>> pieces,
                string reason,
                IReadOnlyList<AreaBundleExplainStep> explainSteps)
            {
                Ids = ids;
                Pieces = pieces;
                Reason = reason;
                ExplainSteps = explainSteps ?? Array.Empty<AreaBundleExplainStep>();
            }

            public int[] Ids { get; }
            public List<IReadOnlyList<Vector2Int>> Pieces { get; }
            public string Reason { get; }
            public IReadOnlyList<AreaBundleExplainStep> ExplainSteps { get; }
        }

        public static Result TryGenerate(
            BoardGrid board,
            Random rng,
            int sampleCount,
            Func<int, float> shapeWeight)
        {
            if (sampleCount < 1)
            {
                sampleCount = 1;
            }

            if (shapeWeight == null)
            {
                shapeWeight = _ => 1f;
            }

            int[] pool = BuildWeightedPoolIds(shapeWeight);
            if (pool.Length == 0)
            {
                return null;
            }

            float[] prefix = BuildPrefixSums(pool, shapeWeight);
            Result weakFallback = null;

            for (int sample = 0; sample < sampleCount; ++sample)
            {
                int a = PickWeighted(pool, prefix, rng);
                int b = PickWeighted(pool, prefix, rng);
                int c = PickWeighted(pool, prefix, rng);
                int[] triple = { a, b, c };
                IReadOnlyList<Vector2Int>[] offsets =
                {
                    BlockBlastCatalog.GetOffsets(a),
                    BlockBlastCatalog.GetOffsets(b),
                    BlockBlastCatalog.GetOffsets(c),
                };

                for (int blocked = 0; blocked < 3; ++blocked)
                {
                    if (PlacementService.CanPlaceAnywhere(offsets[blocked], board))
                    {
                        continue;
                    }

                    int u0 = (blocked + 1) % 3;
                    int u1 = (blocked + 2) % 3;
                    if (!PlacementService.CanPlaceAnywhere(offsets[u0], board)
                        || !PlacementService.CanPlaceAnywhere(offsets[u1], board))
                    {
                        continue;
                    }

                    // Result pieces: [blocked=0, unlock0=1, unlock1=2]
                    if (!TryUnlockWithLineClear(
                            board,
                            offsets[u0],
                            offsets[u1],
                            offsets[blocked],
                            unlock0Slot: 1,
                            unlock1Slot: 2,
                            blockedSlot: 0,
                            out List<AreaBundleExplainStep> explain))
                    {
                        continue;
                    }

                    int[] ids = { triple[blocked], triple[u0], triple[u1] };
                    var pieces = new List<IReadOnlyList<Vector2Int>>(3)
                    {
                        offsets[blocked],
                        offsets[u0],
                        offsets[u1],
                    };

                    bool aloneOpens = AloneUnlocks(board, offsets[u0], offsets[blocked])
                        || AloneUnlocks(board, offsets[u1], offsets[blocked]);
                    if (!aloneOpens)
                    {
                        return new Result(
                            ids,
                            pieces,
                            $"UniqueUnlock blocked={ids[0]} unlock=[{ids[1]},{ids[2]}]",
                            explain);
                    }

                    weakFallback ??= new Result(
                        ids,
                        pieces,
                        $"UniqueUnlock(weak-alone) blocked={ids[0]} unlock=[{ids[1]},{ids[2]}]",
                        explain);
                }
            }

            return weakFallback;
        }

        private static bool AloneUnlocks(
            BoardGrid board,
            IReadOnlyList<Vector2Int> piece,
            IReadOnlyList<Vector2Int> blocked)
        {
            int size = board.BoardSize;
            Vector2Int pivot = Vector2Int.zero;
            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    pivot.x = x;
                    pivot.y = y;
                    if (!PlacementService.CanPlace(piece, pivot, board))
                    {
                        continue;
                    }

                    BoardGrid after = board.Clone();
                    PlacementSimulator.PlaceAndClear(after, piece, pivot);
                    if (PlacementService.CanPlaceAnywhere(blocked, after))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryUnlockWithLineClear(
            BoardGrid board,
            IReadOnlyList<Vector2Int> unlockA,
            IReadOnlyList<Vector2Int> unlockB,
            IReadOnlyList<Vector2Int> blocked,
            int unlock0Slot,
            int unlock1Slot,
            int blockedSlot,
            out List<AreaBundleExplainStep> explain)
        {
            if (TryOrder(
                    board,
                    unlockA,
                    unlockB,
                    blocked,
                    firstSlot: unlock0Slot,
                    secondSlot: unlock1Slot,
                    blockedSlot,
                    out explain))
            {
                return true;
            }

            return TryOrder(
                board,
                unlockB,
                unlockA,
                blocked,
                firstSlot: unlock1Slot,
                secondSlot: unlock0Slot,
                blockedSlot,
                out explain);
        }

        private static bool TryOrder(
            BoardGrid board,
            IReadOnlyList<Vector2Int> first,
            IReadOnlyList<Vector2Int> second,
            IReadOnlyList<Vector2Int> blocked,
            int firstSlot,
            int secondSlot,
            int blockedSlot,
            out List<AreaBundleExplainStep> explain)
        {
            explain = null;
            int size = board.BoardSize;
            Vector2Int pivot = Vector2Int.zero;

            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    pivot.x = x;
                    pivot.y = y;
                    if (!PlacementService.CanPlace(first, pivot, board))
                    {
                        continue;
                    }

                    BoardGrid afterFirst = board.Clone();
                    int clear1 = PlacementSimulator.PlaceAndClear(afterFirst, first, pivot);
                    AreaBundleExplainStep step0 = BuildExplainStep(firstSlot, pivot, first);

                    for (int x2 = 0; x2 < size; ++x2)
                    {
                        for (int y2 = 0; y2 < size; ++y2)
                        {
                            pivot.x = x2;
                            pivot.y = y2;
                            if (!PlacementService.CanPlace(second, pivot, afterFirst))
                            {
                                continue;
                            }

                            BoardGrid afterSecond = afterFirst.Clone();
                            int clear2 = PlacementSimulator.PlaceAndClear(afterSecond, second, pivot);
                            if (clear1 + clear2 < 1)
                            {
                                continue;
                            }

                            if (!TryFindAnyPlacement(blocked, afterSecond, out Vector2Int blockedPivot))
                            {
                                continue;
                            }

                            explain = new List<AreaBundleExplainStep>(3)
                            {
                                step0,
                                BuildExplainStep(secondSlot, pivot, second),
                                BuildExplainStep(blockedSlot, blockedPivot, blocked),
                            };
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool TryFindAnyPlacement(
            IReadOnlyList<Vector2Int> piece,
            BoardGrid board,
            out Vector2Int pivot)
        {
            pivot = Vector2Int.zero;
            int size = board.BoardSize;
            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    pivot.x = x;
                    pivot.y = y;
                    if (PlacementService.CanPlace(piece, pivot, board))
                    {
                        return true;
                    }
                }
            }

            return false;
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

        private static int[] BuildWeightedPoolIds(Func<int, float> shapeWeight)
        {
            var list = new List<int>(40);
            for (int id = BlockBlastCatalog.RandomPoolMin; id <= BlockBlastCatalog.MaxId; ++id)
            {
                if (shapeWeight(id) <= 0f)
                {
                    continue;
                }

                list.Add(id);
            }

            return list.ToArray();
        }

        private static float[] BuildPrefixSums(int[] pool, Func<int, float> shapeWeight)
        {
            float[] prefix = new float[pool.Length];
            float sum = 0f;
            for (int i = 0; i < pool.Length; ++i)
            {
                sum += shapeWeight(pool[i]);
                prefix[i] = sum;
            }

            return prefix;
        }

        private static int PickWeighted(int[] pool, float[] prefix, Random rng)
        {
            float total = prefix[prefix.Length - 1];
            if (total <= 0f)
            {
                return pool[rng.Next(pool.Length)];
            }

            float roll = (float)(rng.NextDouble() * total);
            for (int i = 0; i < prefix.Length; ++i)
            {
                if (roll <= prefix[i])
                {
                    return pool[i];
                }
            }

            return pool[pool.Length - 1];
        }
    }
}
