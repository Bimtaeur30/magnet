using UnityEngine;

namespace JTH.Scripts.Domain
{
    /// <summary>
    /// 자석=(0,0) 격자 좌표 ↔ 월드. 유효 보드: [-N/2 .. N/2].
    /// </summary>
    public static class BoardCoordinates
    {
        public static int HalfExtent(int boardSize) => boardSize / 2;

        public static Vector2 GridToWorld(int gridX, int gridY, float cellSize)
        {
            return new Vector2(gridX * cellSize, gridY * cellSize);
        }

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
