using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "BlockedRingDimConfig", menuName = "Magnet/Blocked Ring Dim Config")]
    public sealed class BlockedRingDimConfigSO : ScriptableObject
    {
        [Tooltip("비활성(막힌) 테두리 링 점유 칸 RGB 배수. 1=변화 없음")]
        [SerializeField, Range(0.05f, 1f)] private float dimMultiply = 0.35f;

        public float DimMultiply => dimMultiply;

        private void OnValidate()
        {
            dimMultiply = Mathf.Clamp(dimMultiply, 0.05f, 1f);
        }
    }
}
