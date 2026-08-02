using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>
    /// System.Random 기반 IRandom. 시드를 주면 같은 순서를 재현한다.
    /// </summary>
    public sealed class RandomDrawer : AbstractDrawer
    {
        private readonly Random _random;

        public RandomDrawer()
        {
            _random = new Random();
        }

        public RandomDrawer(int seed)
        {
            _random = new Random(seed);
        }

        public override List<IReadOnlyList<Vector2Int>> Draw(BlockSpawnContext context, int drawCount)
        {
            List<IReadOnlyList<Vector2Int>> drawn = new(drawCount);
            List<IReadOnlyList<Vector2Int>> remaining = context.BlockShapeSourceSO.Shapes
                .Select(so => so.CellOffsets)
                .ToList();
            
            for (int i = 0; i < drawCount; ++i)
            {
                int index = _random.Next(remaining.Count);
                drawn.Add(remaining[index]);
                remaining.RemoveAt(index);
            }
            
            return drawn;
        }
    }
}
