using System.Collections.Generic;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.BlockSelection.Bundles
{
    /// <summary>
    /// 가중 랜덤으로 피스 3개를 뽑는 샘플러. 가중치 0(1x1·1x2 등 억지 블록)은 애초에 안 뽑힌다.
    /// 같은 모양 2개(페어)는 허용하되 3개(트리플)는 금지.
    /// </summary>
    public static class ShapeSampler
    {
        public const int PieceCount = 3;

        /// <summary>트리플 회피 재추첨 상한. 초과하면 (풀이 사실상 1종) 트리플 허용.</summary>
        private const int TripleRejectRetries = 8;

        /// <summary>
        /// 중복 허용 가중 추첨으로 canonical 3개를 고른 뒤 각각 랜덤 회전을 적용해 반환.
        /// 단 같은 모양 3개는 금지 — 마지막 슬롯이 트리플을 만들면 재추첨.
        /// 유효 가중치 항목이 하나도 없으면 null.
        /// </summary>
        public static List<IReadOnlyList<Vector2Int>> Sample3Rotated(IReadOnlyList<WeightedShape> pool, Random rng)
        {
            float totalWeight = 0f;
            foreach (WeightedShape entry in pool)
            {
                if (entry.Weight > 0f)
                {
                    totalWeight += entry.Weight;
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            List<IReadOnlyList<Vector2Int>> canonicals = new(PieceCount);
            List<IReadOnlyList<Vector2Int>> pieces = new(PieceCount);

            for (int i = 0; i < PieceCount; ++i)
            {
                IReadOnlyList<Vector2Int> canonical = PickWeighted(pool, totalWeight, rng);

                bool wouldBeTriple = i == PieceCount - 1
                    && ReferenceEquals(canonicals[0], canonicals[1]);
                if (wouldBeTriple)
                {
                    for (int retry = 0;
                        retry < TripleRejectRetries && ReferenceEquals(canonical, canonicals[0]);
                        ++retry)
                    {
                        canonical = PickWeighted(pool, totalWeight, rng);
                    }
                }

                canonicals.Add(canonical);
                pieces.Add(ShapeRotator.Rotate(canonical, rng.Next(4)));
            }

            return pieces;
        }

        private static IReadOnlyList<Vector2Int> PickWeighted(IReadOnlyList<WeightedShape> pool, float totalWeight, Random rng)
        {
            float roll = (float)(rng.NextDouble() * totalWeight);
            float accumulated = 0f;

            for (int i = 0; i < pool.Count; ++i)
            {
                if (pool[i].Weight <= 0f)
                {
                    continue;
                }

                accumulated += pool[i].Weight;
                if (roll < accumulated)
                {
                    return pool[i].CellOffsets;
                }
            }

            // 부동소수 누적 오차로 끝까지 못 고른 경우 마지막 유효 항목
            for (int i = pool.Count - 1; i >= 0; --i)
            {
                if (pool[i].Weight > 0f)
                {
                    return pool[i].CellOffsets;
                }
            }

            return null;
        }
    }
}
