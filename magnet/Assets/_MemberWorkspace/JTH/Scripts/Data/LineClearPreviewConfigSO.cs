using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "LineClearPreviewConfig", menuName = "Magnet/Line Clear Preview Config")]
    public sealed class LineClearPreviewConfigSO : ScriptableObject
    {
        [Tooltip("클리어될 칸(Place·프리뷰) 알파 숨쉬기 최소")]
        [field: SerializeField] public float PulseMinAlpha { get; private set; } = 0.45f;

        [Tooltip("프리뷰 칸 알파 숨쉬기 최대 (Place된 칸은 최대 1 고정)")]
        [field: SerializeField] public float PulseMaxAlpha { get; private set; } = 1f;

        [Tooltip("숨쉬기 한 주기(초)")]
        [field: SerializeField] public float PulsePeriod { get; private set; } = 1.2f;
    }
}
