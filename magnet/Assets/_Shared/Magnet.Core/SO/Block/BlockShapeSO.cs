using System.Collections.Generic;
using UnityEngine;

namespace Magnet.Core.SO.Block
{
    [CreateAssetMenu(fileName = "BlockShape", menuName = "Magnet/Block Shape")]
    public sealed class BlockShapeSO : ScriptableObject
    {
        [SerializeField] private string shapeId;
        [SerializeField] private List<Vector2Int> cellOffsets = new();
        [SerializeField] private Texture2D icon;

        public string ShapeId => shapeId;

        public IReadOnlyList<Vector2Int> CellOffsets => cellOffsets;

        public Texture2D Icon => icon;

        // 런타임 스킨 변경 시 아이콘을 즉석 캡처로 갱신할 때만 사용 (RuntimeBlockIconGenerator).
        public void SetIcon(Texture2D newIcon)
        {
            icon = newIcon;
        }
    }
}
