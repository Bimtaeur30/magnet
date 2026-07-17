using LitMotion;
using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "BlockSnapConfig", menuName = "Magnet/Block Snap Config")]
    public sealed class BlockSnapConfigSO : ScriptableObject
    {
        [Tooltip("손 놓은 뒤 Y축 자석 스냅: 칸 1칸 이동에 걸리는 시간(초). 이동 칸 수에 비례")]
        [field: SerializeField] public float Duration { get; private set; } = 0.12f;
        [Tooltip("Place 성공 후 Y 스냅 LitMotion 이징")]
        [field: SerializeField] public Ease Ease { get; private set; } = Ease.OutQuad;

        private void OnValidate()
        {
            Duration = Mathf.Max(0.01f, Duration);
        }
    }
}
