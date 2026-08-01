using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Tiers
{
    /// <summary>
    /// 번들 추첨 확정 결과. 회전이 적용된 피스 3개 + 로그용 번들 id.
    /// </summary>
    public sealed class BundleDraw
    {
        public string BundleId { get; }
        public List<IReadOnlyList<Vector2Int>> Pieces { get; }

        public BundleDraw(string bundleId, List<IReadOnlyList<Vector2Int>> pieces)
        {
            BundleId = bundleId;
            Pieces = pieces;
        }
    }
}
