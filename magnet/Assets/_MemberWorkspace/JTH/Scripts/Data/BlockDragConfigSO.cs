using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "BlockDragConfig", menuName = "Magnet/Block Drag Config")]
    public sealed class BlockDragConfigSO : ScriptableObject
    {
        [Tooltip("Press 시작 포인터 X와의 거리(월드 유닛) 1당 블록 이동 배율 증가량. Block Blast식 감도 램프")]
        [field: SerializeField] public float SensitivityRampPerUnit { get; private set; } = 0.35f;
        [Tooltip("드래그 감도 배율 상한. 1이면 램프 없음")]
        [field: SerializeField] public float SensitivityMaxMultiplier { get; private set; } = 3f;

        private void OnValidate()
        {
            SensitivityRampPerUnit = Mathf.Max(0f, SensitivityRampPerUnit);
            SensitivityMaxMultiplier = Mathf.Max(1f, SensitivityMaxMultiplier);
        }
    }
}
