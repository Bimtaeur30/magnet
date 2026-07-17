using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "ClearReassemblyRuleConfig", menuName = "Magnet/Clear Reassembly Rule Config")]
    public sealed class ClearReassemblyRuleConfigSO : ScriptableObject
    {
        [Tooltip("원점–원래칸 직선에 대한 수선 반폭(격자 단위). 이 복도 안 칸만 후보. 넓히면 대각선도 안정적으로 굴러감. 막히면 제자리")]
        [field: SerializeField] public float CorridorHalfWidth { get; private set; } = 0.75f;

        private void OnValidate()
        {
            CorridorHalfWidth = Mathf.Clamp(CorridorHalfWidth, 0.01f, 3f);
        }
    }
}
