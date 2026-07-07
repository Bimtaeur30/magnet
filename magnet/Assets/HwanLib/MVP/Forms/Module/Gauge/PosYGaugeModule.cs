using HwanLib.Util;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace MVP.Forms.Module.Gauge
{
    internal class PosYGaugeModule : AbstractGaugeModule
    {
        private RectTransform _targetTransform;
        private MotionHandle _handle;

        protected override void Init(GameObject gameObject)
        {
            _targetTransform = gameObject.GetComponent<RectTransform>();
            Vector3 prevPos = _targetTransform.localPosition;
            _targetTransform.SetPivotWithoutScreenPosChange(new Vector2(0.5f, 0));
            _targetTransform.localPosition = prevPos;
        }

        public override void SetGauge(float ratio, float duration = 0, Ease ease = Ease.Linear)
        {
            if (_handle.IsActive()) _handle.Cancel();
            _handle = LMotion.Create(_targetTransform.localScale.y, ratio, duration)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .BindToLocalScaleY(_targetTransform);
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
