using System.Collections.Generic;
using UnityEngine;

namespace JTH.Scripts.Domain.AreaBundleSpawn
{
    /// <summary>패 선택 시 시뮬로 직접 넣은 한 수. 기즈모용.</summary>
    public readonly struct AreaBundleExplainStep
    {
        public AreaBundleExplainStep(int pieceSlotIndex, Vector2Int pivot, IReadOnlyList<Vector2Int> cells)
        {
            PieceSlotIndex = pieceSlotIndex;
            Pivot = pivot;
            Cells = cells;
        }

        public int PieceSlotIndex { get; }
        public Vector2Int Pivot { get; }
        public IReadOnlyList<Vector2Int> Cells { get; }
    }
}
