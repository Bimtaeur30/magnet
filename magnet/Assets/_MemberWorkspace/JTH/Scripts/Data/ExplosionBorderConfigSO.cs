using LitMotion;
using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "ExplosionBorderConfig", menuName = "Magnet/Explosion Border Config")]
    public sealed class ExplosionBorderConfigSO : ScriptableObject
    {
        [Tooltip("폭발 테두리 펄스 LitMotion 시간(초)")]
        [field: SerializeField] public float Duration { get; private set; } = 0.4f;
        [Tooltip("테두리 기준 크기 대비 최대 배율. 1이면 크기 변화 없음")]
        [field: SerializeField] public float PeakScale { get; private set; } = 1.15f;
        [Tooltip("펄스 크기 LitMotion 이징. t는 alpha와 동일, Ease만 다름")]
        [field: SerializeField] public Ease SizeEase { get; private set; } = Ease.OutQuad;
        [Tooltip("펄스 알파 LitMotion 이징. t는 크기와 동일, Ease만 다름")]
        [field: SerializeField] public Ease AlphaEase { get; private set; } = Ease.InQuad;
        [Tooltip("펄스 최대 알파(0~1). 기본색 알파에 곱함")]
        [field: SerializeField] public float MaxAlpha { get; private set; } = 1f;
        [Tooltip("폭발 테두리 LineRenderer 색")]
        [field: SerializeField] public Color Color { get; private set; } = new(0.95f, 0.75f, 0.2f, 1f);
        [Tooltip("폭발 테두리 LineRenderer 두께")]
        [field: SerializeField] public float LineWidth { get; private set; } = 0.06f;
        [Tooltip("폭발 테두리 LineRenderer sortingOrder")]
        [field: SerializeField] public int SortingOrder { get; private set; } = 2;

        private void OnValidate()
        {
            Duration = Mathf.Max(0f, Duration);
            PeakScale = Mathf.Max(1f, PeakScale);
            MaxAlpha = Mathf.Clamp01(MaxAlpha);
            LineWidth = Mathf.Max(0.001f, LineWidth);
        }
    }
}
