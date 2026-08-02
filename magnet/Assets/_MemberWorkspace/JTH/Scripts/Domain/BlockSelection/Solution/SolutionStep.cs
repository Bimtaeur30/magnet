using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Solution
{
    /// <summary>
    /// 유일해의 배치 1스텝 (SPEC §11.5). 어느 슬롯 피스를 어디에 놓는지 + 그때 지워지는 라인 수.
    /// </summary>
    public readonly struct SolutionStep
    {
        public int SlotIndex { get; }
        public Vector2Int Pivot { get; }

        /// <summary>회전 적용 후 offsets (피스 원본과 동일 인스턴스).</summary>
        public IReadOnlyList<Vector2Int> CellOffsets { get; }

        /// <summary>이 스텝 배치 직후 지워지는 라인 수 (난이도 판정 입력).</summary>
        public int ClearedLines { get; }

        public SolutionStep(int slotIndex, Vector2Int pivot, IReadOnlyList<Vector2Int> cellOffsets, int clearedLines)
        {
            SlotIndex = slotIndex;
            Pivot = pivot;
            CellOffsets = cellOffsets;
            ClearedLines = clearedLines;
        }
    }
}
