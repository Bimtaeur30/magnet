using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    public sealed class PlacedBlock
    {
        public PlacedBlock(int blockId, string shapeId, Vector2Int pivot, IReadOnlyList<Vector2Int> cellOffsets)
        {
            BlockId = blockId;
            ShapeId = shapeId;
            Pivot = pivot;
            CellOffsets = cellOffsets;
        }

        public int BlockId { get; }
        public string ShapeId { get; }
        public Vector2Int Pivot { get; }
        public IReadOnlyList<Vector2Int> CellOffsets { get; }
    }
}
