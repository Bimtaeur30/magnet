using System.Collections.Generic;
using JTH.Scripts.Domain.BlockBlast;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Placement;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    public static class UniqueUnlockGenerator
    {
        public sealed class Result
        {
            public Result(int[] ids, List<IReadOnlyList<Vector2Int>> pieces, string reason)
            {
                Ids = ids;
                Pieces = pieces;
                Reason = reason;
            }

            public int[] Ids { get; }
            public List<IReadOnlyList<Vector2Int>> Pieces { get; }
            public string Reason { get; }
        }

        private static readonly int[] PoolIds = BuildUniquePoolIds();

        public static Result TryGenerate(BoardGrid board, Random rng, int sampleCount)
        {
            if (sampleCount < 1)
            {
                sampleCount = 1;
            }

            for (int sample = 0; sample < sampleCount; ++sample)
            {
                int a = PoolIds[rng.Next(PoolIds.Length)];
                int b = PoolIds[rng.Next(PoolIds.Length)];
                int c = PoolIds[rng.Next(PoolIds.Length)];
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

                    if (!CanUnlockWithLineClear(board, offsets[u0], offsets[u1], offsets[blocked]))
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
                    return new Result(
                        ids,
                        pieces,
                        $"UniqueUnlock blocked={ids[0]} unlock=[{ids[1]},{ids[2]}]");
                }
            }

            return null;
        }

        private static bool CanUnlockWithLineClear(
            BoardGrid board,
            IReadOnlyList<Vector2Int> unlockA,
            IReadOnlyList<Vector2Int> unlockB,
            IReadOnlyList<Vector2Int> blocked)
        {
            return TryOrder(board, unlockA, unlockB, blocked)
                || TryOrder(board, unlockB, unlockA, blocked);
        }

        private static bool TryOrder(
            BoardGrid board,
            IReadOnlyList<Vector2Int> first,
            IReadOnlyList<Vector2Int> second,
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
                    if (!PlacementService.CanPlace(first, pivot, board))
                    {
                        continue;
                    }

                    BoardGrid afterFirst = board.Clone();
                    int clear1 = PlacementSimulator.PlaceAndClear(afterFirst, first, pivot);

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

                            if (PlacementService.CanPlaceAnywhere(blocked, afterSecond))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static int[] BuildUniquePoolIds()
        {
            var list = new List<int>(40);
            for (int id = BlockBlastCatalog.RandomPoolMin; id <= BlockBlastCatalog.MaxId; ++id)
            {
                list.Add(id);
            }

            return list.ToArray();
        }
    }
}
