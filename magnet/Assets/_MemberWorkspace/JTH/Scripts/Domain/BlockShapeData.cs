using System.Collections.Generic;
using Magnet.Contracts.BlockShapes;
using UnityEngine;

namespace JTH.Scripts.Domain
{
    /// <summary>
    /// 에디터 SO 대기용 임시 형태 데이터. 코드에서 값을 직접 넣어 쓴다.
    /// </summary>
    public sealed class BlockShapeData : IBlockShape
    {
        private readonly Vector2Int[] cellOffsets;

        public BlockShapeData(string shapeId, Vector2Int[] cellOffsets)
        {
            ShapeId = shapeId;
            this.cellOffsets = cellOffsets;
            BoundsSize = BlockShapeBounds.ComputeSize(cellOffsets);
        }

        public string ShapeId { get; }

        public IReadOnlyList<Vector2Int> CellOffsets => cellOffsets;

        public Vector2Int BoundsSize { get; }
    }
}
