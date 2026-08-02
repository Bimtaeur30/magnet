using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "BlockVisualConfig", menuName = "Magnet/Block Visual Config")]
    public sealed class BlockVisualConfigSO : ScriptableObject
    {
        [Tooltip("드래그 중 보드 격자 프리뷰(고스트) 블록 알파(0~1). 스테이징에는 적용되지 않음")]
        [field: SerializeField] public float PreviewAlpha { get; private set; } = 0.4f;
    }
}
