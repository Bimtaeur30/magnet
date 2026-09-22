using Mvvm;

using GameLib.EventChannelSystem;
using LitMotion;
using Magnet.Core.Events;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class AllClearUIView : MvvmView<AllClearUIViewModel>
    {
        [SerializeField] private EventChannelSO magnetGameChannel;

        [Tooltip("텍스트 알파와 로컬 스케일이 0에서 1까지 변하는 시간(초).")]
        [SerializeField, Min(0.01f)] private float tweenDuration = 0.25f;

        [Tooltip("등장 트윈 완료 후 초기 상태로 돌아가기까지 유지하는 시간(초).")]
        [SerializeField, Min(0f)] private float visibleDuration = 1f;

        [Tooltip("올클리어 시 함께 재생할 선택적 파티클 인스턴스. 비워도 텍스트는 정상 동작합니다.")]
        [SerializeField] private ParticleSystem allClearParticle;

        private MotionHandle animationMotion;

        protected override void Awake()
        {
            base.Awake();
            ResetVisuals();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetVisuals();
            if (magnetGameChannel != null)
                magnetGameChannel.AddListener<AllClearEvent>(OnAllClear);
        }

        protected override void OnDisable()
        {
            if (magnetGameChannel != null)
                magnetGameChannel.RemoveListener<AllClearEvent>(OnAllClear);
            StopAnimation();
            ResetVisuals();
            base.OnDisable();
        }

        private void OnAllClear(AllClearEvent evt)
        {
            StopAnimation();
            ResetVisuals();

            if (allClearParticle != null)
                allClearParticle.Play(true);

            float appearDuration = Mathf.Max(0.01f, tweenDuration);
            float totalDuration = appearDuration + Mathf.Max(0f, visibleDuration);
            animationMotion = LMotion.Create(0f, totalDuration, totalDuration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(ResetVisuals)
                .Bind(elapsed =>
                {
                    float progress = Mathf.Clamp01(elapsed / appearDuration);
                    // Ease.OutQuad: 등장 후에는 1을 유지한다.
                    float value = 1f - (1f - progress) * (1f - progress);
                    if (ViewModel == null) return;
                    ViewModel.TextTMPAlpha = value;
                    ViewModel.TextTMPScale = Vector3.one * value;
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
