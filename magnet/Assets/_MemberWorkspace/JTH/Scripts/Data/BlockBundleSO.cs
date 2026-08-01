using System.Collections.Generic;
using JTH.Scripts.Domain.BlockSelection.Bundles;
using Magnet.Core.SO.Block;
using UnityEngine;

namespace JTH.Scripts.Data
{
    /// <summary>
    /// 피스 3개의 고정 조합 (SPEC §15.1). 슬롯 0,1,2 순서 대응. 회전은 Draw 시점에 랜덤 적용.
    /// </summary>
    [CreateAssetMenu(fileName = "BlockBundle", menuName = "Magnet/Block Bundle")]
    public sealed class BlockBundleSO : ScriptableObject
    {
        [SerializeField, Tooltip("로그·디버그용 번들 식별자 (예: normal_big)")]
        private string bundleId;

        [SerializeField, Tooltip("번들 용도 태그. 티어 스택이 태그별로 후보를 거른다")]
        private BundleTag tag;

        [SerializeField, Tooltip("슬롯 0,1,2에 대응하는 블록 모양 3개. 1x1은 Relife 태그에서만 허용")]
        private List<BlockShapeSO> shapes = new(3);

        [SerializeField, Tooltip("같은 태그 안에서의 가중 랜덤 추첨 가중치 (Trap/ComboBreak는 1이어도 됨)")]
        private int weight = 1;

        public string BundleId => bundleId;
        public BundleTag Tag => tag;
        public IReadOnlyList<BlockShapeSO> Shapes => shapes;
        public int Weight => weight;
    }
}
