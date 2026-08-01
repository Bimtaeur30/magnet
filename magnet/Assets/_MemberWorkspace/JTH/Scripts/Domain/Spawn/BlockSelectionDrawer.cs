using System.Collections.Generic;
using JTH.Scripts.Domain.BlockSelection;
using UnityEngine;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>
    /// 티어 스택 기반 Drawer (SPEC §16.2). RandomDrawer를 대체한다.
    /// 진단 데이터(티어·유일해 등)는 LastResult로 노출 — 이벤트 발행은 Bootstrap 몫.
    /// </summary>
    public sealed class BlockSelectionDrawer : AbstractDrawer
    {
        private readonly BlockSelectionOrchestrator _orchestrator;

        public BlockSelectionResult LastResult { get; private set; }

        public BlockSelectionDrawer(BlockSelectionOrchestrator orchestrator)
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
