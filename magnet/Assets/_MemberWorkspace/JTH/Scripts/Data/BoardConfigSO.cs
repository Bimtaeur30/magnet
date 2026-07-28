using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Magnet/Board Config")]
    public sealed class BoardConfigSO : ScriptableObject
    {
        [field: SerializeField] public int CellCount { get; private set; } = 8;
        [field: SerializeField] public float CellSize { get; private set; } = 1f;
        [field: SerializeField] public Color CellColor { get; private set; } = new(0.2f, 0.22f, 0.28f, 1f);

        private void OnValidate()
        {
            CellCount = Mathf.Max(1, CellCount);
            CellSize = Mathf.Max(0.1f, CellSize);
        }
    }
}
