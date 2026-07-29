using UnityEngine;

namespace JTH.Scripts.Domain.Placement
{
    public sealed class DragSensitivityRamp
    {
        private readonly float _rampPerWorldUnit;
        private Vector2 _pressOriginPos;
        private Vector2 _lastPointerPos;
        private bool _hasOrigin;
        
        public DragSensitivityRamp(float rampPerWorldUnit)
        {
            _rampPerWorldUnit = rampPerWorldUnit;
        }
        
        public void Begin(Vector2 pressOriginPos)
        {
            _pressOriginPos = pressOriginPos;
            _lastPointerPos = pressOriginPos;
            _hasOrigin = true;
        }
        
        public void Reset()
        {
            _hasOrigin = false;
        }
        
        public Vector2 UpdateDelta(Vector2 pointerWorldPos)
        {
            Vector2 pointerDelta = pointerWorldPos - _lastPointerPos;
            Vector2 rampDelta = ApplyPointerDelta(pointerDelta, pointerWorldPos);
            _lastPointerPos = pointerWorldPos;
            return rampDelta;
        }
        
        private Vector2 ApplyPointerDelta(
            Vector2 pointerDelta,
            Vector2 currentPointerWorld)
        {
            float distanceFromOrigin = _hasOrigin
                ? (_pressOriginPos - currentPointerWorld).magnitude
                : 0;
            float multiplier = 1f + distanceFromOrigin * _rampPerWorldUnit;
            return pointerDelta * multiplier;
        }
    }
}
