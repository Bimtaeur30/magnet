using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Health;
using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Generation
{
    /// <summary>
    /// 보드만 보고 "접대할 가치가 있는 기회"를 0~1로 점수화 (SPEC §10.2).
    /// fillRate·deadZone·bigSlots는 BoardHealthResult를 재사용하고, near-line만 직접 센다.
    /// </summary>
    public static class OpportunityScorer
    {
        public static float Score(BoardGrid board, BoardHealthResult health, BlockSelectionTuningSO tuning)
        {
            float score = 0f;

            int nearLines = CountNearCompleteLines(board);
            score += nearLines * tuning.OpportunityNearLineWeight;

            if (nearLines >= 2)
            {
                score += tuning.OpportunityMultiLineBonus;
            }

            if (health.FillRate > 0f
                && health.FillRate < tuning.OpportunityAllClearFillMax
                && health.DeadZoneCount == 0)
            {
                score += tuning.OpportunityAllClearWeight;
            }

            score += Mathf.Clamp01(health.BigPieceSlots / (float)tuning.BigSlotNormalizeMax)
                * tuning.OpportunityBigSlotWeight;

            score -= health.DeadZoneCount * tuning.OpportunityDeadZonePenalty;

            return Mathf.Clamp01(score);
        }

        /// <summary>
        /// 빈 칸이 정확히 1개인 행·열 수 (한 칸 부족 = near-complete line).
        /// </summary>
        private static int CountNearCompleteLines(BoardGrid board)
        {
            int size = board.BoardSize;
            int nearLines = 0;
            Vector2Int cell = Vector2Int.zero;

            for (int x = 0; x < size; ++x)
            {
                int emptyInColumn = 0;
                for (int y = 0; y < size; ++y)
                {
                    cell.x = x;
                    cell.y = y;

                    if (!board.IsOccupied(cell))
                    {
                        ++emptyInColumn;
                    }
                }

                if (emptyInColumn == 1)
                {
                    ++nearLines;
                }
            }

            for (int y = 0; y < size; ++y)
            {
                int emptyInRow = 0;
                for (int x = 0; x < size; ++x)
                {
                    cell.x = x;
                    cell.y = y;

                    if (!board.IsOccupied(cell))
                    {
                        ++emptyInRow;
                    }
                }

                if (emptyInRow == 1)
                {
                    ++nearLines;
                }
            }

            return nearLines;
        }
    }
}
