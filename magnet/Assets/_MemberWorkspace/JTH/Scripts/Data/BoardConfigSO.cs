using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Magnet/Board Config")]
    public sealed class BoardConfigSO : ScriptableObject
    {
        [field: SerializeField] public int CellCount { get; private set; } = 8;
        [field: SerializeField] public float CellSize { get; private set; } = 1f;
        [field: SerializeField] public float CellFill { get; private set; } = 0.9f;
    }
}
