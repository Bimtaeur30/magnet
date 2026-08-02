using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockBlast
{
    /// <summary>
    /// BlockBlast! 핸드오프 보고서 §7의 42종 블록 카탈로그.
    /// rows는 행별 비트마스크(bit 0 = 왼쪽 셀)이며, 회전형이 각각 별도 ID로 존재하므로
    /// 스폰 시 추가 회전을 적용하지 않는다.
    /// </summary>
    public static class BlockBlastCatalog
    {
        public const int MinId = 1;
        public const int MaxId = 42;

        /// <summary>produceRandomId 풀 — 핸드오프 §11: 2..42.</summary>
        public const int RandomPoolMin = 2;
        public const int RandomPoolMax = 42;

        /// <summary>randomNoDie 풀 — 핸드오프 §11: 2..30.</summary>
        private const int NoDiePoolMin = 2;
        private const int NoDiePoolMax = 30;

        /// <summary>
        /// 1370 근사 풀에서 제외하는 ID — 대각 3칸 계열 (§8.1 미관측 + 원작 체감상 미등장).
        /// 대형·장블록(5칸+)은 실플레이에서 등장하므로 포함하되 가중치로 빈도를 낮춘다
        /// (검증 500건의 미관측은 합성 랜덤 보드에서 완주 검증을 통과 못 한 왜곡으로 판단).
        /// </summary>
        private static readonly HashSet<int> FillPoolExcluded = new() { 39, 40, 41 };

        /// <summary>ID(1-based) → 행별 비트마스크. 핸드오프 §7 그대로.</summary>
        private static readonly int[][] RowMasks =
        {
            null,                            // 0 (미사용)
            new[] { 1 },                     // 1: 1x1
            new[] { 1, 1 },                  // 2: 세로 2
            new[] { 3 },                     // 3: 가로 2
            new[] { 1, 1, 1 },               // 4: 세로 3
            new[] { 7 },                     // 5: 가로 3
            new[] { 3, 2 },                  // 6: 3칸 ㄱ
            new[] { 1, 1, 1, 1 },            // 7: 세로 4
            new[] { 4, 7 },                  // 8: L4
            new[] { 3, 3 },                  // 9: 2x2
            new[] { 2, 7 },                  // 10: T4
            new[] { 31 },                    // 11: 가로 5
            new[] { 7, 1, 1 },               // 12: 큰 L5
            new[] { 7, 7, 7 },               // 13: 3x3 꽉참
            new[] { 3, 6 },                  // 14: S4
            new[] { 3, 1 },                  // 15: 3칸 ㄱ
            new[] { 2, 3, 1 },               // 16: S4 세로
            new[] { 15 },                    // 17: 가로 4
            new[] { 6, 3 },                  // 18: Z4
            new[] { 1, 3, 2 },               // 19: Z4 세로
            new[] { 2, 3, 2 },               // 20: T4 세로
            new[] { 7, 4, 4 },               // 21: 큰 L5
            new[] { 1, 1, 1, 1, 1 },         // 22: 세로 5
            new[] { 4, 4, 7 },               // 23: 큰 L5
            new[] { 1, 1, 7 },               // 24: 큰 L5
            new[] { 1, 3, 1 },               // 25: T4 세로
            new[] { 7, 2 },                  // 26: T4
            new[] { 2, 3 },                  // 27: 3칸 ㄱ
            new[] { 1, 3 },                  // 28: 3칸 ㄱ
            new[] { 1, 1, 3 },               // 29: L4 세로
            new[] { 7, 1 },                  // 30: J4
            new[] { 3, 2, 2 },               // 31: J4 세로
            new[] { 3, 1, 1 },               // 32: L4 세로
            new[] { 1, 7 },                  // 33: J4
            new[] { 7, 4 },                  // 34: L4
            new[] { 7, 7 },                  // 35: 3x2 직사각
            new[] { 3, 3, 3 },               // 36: 2x3 직사각
            new[] { 2, 1 },                  // 37: 대각 2
            new[] { 1, 2 },                  // 38: 대각 2
            new[] { 4, 2, 1 },               // 39: 대각 3
            new[] { 1, 2, 4 },               // 40: 대각 3
            new[] { 1, 2, 4 },               // 41: 대각 3 (런타임상 40과 동일)
            new[] { 2, 2, 3 },               // 42: J4 세로
        };

        private static readonly IReadOnlyList<Vector2Int>[] Offsets = BuildOffsets();

        public static IReadOnlyList<Vector2Int> GetOffsets(int id)
        {
            return Offsets[id];
        }

        /// <summary>randomNoDie 후보 풀 (2..30) — 호출자가 셔플해서 쓴다.</summary>
        public static int[] BuildNoDiePool()
        {
            int[] pool = new int[NoDiePoolMax - NoDiePoolMin + 1];
            for (int i = 0; i < pool.Length; ++i)
            {
                pool[i] = NoDiePoolMin + i;
            }

            return pool;
        }

        /// <summary>1370 근사 풀의 ID 배열 (2..42 − 대각 3칸). FillPoolWeights와 인덱스 대응.</summary>
        public static readonly int[] FillPoolIds = BuildFillPoolIds();

        /// <summary>
        /// 1370 근사 추첨 가중치 (셀 수 기반). 완주 검증이 소형 블록에 유리하므로
        /// 4칸을 기준(1.0)으로 대형은 낮게, 소형도 약간 낮게 잡아 원작 체감 분포에 맞춘다.
        /// </summary>
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
            _ => 0.35f, // 9칸 (3x3 꽉참)
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
