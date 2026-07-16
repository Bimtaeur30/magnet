using UnityEngine;

namespace JTH.Scripts.Domain
{
    /// <summary>
    /// 자석=(0,0) 격자 좌표 ↔ 보드 로컬 좌표. 유효 보드: [-N/2 .. N/2].
    /// 월드 배치는 Board Transform이 담당한다. 반환값은 localPosition용.
    /// </summary>
    public static class BoardCoordinates
    {
        public static int HalfExtent(int boardSize) => boardSize / 2;

        /// <summary>격자 → 보드 로컬 (자석 기준). 이름은 World이나 의미는 board-local.</summary>
        public static Vector2 GridToWorld(int gridX, int gridY, float cellSize)
        {
            return new Vector2(gridX * cellSize, gridY * cellSize);
        }

        /// <summary>보드 로컬 → 격자. <paramref name="world"/>는 board-local 좌표.</summary>
        public static Vector2Int WorldToGrid(Vector2 world, float cellSize)
        {
            return new Vector2Int(
                Mathf.RoundToInt(world.x / cellSize),
                Mathf.RoundToInt(world.y / cellSize));
        }

        public static bool IsInBounds(int gridX, int gridY, int boardSize)
        {
            int half = HalfExtent(boardSize);
            return gridX >= -half && gridX <= half && gridY >= -half && gridY <= half;
        }

        public static bool IsMagnetCell(int gridX, int gridY)
        {
            return gridX == 0 && gridY == 0;
        }
    }
}
