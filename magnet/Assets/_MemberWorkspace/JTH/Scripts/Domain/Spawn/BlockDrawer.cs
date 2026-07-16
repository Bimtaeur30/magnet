using Magnet.Contracts.BlockShapes;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>
    /// 형태 소스에서 IRandom으로 블록 형태 1개를 뽑고, 0/90/180/270 중 균등 회전을 적용한다.
    /// </summary>
    public sealed class BlockDrawer
    {
        private readonly IBlockShapeSource _source;
        private readonly IRandom _random;

        public BlockDrawer(IBlockShapeSource source, IRandom random)
        {
            _source = source;
            _random = random;
        }

        public IBlockShape Draw()
        {
            var shapes = _source.Shapes;
            IBlockShape raw = shapes[_random.Next(shapes.Count)];
            int quarterTurns = _random.Next(4);
            return quarterTurns == 0
                ? raw
                : new RotatedBlockShape(raw, quarterTurns);
        }
    }
}
