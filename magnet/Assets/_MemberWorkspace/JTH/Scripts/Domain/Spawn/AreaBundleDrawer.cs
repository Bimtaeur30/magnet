using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>Area-번들 cascade Drawer.</summary>
    public sealed class AreaBundleDrawer : AbstractDrawer
    {
        private readonly AreaBundleSpawn.AreaBundleOrchestrator _orchestrator;

        public AreaBundleSpawn.AreaBundleSelectionResult LastResult { get; private set; }

        public AreaBundleDrawer(AreaBundleSpawn.AreaBundleOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        public override List<IReadOnlyList<Vector2Int>> Draw(BlockSpawnContext context, int drawCount)
        {
            LastResult = _orchestrator.Select(context.Grid, context.TurnIndex, context.IsRetrySession);
            return LastResult.Pieces;
        }
    }
}
