using System.Collections.Generic;
using JTH.Scripts.Domain.BlockBlast;
using UnityEngine;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>
    /// BlockBlast! 핸드오프 알고리즘 기반 Drawer. BlockSelectionDrawer(티어 스택)를 대체한다.
    /// 진단 데이터(알고리즘 ID 체인 등)는 LastSelection으로 노출 — 로그·이벤트는 Bootstrap 몫.
    /// </summary>
    public sealed class BlockBlastDrawer : AbstractDrawer
    {
        private readonly BlockBlastAlgorithm _algorithm;

        public BlockBlastSelection LastSelection { get; private set; }

        public BlockBlastDrawer(BlockBlastAlgorithm algorithm)
        {
            _algorithm = algorithm;
        }

        public override List<IReadOnlyList<Vector2Int>> Draw(BlockSpawnContext context, int drawCount)
        {
            LastSelection = _algorithm.Select(context.Grid);
            return LastSelection.Pieces;
        }
    }
}
