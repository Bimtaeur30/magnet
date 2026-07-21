using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    //TODO 고치기
    public sealed class DragSensitivityRamp
    {
        // private readonly float _rampPerWorldUnit;
        // private readonly float _maxMultiplier;
        // private float _pressOriginWorldX;
        // private float _lastPointerWorldX;
        // private bool _hasOrigin;
        //
        // public DragSensitivityRamp(float rampPerWorldUnit, float maxMultiplier)
        // {
        //     _rampPerWorldUnit = rampPerWorldUnit;
        //     _maxMultiplier = maxMultiplier;
        // }
        //
        // public void Begin(float pressOriginWorldX)
        // {
        //     _pressOriginWorldX = pressOriginWorldX;
        //     _lastPointerWorldX = pressOriginWorldX;
        //     _hasOrigin = true;
        // }
        //
        // public void Reset()
        // {
        //     _hasOrigin = false;
        // }
        //
        // public float UpdateDelta(float pointerWorldX)
        // {
        //     float pointerDeltaX = pointerWorldX - _lastPointerWorldX;
        //     float rampDelta = ApplyPointerDelta(pointerDeltaX, pointerWorldX);
        //     _lastPointerWorldX = pointerWorldX;
        //     return rampDelta;
        // }
        //
        // private float ApplyPointerDelta(
        //     float pointerDeltaX,
        //     float currentPointerWorldX)
        // {
        //     float distanceFromOrigin = _hasOrigin
        //         ? Mathf.Abs(currentPointerWorldX - _pressOriginWorldX)
        //         : 0f;
        //     float multiplier = 1f + distanceFromOrigin * _rampPerWorldUnit;
        //     multiplier = Mathf.Min(multiplier, _maxMultiplier);
        //     return pointerDeltaX * multiplier;
        // }
    }
}
