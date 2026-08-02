using System.Collections.Generic;
using JTH.Scripts.Domain.HybridSpawn;
using UnityEngine;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>
    /// 병합 알고리즘 Drawer — BlockBlastDrawer(순수 핸드오프)·BlockSelectionDrawer(구 티어 스택)를 대체한다.
    /// 진단 데이터(티어·유일해·알고리즘 ID)는 LastResult로 노출 — 로그·이벤트는 Bootstrap 몫.
    /// </summary>
    public sealed class HybridDrawer : AbstractDrawer
    {
        private readonly HybridSpawnOrchestrator _orchestrator;

        public HybridSelectionResult LastResult { get; private set; }

        public HybridDrawer(HybridSpawnOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        public override List<IReadOnlyList<Vector2Int>> Draw(BlockSpawnContext context, int drawCount)
        {
            LastResult = _orchestrator.SelectPieces(
                context.Grid,
                context.Health,
                context.BlameTotal,
                context.IsRetrySession,
                context.TurnIndex);

            return LastResult.Pieces;
        }
    }
}
