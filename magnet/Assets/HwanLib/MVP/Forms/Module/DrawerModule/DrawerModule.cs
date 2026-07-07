using System;
using LitMotion;
using LitMotion.Extensions;
using HwanLib.Util;
using UnityEngine;

namespace MVP.Forms.Module.DrawerModule
{
    public class DrawerModule
    {
        public event Action<bool> OnDrawEnd;
        
        private RectTransform _rectTrm;
        private Vector2 _originalPos;
        private DrawDirection _drawDirection;
        private MotionHandle _drawHandle;
        
        public DrawerModule(RectTransform rectTrm, DrawDirection direction)
        {
            _rectTrm = rectTrm;
            Vector2 targetPivot = direction switch
            {
                DrawDirection.Up => new Vector2(0.5f, 0),
                DrawDirection.Down => new Vector2(0.5f, 1),
                DrawDirection.Left => new Vector2(1, 0.5f),
                DrawDirection.Right => new Vector2(0, 0.5f),
                _ => Vector2.zero
            };
            
            _rectTrm.SetPivotWithoutScreenPosChange(targetPivot);
            
            _originalPos = _rectTrm.anchoredPosition;
            _drawDirection = direction;
        }

        public void Draw(bool isIn, float duration, bool setActive)
        {
            if (_drawHandle.IsActive())
            {
                _drawHandle.Complete();
                _drawHandle.Cancel();
            }
            
            if (setActive && isIn)
                _rectTrm.gameObject.SetActive(true);

            if (_drawDirection is DrawDirection.Up or DrawDirection.Down)
            {
                _drawHandle = LMotion.Create(_rectTrm.anchoredPosition.y, isIn ? _originalPos.y : 0f, duration)
                    .WithEase(Ease.OutBack)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithOnComplete(() =>
                    {
                        if (setActive && isIn == false)
                            _rectTrm.gameObject.SetActive(false);
                        OnDrawEnd?.Invoke(isIn);
                    })
                    .BindToAnchoredPositionY(_rectTrm);
            }
            else
            {
                _drawHandle = LMotion.Create(_rectTrm.anchoredPosition.x, isIn ? _originalPos.x : 0f, duration)
                    .WithEase(Ease.OutBack)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithOnComplete(() =>
                    {
                        if (setActive && isIn == false)
                            _rectTrm.gameObject.SetActive(false);
                        OnDrawEnd?.Invoke(isIn);
                    })
                    .BindToAnchoredPositionX(_rectTrm);
            }
        }
    }
}
