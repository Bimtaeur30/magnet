using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    /// <summary>
    /// Press 시작 포인터 X와의 거리에 따라 블록 이동 배율을 높인다. Pointer Up 시 Reset.
    /// </summary>
    public sealed class DragSensitivityRamp
    {
        private readonly float _rampPerWorldUnit;
        private readonly float _maxMultiplier;
        private float _pressOriginWorldX;
        private float _lastPointerWorldX;
        private bool _hasOrigin;

        public DragSensitivityRamp(float rampPerWorldUnit, float maxMultiplier)
        {
            _rampPerWorldUnit = rampPerWorldUnit;
            _maxMultiplier = maxMultiplier;
        }

        public void Begin(float pressOriginWorldX)
        {
            _pressOriginWorldX = pressOriginWorldX;
            _lastPointerWorldX = pressOriginWorldX;
            _hasOrigin = true;
        }

        public void Reset()
        {
            _hasOrigin = false;
        }

        public float UpdateDelta(float pointerWorldX)
        {
            float pointerDeltaX = pointerWorldX - _lastPointerWorldX;
            float rampDelta = ApplyPointerDelta(pointerDeltaX, pointerWorldX);
            _lastPointerWorldX = pointerWorldX;
            return rampDelta;
        }

        private float ApplyPointerDelta(
            float pointerDeltaX,
            float currentPointerWorldX)
        {
            float distanceFromOrigin = _hasOrigin
                ? Mathf.Abs(currentPointerWorldX - _pressOriginWorldX)
                : 0f;
            float multiplier = 1f + distanceFromOrigin * _rampPerWorldUnit;
            multiplier = Mathf.Min(multiplier, _maxMultiplier);
            return pointerDeltaX * multiplier;
        }
    }
}
