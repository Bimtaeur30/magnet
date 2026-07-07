using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace HwanLib.MVP.Visual
{
    /// <summary>
    /// 대상 Graphic의 알파 또는 localScale를 min↔max로 주기 반복하는 독립 연출.
    /// InteractionFeedback과 무관. 아무 GO에 부착하면 기본값으로 동작.
    /// </summary>
    public class ImagePulse : MonoBehaviour
    {
        private enum PulseMode { Alpha, Scale }

        [SerializeField] private PulseMode mode = PulseMode.Alpha;
        [SerializeField] private float cycleDuration = 1f; // min→max 1방향 시간(초)
        [SerializeField] private float min = 0.4f;
        [SerializeField] private float max = 1f;
        [SerializeField] private Ease ease = Ease.InOutSine;
        [SerializeField] private bool ignoreTimeScale = true;

        private Graphic _graphic;
        private MotionHandle _handle;

        private void Awake()
        {
            _graphic = GetComponent<Graphic>();
        }

        private void OnEnable()
        {
            Play();
        }

        private void OnDisable()
        {
            if (_handle.IsActive()) _handle.Cancel();
            _handle = default;
        }

        private void Play()
        {
            if (_handle.IsActive()) _handle.Cancel();

            if (mode == PulseMode.Alpha)
            {
                if (_graphic == null) return;
                SetAlpha(min);
                var motion = LMotion.Create(min, max, cycleDuration)
                    .WithEase(ease)
                    .WithLoops(-1, LoopType.Yoyo);
                if (ignoreTimeScale) motion = motion.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale);
                _handle = motion.BindToColorA(_graphic);
                return;
            }

            transform.localScale = Vector3.one * min;
            var scaleMotion = LMotion.Create(min, max, cycleDuration)
                .WithEase(ease)
                .WithLoops(-1, LoopType.Yoyo);
            if (ignoreTimeScale) scaleMotion = scaleMotion.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale);
            _handle = scaleMotion.BindToLocalScaleXYZ(transform);
        }

        private void SetAlpha(float a)
        {
            if (_graphic == null) return;
            Color c = _graphic.color;
            c.a = a;
            _graphic.color = c;
        }
    }
}
