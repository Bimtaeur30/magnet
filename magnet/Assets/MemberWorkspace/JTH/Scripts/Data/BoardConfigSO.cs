using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Magnet/Board Config")]
    public sealed class BoardConfigSO : ScriptableObject
    {
        [SerializeField] private int _boardSize = 9;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private Color _cellColor = new(0.2f, 0.22f, 0.28f, 1f);
        [SerializeField] private Color _magnetAxisColor = new(0.95f, 0.75f, 0.2f, 1f);

        public int BoardSize => _boardSize;
        public int HalfExtent => _boardSize / 2;
        public float CellSize => _cellSize;
        public Color CellColor => _cellColor;
        public Color MagnetAxisColor => _magnetAxisColor;

        private void OnValidate()
        {
            _boardSize = Mathf.Max(3, _boardSize);
            if (_boardSize % 2 == 0)
            {
                _boardSize += 1;
            }

            _cellSize = Mathf.Max(0.1f, _cellSize);
        }
    }
}
