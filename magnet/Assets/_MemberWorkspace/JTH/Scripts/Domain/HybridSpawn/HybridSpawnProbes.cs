using System.Collections.Generic;
using JTH.Scripts.Domain.BlockBlast;
using UnityEngine;

namespace JTH.Scripts.Domain.HybridSpawn
{
    /// <summary>
    /// BoardHealth의 placementFreedom 프로브 피스 — 42-ID 중 회전 중복이 없는 대표 canonical 13종.
    /// (BoardHealthCalculator가 내부에서 4회전을 적용하므로 회전형 ID를 넣으면 중복 계산이다.)
    /// </summary>
    public static class HybridSpawnProbes
    {
        private static readonly int[] ProbeIds =
        {
            5,   // 가로 3
            17,  // 가로 4
            11,  // 가로 5
            9,   // 2x2
            35,  // 3x2
            13,  // 3x3
            8,   // L4
            30,  // J4
            10,  // T4
            14,  // S4
            18,  // Z4
            6,   // 3칸 ㄱ
            37,  // 대각 2
        };

        public static readonly IReadOnlyList<IReadOnlyList<Vector2Int>> FreedomProbePieces = Build();

        private static IReadOnlyList<IReadOnlyList<Vector2Int>> Build()
        {
            List<IReadOnlyList<Vector2Int>> pieces = new(ProbeIds.Length);
            foreach (int id in ProbeIds)
            {
                pieces.Add(BlockBlastCatalog.GetOffsets(id));
            }

            return pieces;
        }
    }
}
