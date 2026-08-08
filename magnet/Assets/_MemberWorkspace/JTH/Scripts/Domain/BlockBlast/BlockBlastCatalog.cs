using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockBlast
{
    public static class BlockBlastCatalog
    {
        public const int MinId = 1;
        public const int MaxId = 42;

        public const int RandomPoolMin = 2;
        public const int RandomPoolMax = 42;

        private const int NoDiePoolMin = 2;
        private const int NoDiePoolMax = 30;

        private static readonly HashSet<int> FillPoolExcluded = new() { 39, 40, 41 };

        private static readonly int[][] RowMasks =
        {
            null,
            new[] { 1 },
            new[] { 1, 1 },
            new[] { 3 },
            new[] { 1, 1, 1 },
            new[] { 7 },
            new[] { 3, 2 },
            new[] { 1, 1, 1, 1 },
            new[] { 4, 7 },
            new[] { 3, 3 },
            new[] { 2, 7 },
            new[] { 31 },
            new[] { 7, 1, 1 },
            new[] { 7, 7, 7 },
            new[] { 3, 6 },
            new[] { 3, 1 },
            new[] { 2, 3, 1 },
            new[] { 15 },
            new[] { 6, 3 },
            new[] { 1, 3, 2 },
            new[] { 2, 3, 2 },
            new[] { 7, 4, 4 },
            new[] { 1, 1, 1, 1, 1 },
            new[] { 4, 4, 7 },
            new[] { 1, 1, 7 },
            new[] { 1, 3, 1 },
            new[] { 7, 2 },
            new[] { 2, 3 },
            new[] { 1, 3 },
            new[] { 1, 1, 3 },
            new[] { 7, 1 },
            new[] { 3, 2, 2 },
            new[] { 3, 1, 1 },
            new[] { 1, 7 },
            new[] { 7, 4 },
            new[] { 7, 7 },
            new[] { 3, 3, 3 },
            new[] { 2, 1 },
            new[] { 1, 2 },
            new[] { 4, 2, 1 },
            new[] { 1, 2, 4 },
            new[] { 1, 2, 4 },
            new[] { 2, 2, 3 },
        };

        private static readonly IReadOnlyList<Vector2Int>[] Offsets = BuildOffsets();

        public static IReadOnlyList<Vector2Int> GetOffsets(int id)
        {
            return Offsets[id];
        }

        public static int[] BuildNoDiePool()
        {
            int[] pool = new int[NoDiePoolMax - NoDiePoolMin + 1];
            for (int i = 0; i < pool.Length; ++i)
            {
                pool[i] = NoDiePoolMin + i;
            }

            return pool;
        }

        public static readonly int[] FillPoolIds = BuildFillPoolIds();

        public static readonly float[] FillPoolWeights = BuildFillPoolWeights();

        public static float FillPoolWeightTotal { get; private set; }

        private static int[] BuildFillPoolIds()
        {
            List<int> pool = new(RandomPoolMax - RandomPoolMin + 1);
            for (int id = RandomPoolMin; id <= RandomPoolMax; ++id)
            {
                if (FillPoolExcluded.Contains(id))
                {
                    continue;
                }

                pool.Add(id);
            }

            return pool.ToArray();
        }

        private static float[] BuildFillPoolWeights()
        {
            float[] weights = new float[FillPoolIds.Length];
            for (int i = 0; i < FillPoolIds.Length; ++i)
            {
                weights[i] = WeightByCellCount(Offsets[FillPoolIds[i]].Count);
                FillPoolWeightTotal += weights[i];
            }

            return weights;
        }

        private static float WeightByCellCount(int cells) => cells switch
        {
            <= 2 => 0.6f,
            3 => 0.8f,
            4 => 1.0f,
            5 => 0.5f,
            6 => 0.5f,
            _ => 0.35f,
        };

        private static IReadOnlyList<Vector2Int>[] BuildOffsets()
        {
            IReadOnlyList<Vector2Int>[] offsets = new IReadOnlyList<Vector2Int>[MaxId + 1];
            for (int id = MinId; id <= MaxId; ++id)
            {
                int[] rows = RowMasks[id];
                List<Vector2Int> cells = new();
                for (int row = 0; row < rows.Length; ++row)
                {
                    for (int col = 0; (rows[row] >> col) != 0; ++col)
                    {
                        if ((rows[row] & (1 << col)) != 0)
                        {
                            cells.Add(new Vector2Int(col, row));
                        }
                    }
                }

                offsets[id] = cells;
            }

            return offsets;
        }
    }
}
