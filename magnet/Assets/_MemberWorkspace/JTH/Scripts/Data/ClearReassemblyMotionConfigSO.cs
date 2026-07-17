using LitMotion;
using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "ClearReassemblyMotionConfig", menuName = "Magnet/Clear Reassembly Motion Config")]
    public sealed class ClearReassemblyMotionConfigSO : ScriptableObject
    {
        [Tooltip("폭발 후 바깥으로 튕기는 거리(칸)")]
        [field: SerializeField] public float BounceCells { get; private set; } = 3f;
        [Tooltip("튕김 LitMotion 시간(초)")]
        [field: SerializeField] public float BounceDuration { get; private set; } = 0.15f;
        [Tooltip("튕김(바깥으로 밀려남) LitMotion 이징")]
        [field: SerializeField] public Ease BounceEase { get; private set; } = Ease.OutQuad;
        [Tooltip("착지(목표 칸 이동) 시간(초)")]
        [field: SerializeField] public float LandDuration { get; private set; } = 0.12f;
        [Tooltip("착지(목표 칸으로 이동) LitMotion 이징")]
        [field: SerializeField] public Ease LandEase { get; private set; } = Ease.OutQuad;
        [Tooltip("비행 중 자전 각속도(도/초)")]
        [field: SerializeField] public float SpinDegreesPerSecond { get; private set; } = 720f;
        [Tooltip("다음 링 시작 지연(초). 같은 링은 동시 이동. 이전 링 완료를 기다리지 않음")]
        [field: SerializeField] public float StaggerPerRing { get; private set; } = 0.12f;

        private void OnValidate()
        {
            BounceCells = Mathf.Max(0f, BounceCells);
            BounceDuration = Mathf.Max(0.01f, BounceDuration);
            LandDuration = Mathf.Max(0.01f, LandDuration);
            SpinDegreesPerSecond = Mathf.Max(0f, SpinDegreesPerSecond);
            StaggerPerRing = Mathf.Max(0f, StaggerPerRing);
        }
    }
}
