using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Simulation
{
    public static class ShapeRotator
    {
        /// <summary>
        /// 시계 방향으로 rotationCount만큼 회전시키는 함수
        /// </summary>
        public static IReadOnlyList<Vector2Int> Rotate(IReadOnlyList<Vector2Int> canonicalOffsets, int rotationCount)
        {
            int steps = rotationCount % 4;
            if (steps == 0)
            {
                return canonicalOffsets;
            }

            //시계 방향 회전
            List<Vector2Int> rotated = new(canonicalOffsets.Count);
            foreach (Vector2Int offset in canonicalOffsets)
            {
                Vector2Int cell = offset;
                for (int i = 0; i < steps; ++i)
                {
                    cell = new Vector2Int(cell.y, -cell.x);
                }
                rotated.Add(cell);
            }

            Normalize(rotated);
            return rotated;
        }

        /// <summary>
        /// 오프셋의 가장 작은 값을 0,0으로 바꿔서 1사분면 구석에 박는(피봇 맞추기용) 함수
        /// </summary>
        private static void Normalize(List<Vector2Int> offsets)
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            foreach (Vector2Int offset in offsets)
            {
                minX = Mathf.Min(minX, offset.x);
                minY = Mathf.Min(minY, offset.y);
            }

            for (int i = 0; i < offsets.Count; ++i)
            {
                offsets[i] -= new Vector2Int(minX, minY);
            }
        }
    }
}
