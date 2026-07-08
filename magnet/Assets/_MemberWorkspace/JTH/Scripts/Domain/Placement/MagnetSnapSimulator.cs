using Magnet.Contracts.BlockShapes;
using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    /// <summary>
    /// 자석 흡착 규칙으로 "최종 정착 pivot"을 계산한다. (연출/애니메이션은 Presentation 책임)
    /// 내부적으로는 격자 규칙 검사를 위해 한 칸 단위로 진행하지만, 이는 "이동 연출"이 아니라 "정착 좌표 계산"이다.
    ///
    /// Y축으로 자석 행(y=0)을 향해 진행하며, 점유·자석·Y 경계에 닿으면 직전 pivot에서 정지한다.
    /// 스테이징(보드 아래)처럼 Y 경계 밖에서 시작하는 경우, 진입 중인 칸은 허용한다.
    /// </summary>
    public sealed class MagnetSnapSimulator
    {
        public Vector2Int Snap(IBlockShape shape, Vector2Int startPivot, BoardGrid grid)
        {
            int stepY = GetStepTowardMagnetRow(startPivot.y);
            Vector2Int pivot = startPivot;

            if (stepY == 0)
            {
                return pivot;
            }

            int half = BoardCoordinates.HalfExtent(grid.BoardSize);

            while (CanStep(shape, pivot, stepY, half, grid))
            {
                pivot = new Vector2Int(pivot.x, pivot.y + stepY);
            }

            return pivot;
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

        private static bool CanStep(IBlockShape shape, Vector2Int currentPivot, int stepY, int half, BoardGrid grid)
        {
            Vector2Int nextPivot = new(currentPivot.x, currentPivot.y + stepY);

            foreach (Vector2Int offset in shape.CellOffsets)
            {
                Vector2Int cell = nextPivot + offset;

                if (BoardCoordinates.IsMagnetCell(cell.x, cell.y))
                {
                    return false;
                }

                if (grid.IsOccupied(cell))
                {
                    return false;
                }

                if (stepY > 0 && cell.y > half)
                {
                    return false;
                }

                if (stepY < 0 && cell.y < -half)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
