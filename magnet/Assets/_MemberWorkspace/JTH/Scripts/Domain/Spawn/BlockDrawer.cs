using Magnet.Contracts.BlockShapes;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>
    /// 형태 소스에서 IRandom으로 블록 형태 1개를 뽑는다.
    /// Phase 2: 균등 확률. 가중치는 Phase 4에서 Draw 내부만 교체한다 (소스·공급기 불변).
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
            return shapes[_random.Next(shapes.Count)];
        }
    }
}
