using Mvvm;

using GameLib.EventChannelSystem;
using Coffee.UIEffects;
using LitMotion;
using Magnet.Core.Events;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class AllClearUIView : MvvmView<AllClearUIViewModel>
    {
        [SerializeField] private EventChannelSO magnetGameChannel;

        [Tooltip("텍스트 등장과 퇴장 각각의 트윈 시간(초).")]
        [SerializeField, Min(0.01f)] private float tweenDuration = 0.25f;

        [Tooltip("등장 트윈 완료 후 퇴장 시작까지 유지하는 시간(초).")]
        [SerializeField, Min(0f)] private float visibleDuration = 1f;

        [Tooltip("올클리어 시 함께 재생할 선택적 파티클 인스턴스. 비워도 텍스트는 정상 동작합니다.")]
        [SerializeField] private ParticleSystem allClearParticle;

        [Tooltip("텍스트 UIEffect의 Gradation Offset이 0에서 1까지 반복되는 시간(초).")]
        [SerializeField, Min(0.01f)] private float gradationDuration = 1f;

        private MotionHandle animationMotion;
        private MotionHandle gradationMotion;
        private UIEffect textEffect;

        protected override void Awake()
        {
            base.Awake();
            ResetVisuals();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetVisuals();
            StartGradation();
            if (magnetGameChannel != null)
                magnetGameChannel.AddListener<AllClearEvent>(OnAllClear);
        }

        protected override void OnDisable()
        {
            if (magnetGameChannel != null)
                magnetGameChannel.RemoveListener<AllClearEvent>(OnAllClear);
            StopAnimation();
            StopGradation();
            ResetVisuals();
            base.OnDisable();
        }

        private void StartGradation()
        {
            StopGradation();
            if (allClearUITextTMPColorA == null ||
                !allClearUITextTMPColorA.TryGetComponent(out textEffect))
                return;

            textEffect.gradationOffset = 0f;
            gradationMotion = LMotion.Create(0f, 1f, Mathf.Max(0.01f, gradationDuration))
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(Ease.Linear)
                .WithLoops(-1, LoopType.Restart)
                .Bind(value =>
                {
                    if (textEffect != null)
                        textEffect.gradationOffset = value;
                });
        }

        private void StopGradation()
        {
            if (gradationMotion.IsActive())
                gradationMotion.Cancel();
            gradationMotion = default;
            if (textEffect != null)
                textEffect.gradationOffset = 0f;
        }

        private void OnAllClear(AllClearEvent evt)
        {
            StopAnimation();
            ResetVisuals();

            if (allClearParticle != null)
                allClearParticle.Play(true);

            float appearDuration = Mathf.Max(0.01f, tweenDuration);
            float exitStart = appearDuration + Mathf.Max(0f, visibleDuration);
            float totalDuration = exitStart + appearDuration;
            animationMotion = LMotion.Create(0f, totalDuration, totalDuration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(Ease.Linear)
                .WithOnComplete(ResetVisuals)
                .Bind(elapsed =>
                {
                    if (ViewModel == null) return;
                    float alpha = 1f;
                    float scale = 1f;
                    if (elapsed < appearDuration)
                    {
                        float progress = Mathf.Clamp01(elapsed / appearDuration);
                        alpha = EaseUtility.Evaluate(progress, Ease.OutQuad);
                        scale = EaseUtility.Evaluate(progress, Ease.OutElastic);
                    }
                    else if (elapsed >= exitStart)
                    {
                        float progress = Mathf.Clamp01((elapsed - exitStart) / appearDuration);
                        alpha = 1f - EaseUtility.Evaluate(progress, Ease.InQuad);
                        scale = 1f - EaseUtility.Evaluate(progress, Ease.InBack);
                    }

                    ViewModel.TextTMPAlpha = alpha;
                    ViewModel.TextTMPScale = Vector3.one * scale;
                });
        }

        private void StopAnimation()
        {
            if (animationMotion.IsActive())
                animationMotion.Cancel();
            animationMotion = default;
        }

        private void ResetVisuals()
        {
            if (ViewModel != null)
            {
                ViewModel.TextTMPAlpha = 0f;
                ViewModel.TextTMPScale = Vector3.zero;
                ApplyViewModelToView();
            }

            if (allClearParticle != null)
                allClearParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
