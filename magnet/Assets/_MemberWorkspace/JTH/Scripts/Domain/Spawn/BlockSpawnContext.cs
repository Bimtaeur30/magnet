using JTH.Scripts.Domain.BlockSelection.Health;
using JTH.Scripts.Domain.Board;
using Magnet.Core.SO.Block;

namespace JTH.Scripts.Domain.Spawn
{
    public class BlockSpawnContext
    {
        public BlockShapeSourceSO BlockShapeSourceSO { get; private set; }
        public BoardGrid Grid { get; set; }
        public int Score { get; set; }

        // 블록 선택 알고리즘 입력 (SPEC §16.2). Score는 알고리즘에서 읽지 않는다.
        public BoardHealthResult Health { get; set; }
        public float BlameTotal { get; set; }
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