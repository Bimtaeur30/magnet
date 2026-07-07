using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace MVP.Forms.Module.Gauge
{
    public class FilledGaugeModule : AbstractGaugeModule
    {
        private Image _targetImage;
        private MotionHandle _handle;

        protected override void Init(GameObject gameObject)
        {
            _targetImage = gameObject.GetComponent<Image>();
            _targetImage.fillAmount = 1;
        }

        public override void SetGauge(float ratio, float duration = 0, Ease ease = Ease.Linear)
        {
            if (_handle.IsActive()) _handle.Cancel();
            _handle = LMotion.Create(_targetImage.fillAmount, ratio, duration)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .BindToFillAmount(_targetImage);
        }

        public override void OnDestroy()
        {
            if (_handle.IsActive()) _handle.Cancel();
        }

        public override void StopCooldown()
        {
            if (_handle.IsActive()) _handle.PlaybackSpeed = 0f;
        }

        public override void StartCooldown()
        {
            if (_handle.IsActive()) _handle.PlaybackSpeed = 1f;
        }
    }
}
