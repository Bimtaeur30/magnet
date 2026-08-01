using System.Collections.Generic;
using JTH.Scripts.Domain.BlockSelection.Bundles;
using UnityEngine;

namespace JTH.Scripts.Data
{
    /// <summary>
    /// 번들 모음 (SPEC §15.2). 태그별 조회는 최초 1회 캐시.
    /// </summary>
    [CreateAssetMenu(fileName = "BlockBundlePool", menuName = "Magnet/Block Bundle Pool")]
    public sealed class BlockBundlePoolSO : ScriptableObject
    {
        [SerializeField, Tooltip("전체 번들 목록. 태그 무관하게 전부 등록")]
        private List<BlockBundleSO> allBundles = new();

        private Dictionary<BundleTag, List<BlockBundleSO>> _byTag;

        public IReadOnlyList<BlockBundleSO> AllBundles => allBundles;

        public IReadOnlyList<BlockBundleSO> GetByTag(BundleTag tag)
        {
            if (_byTag == null)
            {
                BuildCache();
            }

            return _byTag.TryGetValue(tag, out List<BlockBundleSO> list) ? list : System.Array.Empty<BlockBundleSO>();
        }

        private void BuildCache()
        {
            _byTag = new Dictionary<BundleTag, List<BlockBundleSO>>();
            foreach (BlockBundleSO bundle in allBundles)
            {
                if (bundle == null)
                {
                    continue;
                }

                if (!_byTag.TryGetValue(bundle.Tag, out List<BlockBundleSO> list))
                {
                    list = new List<BlockBundleSO>();
                    _byTag.Add(bundle.Tag, list);
                }

                list.Add(bundle);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _byTag = null;
        }
#endif
    }
}
