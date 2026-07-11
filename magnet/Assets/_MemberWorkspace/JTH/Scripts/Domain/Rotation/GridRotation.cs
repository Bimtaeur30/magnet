using UnityEngine;

namespace JTH.Scripts.Domain.Rotation
{
    public static class GridRotation
    {
        /// <summary>자석 (0,0) 기준 시계방향 90°.</summary>
        public static Vector2Int RotateClockwise90(Vector2Int grid)
        {
            return new Vector2Int(grid.y, -grid.x);
        }
    }
}
