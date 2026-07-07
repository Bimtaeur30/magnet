using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Magnet/Board Config")]
    public sealed class BoardConfigSO : ScriptableObject
    {
        [SerializeField] private int cellsPerSide = 4;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Color cellColor = new(0.2f, 0.22f, 0.28f, 1f);
        [SerializeField] private Color magnetAxisColor = new(0.95f, 0.75f, 0.2f, 1f);

        /// <summary>자석(0,0)에서 한쪽 끝까지 칸 수. 전체 한 변 = CellsPerSide * 2 + 1.</summary>
        public int CellsPerSide => cellsPerSide;

        public int BoardSize => cellsPerSide * 2 + 1;
        public float CellSize => cellSize;
        public Color CellColor => cellColor;
        public Color MagnetAxisColor => magnetAxisColor;

        private void OnValidate()
        {
            cellsPerSide = Mathf.Max(1, cellsPerSide);
            cellSize = Mathf.Max(0.1f, cellSize);
        }
    }
}
