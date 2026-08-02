using JTH.Scripts.Domain.Board;
using Magnet.Core.SO.Block;

namespace JTH.Scripts.Domain.Spawn
{
    public class BlockSpawnContext
    {
        public BlockShapeSourceSO BlockShapeSourceSO { get; private set; }
        public BoardGrid Grid { get; set; }
        public int Score { get; set; }

        public bool IsRetrySession { get; set; }
        public int TurnIndex { get; set; }

        public BlockSpawnContext(BlockShapeSourceSO sourceSO, BoardGrid grid, int score)
        {
            BlockShapeSourceSO = sourceSO;
            Grid = grid;
            Score = score;
        }
    }
}
