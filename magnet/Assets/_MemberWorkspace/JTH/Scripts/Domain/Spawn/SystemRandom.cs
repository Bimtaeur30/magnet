using System;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>
    /// System.Random 기반 IRandom. 시드를 주면 같은 순서를 재현한다.
    /// </summary>
    public sealed class SystemRandom : IRandom
    {
        private readonly Random _random;

        public SystemRandom()
        {
            _random = new Random();
        }

        public SystemRandom(int seed)
        {
            _random = new Random(seed);
        }

        public int Next(int maxExclusive) => _random.Next(maxExclusive);
    }
}
