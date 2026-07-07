using System.Collections.Generic;
using Magnet.Contracts.BlockShapes;
using UnityEngine;

namespace PTY.Scripts.Data
{
    [CreateAssetMenu(fileName = "BlockShape", menuName = "Magnet/Block Shape")]
    public sealed class BlockShapeSO : ScriptableObject, IBlockShape
    {
        [SerializeField] private string shapeId;
        [SerializeField] private List<Vector2Int> cellOffsets = new();

        public string ShapeId => shapeId;

        public IReadOnlyList<Vector2Int> CellOffsets => cellOffsets;
    }
}
