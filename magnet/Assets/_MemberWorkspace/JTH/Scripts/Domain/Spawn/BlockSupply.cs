using System.Collections.Generic;
using Magnet.Contracts.BlockShapes;

namespace JTH.Scripts.Domain.Spawn
{
    /// <summary>
    /// 하단 3슬롯 블록 후보 상태. 추첨·이벤트는 담당하지 않는다.
    /// </summary>
    public sealed class BlockSupply
    {
        public const int SlotCount = 3;

        private readonly BlockDrawer _drawer;
        private readonly IBlockShape[] _slots = new IBlockShape[SlotCount];

        public BlockSupply(BlockDrawer drawer)
        {
            _drawer = drawer;
        }

        public IReadOnlyList<IBlockShape> Candidates => _slots;

        public IBlockShape GetCandidate(int slotIndex) => _slots[slotIndex];

        public void Fill()
        {
            for (var i = 0; i < SlotCount; i++)
            {
                _slots[i] = _drawer.Draw();
            }
        }

        public void Consume(int slotIndex)
        {
            _slots[slotIndex] = _drawer.Draw();
        }

        public IBlockShape[] CreateSnapshot()
        {
            var copy = new IBlockShape[SlotCount];
            for (var i = 0; i < SlotCount; i++)
            {
                copy[i] = _slots[i];
            }

            return copy;
        }
    }
}
