using System;
using Magnet.Contracts;

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

        public int Next(int maxExclusive) => _random.Next(maxExclusive);
        
        public override IBlockShape_ Next(BlockSpawnContext context)
        {
            return context.Shapes[_random.Next(context.Shapes.Length)];
        }
    }
}
