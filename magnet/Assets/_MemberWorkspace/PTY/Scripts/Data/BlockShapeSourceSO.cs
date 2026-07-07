using System.Collections.Generic;
using Magnet.Contracts.BlockShapes;
using UnityEngine;

namespace PTY.Scripts.Data
{
    /// <summary>
    /// 추첨 풀에 포함할 BlockShapeSO 목록. IBlockShapeSource 계약 구현.
    /// Installer가 RegisterValue로 등록하고, JTH는 [Inject] IBlockShapeSource로 소비한다.
    /// </summary>
    [CreateAssetMenu(fileName = "BlockShapeSource", menuName = "Magnet/Block Shape Source")]
    public sealed class BlockShapeSourceSO : ScriptableObject, IBlockShapeSource
    {
        [SerializeField] private List<BlockShapeSO> shapes = new();

        private IReadOnlyList<IBlockShape> _shapesReadOnly;

        private void OnEnable()
        {
            RebuildReadOnlyShapes();
        }

        public IReadOnlyList<IBlockShape> Shapes => _shapesReadOnly;

#if UNITY_EDITOR
        private void OnValidate()
        {
            RebuildReadOnlyShapes();
        }
#endif

        private void RebuildReadOnlyShapes()
        {
            var list = new List<IBlockShape>(shapes.Count);
            foreach (var shape in shapes)
            {
                if (shape != null)
                {
                    list.Add(shape);
                }
            }

            _shapesReadOnly = list;
        }
    }
}
