using System.Collections.Generic;
using UnityEngine;

namespace Magnet.Contracts.BlockShapes
{
    /// <summary>
    /// CellOffsets → BoundsSize 계산. SO 구현체·멤버별 임시 데이터가 공통 사용.
    /// </summary>
    public static class BlockShapeBounds
    {
        public static Vector2Int ComputeSize(IReadOnlyList<Vector2Int> cellOffsets)
        {
            if (cellOffsets == null || cellOffsets.Count == 0)
            {
                return Vector2Int.zero;
            }

            int minX = cellOffsets[0].x;
            int maxX = cellOffsets[0].x;
            int minY = cellOffsets[0].y;
            int maxY = cellOffsets[0].y;

            for (int i = 1; i < cellOffsets.Count; i++)
            {
                Vector2Int cell = cellOffsets[i];
                if (cell.x < minX)
                {
                    minX = cell.x;
                }

                if (cell.x > maxX)
                {
                    maxX = cell.x;
                }

                if (cell.y < minY)
                {
                    minY = cell.y;
                }

                if (cell.y > maxY)
                {
                    maxY = cell.y;
                }
            }

            return new Vector2Int(maxX - minX + 1, maxY - minY + 1);
        }
    }
}
