using System.Collections.Generic;
using JTH.Scripts.Domain.Placement;

namespace JTH.Scripts.Domain
{
    public sealed class BoardSession
    {
        private readonly BoardGrid _grid;
        private readonly List<PlacedBlock> _placedBlocks = new();
        private int _nextBlockId = 1;

        public BoardSession(int boardSize)
        {
            _grid = new BoardGrid(boardSize);
        }

        public BoardGrid Grid => _grid;
        public IReadOnlyList<PlacedBlock> PlacedBlocks => _placedBlocks;

        public int AllocateBlockId()
        {
            return _nextBlockId++;
        }

        public void RegisterPlacedBlock(PlacedBlock placedBlock)
        {
            _placedBlocks.Add(placedBlock);
        }
    }
}
