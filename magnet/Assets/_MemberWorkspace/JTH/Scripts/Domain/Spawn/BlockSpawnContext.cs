using Magnet.Contracts;

namespace JTH.Scripts.Domain.Spawn
{
    public class BlockSpawnContext
    {
        public IBlockShape_[] Shapes { get; set; }

        public BlockSpawnContext Init(IBlockShape_[] shapes)
        {
            Shapes = shapes;

            return this;
        }
    }
}