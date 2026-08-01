using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Solution
{
    /// <summary>
    /// Pressure 턴의 유일한 full sequence (SPEC §11.5). 배치별 정답 매칭(엄지척 UI 데이터)에 쓴다.
    /// Pressure가 아닌 턴에는 존재하지 않는다 (null).
    /// </summary>
    public sealed class UniqueSolution
    {
        public IReadOnlyList<SolutionStep> Steps { get; }

        public UniqueSolution(IReadOnlyList<SolutionStep> steps)
        {
            Steps = steps;
        }

        /// <summary>
        /// currentStepIndex번째 배치가 유일해와 일치하는지 판정.
        /// 일치하면 placedCells에 엄지척 UI를 띄울 보드 칸 목록을 담는다.
        /// 라인 클리어로 보드가 변하므로 스텝 "순서"까지 일치해야 정답이다.
        /// </summary>
        public bool MatchesStep(
            int currentStepIndex,
            int placedSlotIndex,
            Vector2Int placedPivot,
            out IReadOnlyList<Vector2Int> placedCells)
        {
            placedCells = null;

            if (currentStepIndex < 0 || currentStepIndex >= Steps.Count)
            {
                return false;
            }

            SolutionStep step = Steps[currentStepIndex];
            if (step.SlotIndex != placedSlotIndex || step.Pivot != placedPivot)
            {
                return false;
            }

            List<Vector2Int> cells = new(step.CellOffsets.Count);
            foreach (Vector2Int offset in step.CellOffsets)
            {
                cells.Add(step.Pivot + offset);
            }

            placedCells = cells;
            return true;
        }
    }
}
