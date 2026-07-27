using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.Spawn
{
    public abstract class AbstractDrawer
    {
        public abstract List<IReadOnlyList<Vector2Int>> Draw(BlockSpawnContext context, int drawCount);
    }
}
