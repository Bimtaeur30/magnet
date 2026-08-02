using System.Collections.Generic;
using UnityEngine;

namespace Magnet.Contracts
{
    public class ShapeBlockData
    {
        public IReadOnlyList<Vector2Int> CellOffsets { get; set; }
        public int SkinId { get; set; }
    }
}