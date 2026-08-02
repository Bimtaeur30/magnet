using System;
using System.Collections.Generic;
using JTH.Scripts.Domain.BlockBlast;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.HybridSpawn
{
    /// <summary>
    /// 42-ID 카탈로그 기반 가중 추첨 풀. 가중치 0인 ID는 애초에 안 뽑힌다.
    /// 같은 ID 3개(트리플)와 직전 턴 트리플(다중집합 동일)은 재추첨으로 회피한다
    /// — 반복 억제 트레이트를 우회하는 특수 티어의 자체 반복 방지.
    /// </summary>
    public sealed class HybridPiecePool
    {
        public const int TripleSize = 3;

        /// <summary>회피 조건(전부 동일·직전 트리플) 재추첨 상한. 초과 시 마지막 후보 반환.</summary>
        private const int RejectRetries = 8;

        private readonly int[] _ids;
        private readonly float[] _weights;
        private readonly float _totalWeight;

        public bool IsEmpty => _totalWeight <= 0f;

        public HybridPiecePool(IReadOnlyList<int> candidateIds, Func<int, float> weightOf)
        {
            List<int> ids = new(candidateIds.Count);
            List<float> weights = new(candidateIds.Count);
            float total = 0f;

            foreach (int id in candidateIds)
            {
                float weight = weightOf(id);
                if (weight <= 0f)
                {
                    continue;
                }

                ids.Add(id);
                weights.Add(weight);
                total += weight;
            }

            _ids = ids.ToArray();
            _weights = weights.ToArray();
            _totalWeight = total;
        }

        /// <summary>
        /// 가중 독립 추첨 3회. 3개 전부 동일하거나 avoidTriple과 다중집합이 같으면 재추첨.
        /// 풀이 비어 있으면 null, 재추첨 상한 초과 시(풀이 사실상 1~2종) 마지막 후보를 그대로 반환.
        /// </summary>
        public int[] SampleTriple(Random rng, int[] avoidTriple)
        {
            if (IsEmpty)
            {
                return null;
            }

            int[] triple = new int[TripleSize];

            for (int retry = 0; retry <= RejectRetries; ++retry)
            {
                for (int i = 0; i < TripleSize; ++i)
                {
                    triple[i] = SampleId(rng);
                }

                bool allSame = triple[0] == triple[1] && triple[1] == triple[2];
                bool repeatsLast = avoidTriple != null && SameMultiset(triple, avoidTriple);

                if (!allSame && !repeatsLast)
                {
                    return triple;
                }
            }

            return triple;
        }

        private int SampleId(Random rng)
        {
            float roll = (float)(rng.NextDouble() * _totalWeight);

            for (int i = 0; i < _weights.Length; ++i)
            {
                roll -= _weights[i];
                if (roll <= 0f)
                {
                    return _ids[i];
                }
            }

            return _ids[^1];
        }

        public static List<IReadOnlyList<Vector2Int>> BuildPieces(int[] triple)
        {
            List<IReadOnlyList<Vector2Int>> pieces = new(triple.Length);
            foreach (int id in triple)
            {
                pieces.Add(BlockBlastCatalog.GetOffsets(id));
            }

            return pieces;
        }

        public static bool SameMultiset(int[] a, int[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            List<int> remainder = new(b);
            foreach (int id in a)
            {
                if (!remainder.Remove(id))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
