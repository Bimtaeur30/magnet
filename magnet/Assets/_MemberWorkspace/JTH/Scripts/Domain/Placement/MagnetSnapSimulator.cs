using Magnet.Contracts.BlockShapes;
using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    /// <summary>
    /// 자석 흡착으로 "최종 정착 pivot"을 계산한다. (연출은 Presentation 책임)
    ///
    /// 경로상 **블록 또는 자석**에 먼저 닿으면 직전 pivot에 스냅(성공).
    /// **경계만** 만나고 블록·자석을 못 만나면 배치 불가(<see cref="PlacementFailureReason.NoSnapTarget"/>).
    /// </summary>
    public sealed class MagnetSnapSimulator
    {
        public bool TrySnap(IBlockShape shape, Vector2Int startPivot, BoardGrid grid, out Vector2Int finalPivot)
        {
            int stepY = GetStepTowardMagnetRow(startPivot.y);
            Vector2Int pivot = startPivot;
            finalPivot = startPivot;

            // 스테이징은 보드 밖(Y≠0)에서 시작한다고 가정. 이미 자석 행이면 흡착 경로가 없다.
            if (stepY == 0)
            {
                return false;
            }

            int half = BoardCoordinates.HalfExtent(grid.BoardSize);

            while (true)
            {
                StepBlock block = EvaluateNext(shape, pivot, stepY, half, grid);
                if (block == StepBlock.Contact)
                {
                    finalPivot = pivot;
                    return true;
                }

                if (block == StepBlock.Boundary)
                {
                    finalPivot = pivot;
                    return false;
                }

                pivot = new Vector2Int(pivot.x, pivot.y + stepY);
            }
        }

        private enum StepBlock
        {
            None,
            Contact,
            Boundary,
        }

        private static int GetStepTowardMagnetRow(int pivotY)
        {
            if (pivotY < 0)
            {
                return 1;
            }

            if (pivotY > 0)
            {
                return -1;
            }

            return 0;
        }

        private static StepBlock EvaluateNext(IBlockShape shape, Vector2Int currentPivot, int stepY, int half, BoardGrid grid)
        {
            Vector2Int nextPivot = new(currentPivot.x, currentPivot.y + stepY);

            foreach (Vector2Int offset in shape.CellOffsets)
            {
                Vector2Int cell = nextPivot + offset;

                if (stepY > 0 && cell.y > half)
                {
                    return StepBlock.Boundary;
                }

                if (stepY < 0 && cell.y < -half)
                {
                    return StepBlock.Boundary;
                }
            }

            if (BlockPlacementCells.HasOverlap(shape, nextPivot, grid))
            {
                return StepBlock.Contact;
            }

            return StepBlock.None;
        }
    }
}
