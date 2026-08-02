using Mvvm;

using LitMotion;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class ScoreUIViewModel
    {
        private MotionHandle _currentStageScaleHandle;
        private Vector3 _currentStageScale = Vector3.one;

        public Vector3 CurrentStageScale
        {
            get => _currentStageScale;
            private set => SetProperty(ref _currentStageScale, value);
        }

        public void SetCurrentStage(int stage)
        {
            CurrentStageTxt = stage.ToString();
        }

        public void SetBestStage(int stage)
        {
            BestStageTxt = stage.ToString();
        }

        public void PlayCurrentStageScaleAnimation()
        {
            StopCurrentStageScaleAnimation();

            _currentStageScaleHandle = LMotion.Create(1f, 1.2f, 0.08f)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() =>
                {
                    _currentStageScaleHandle = LMotion.Create(1.2f, 1f, 0.12f)
                        .WithEase(Ease.OutBack)
                        .Bind(SetCurrentStageScale);
                })
                .Bind(SetCurrentStageScale);
        }

        public void StopCurrentStageScaleAnimation()
        {
            if (_currentStageScaleHandle.IsActive())
            {
                _currentStageScaleHandle.Cancel();
                _currentStageScaleHandle = default;
            }

            CurrentStageScale = Vector3.one;
        }

        private void SetCurrentStageScale(float scale)
        {
            CurrentStageScale = new Vector3(scale, scale, 1f);
        }
    }
}
