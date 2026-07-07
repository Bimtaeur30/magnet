using System;
using LitMotion;
using LitMotion.Extensions;
using HwanLib.MVP.System.BaseMVP;
using HwanLib.MVP.System.BaseMVP.Form;
using UnityEngine;

namespace HwanLib.MVP.Forms
{
    public class TweenWindowForm : BaseForm, IInitializable
    {
        [SerializeField] private float openDuration = 0.25f;
        [SerializeField] private float closeDuration = 0.225f;
        
        public event Action OnAnimationEnd;

        private MotionHandle _handle;

        public void Initialize()
        {
        }

        public void PlayAnimation(bool isOpen)
            => PlayAnimation(isOpen, isOpen ? openDuration : closeDuration);

        public void PlayAnimation(bool isOpen, float duration)
        {
            if (_handle.IsActive())
            {
                _handle.Complete();
                _handle.Cancel();
                OnAnimationEnd?.Invoke();
            }
            transform.localScale = isOpen ? Vector3.zero : transform.localScale;
            float curDuration = isOpen ? Mathf.Clamp01(1 - transform.localScale.x) * duration
                : transform.localScale.x * closeDuration;
            Vector3 targetScale = isOpen ? Vector3.one : Vector3.zero;
            Ease ease = isOpen ? Ease.InCirc : Ease.InBack;

            _handle = LMotion.Create(transform.localScale, targetScale, curDuration)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() => OnAnimationEnd?.Invoke())
                .BindToLocalScale(transform);
        }
        
        private void OnDestroy()
        {
            if (_handle.IsActive())
            {
                _handle.Complete();
                _handle.Cancel();
            }
        }
    }
}
