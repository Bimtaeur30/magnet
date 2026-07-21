using System.Collections.Generic;
using UnityEngine;

namespace _Shared.Magnet.Core.SO.Block
{
    /// <summary>
    /// 추첨 풀에 포함할 BlockShapeSO 목록. IBlockShapeSource 계약 구현.
    /// Installer가 RegisterValue로 등록하고, JTH는 [Inject] IBlockShapeSource로 소비한다.
    /// </summary>
    [CreateAssetMenu(fileName = "BlockShapeSource", menuName = "Magnet/Block Shape Source")]
    public sealed class BlockShapeSourceSO : ScriptableObject
    {
        [SerializeField] private List<BlockShapeSO> shapes = new();

        private void OnEnable()
        {
            RebuildReadOnlyShapes();
        }

        public IReadOnlyList<BlockShapeSO> Shapes => shapes;

#if UNITY_EDITOR
        private void OnValidate()
        {
            RebuildReadOnlyShapes();
        }
#endif

        private void RebuildReadOnlyShapes()
        {
            var list = new List<BlockShapeSO>(shapes.Count);
            foreach (var shape in shapes)
            {
                if (shape != null)
                {
                    list.Add(shape);
                }
            }

            shapes = list;
        }
    }
}
