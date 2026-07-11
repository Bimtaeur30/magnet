using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    public sealed class PlacedBlock
    {
        private readonly List<Vector2Int> _cellOffsets;

        public PlacedBlock(int blockId, string shapeId, Vector2Int pivot, IReadOnlyList<Vector2Int> cellOffsets)
        {
            BlockId = blockId;
            ShapeId = shapeId;
            Pivot = pivot;
            _cellOffsets = new List<Vector2Int>(cellOffsets);
        }

        public int BlockId { get; }
        public string ShapeId { get; }
        public Vector2Int Pivot { get; private set; }
        public IReadOnlyList<Vector2Int> CellOffsets => _cellOffsets;

        public IEnumerable<Vector2Int> AbsoluteCells()
        {
            for (int i = 0; i < _cellOffsets.Count; i++)
            {
                yield return Pivot + _cellOffsets[i];
            }
        }

        public void SetPlacement(Vector2Int pivot, IReadOnlyList<Vector2Int> cellOffsets)
        {
            Pivot = pivot;
            _cellOffsets.Clear();
            _cellOffsets.AddRange(cellOffsets);
        }
    }
}
