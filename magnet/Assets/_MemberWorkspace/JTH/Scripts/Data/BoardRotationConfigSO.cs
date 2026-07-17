using LitMotion;
using UnityEngine;

namespace JTH.Scripts.Data
{
    [CreateAssetMenu(fileName = "BoardRotationConfig", menuName = "Magnet/Board Rotation Config")]
    public sealed class BoardRotationConfigSO : ScriptableObject
    {
        [Tooltip("폭발 처리 후 보드·블록 90° 회전 LitMotion 시간(초)")]
        [field: SerializeField] public float Duration { get; private set; } = 0.2f;
        [Tooltip("재조립 연출이 끝난 뒤 회전 시작 전 대기(초)")]
        [field: SerializeField] public float PreRotationDelay { get; private set; } = 0.25f;
        [Tooltip("보드 90° 회전 시 칸 View 이동 LitMotion 이징")]
        [field: SerializeField] public Ease Ease { get; private set; } = Ease.OutQuad;

        private void OnValidate()
        {
            Duration = Mathf.Max(0.01f, Duration);
            PreRotationDelay = Mathf.Max(0f, PreRotationDelay);
        }
    }
}
