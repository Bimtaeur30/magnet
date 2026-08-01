using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Bundles
{
    /// <summary>
    /// 실시간 생성(Hospitality/Pressure/Fallback)용 추첨 항목. canonical offsets + 티어별 가중치 1개.
    /// </summary>
    public readonly struct WeightedShape
    {
        public IReadOnlyList<Vector2Int> CellOffsets { get; }
        public float Weight { get; }

        public WeightedShape(IReadOnlyList<Vector2Int> cellOffsets, float weight)
        {
            CellOffsets = cellOffsets;
            Weight = weight;
        }
    }
}
