using JTH.Scripts.Domain.Board;
using Magnet.Core.SO.Block;

namespace JTH.Scripts.Domain.Spawn
{
    public class BlockSpawnContext
    {
        public BlockShapeSourceSO BlockShapeSourceSO { get; private set; }
        public BoardGrid Grid { get; set; }
        public int Score { get; set; }

        public BlockSpawnContext(BoardGrid grid, int score)
        {
            Grid = grid;
            Score = score;
        }
    }
}