using System.Collections.Generic;
using JTH.Scripts.Domain.Rotation;
using Magnet.Contracts.BlockShapes;
using UnityEngine;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>
    /// 원본 형태 SO를 바꾸지 않고, 시계방향 90°×N 회전된 CellOffsets를 노출한다.
    /// </summary>
    public sealed class RotatedBlockShape : IBlockShape
    {
        private readonly Vector2Int[] _cellOffsets;

        /// <param name="clockwiseQuarterTurns">0~3. 그 외는 mod 4로 정규화.</param>
        public RotatedBlockShape(IBlockShape source, int clockwiseQuarterTurns)
        {
            ShapeId = source.ShapeId;
            Icon = source.Icon;

            int turns = ((clockwiseQuarterTurns % 4) + 4) % 4;
            IReadOnlyList<Vector2Int> sourceOffsets = source.CellOffsets;
            _cellOffsets = new Vector2Int[sourceOffsets.Count];
            for (int i = 0; i < sourceOffsets.Count; i++)
            {
                Vector2Int cell = sourceOffsets[i];
                for (int t = 0; t < turns; t++)
                {
                    cell = GridRotation.RotateClockwise90(cell);
                }

                _cellOffsets[i] = cell;
            }
        }

        public string ShapeId { get; }

        public IReadOnlyList<Vector2Int> CellOffsets => _cellOffsets;

        public Texture2D Icon { get; }
    }
}
