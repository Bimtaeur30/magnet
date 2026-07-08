using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace KTJ.System
{
    [DisallowMultipleComponent]
    [AddComponentMenu("KTJ/System/Floating Y Motion")]
    public sealed class FloatingYMotion : MonoBehaviour
    {
        [Header("Motion")]
        [Min(0f)]
        [SerializeField] private float range = 0.25f;
        [Min(0.01f)]
        [SerializeField] private float duration = 1.6f;
        [SerializeField] private Ease ease = Ease.Linear;

        [Header("Settings")]
        [SerializeField] private bool restorePositionOnDisable = true;
        [SerializeField] private bool ignoreTimeScale = false;

        private MotionHandle _handle;
        private Vector3 _originLocalPosition;

        private void Awake()
        {
            _originLocalPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            Play();
        }

        private void OnDisable()
        {
            StopMotion();

            if (restorePositionOnDisable)
                transform.localPosition = _originLocalPosition;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            range = Mathf.Max(0f, range);
            duration = Mathf.Max(0.01f, duration);
        }
#endif

        [ContextMenu("Restart Floating")]
        public void Play()
        {
            StopMotion();

            _originLocalPosition = transform.localPosition;
            if (Mathf.Approximately(range, 0f)) return;

            var motion = LMotion.Create(0f, Mathf.PI * 2f, duration)
                .WithEase(ease)
                .WithLoops(-1, LoopType.Restart);

            if (ignoreTimeScale)
                motion = motion.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale);

            _handle = motion.Bind(ApplyYPosition);
        }

        [ContextMenu("Stop Floating")]
        public void StopMotion()
        {
            if (!_handle.IsActive()) return;

            _handle.Cancel();
            _handle = default;
        }

        private void ApplyYPosition(float phase)
        {
            Vector3 position = _originLocalPosition;
            position.y += Mathf.Sin(phase) * range;
            transform.localPosition = position;
        }
    }
}


