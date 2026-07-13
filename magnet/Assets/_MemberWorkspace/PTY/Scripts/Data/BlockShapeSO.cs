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
        [SerializeField] private Texture2D icon;

        public string ShapeId => shapeId;

        public IReadOnlyList<Vector2Int> CellOffsets => cellOffsets;

        public Texture2D Icon => icon;
    }
}
