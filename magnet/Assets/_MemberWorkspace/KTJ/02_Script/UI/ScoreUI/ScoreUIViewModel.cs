using Mvvm;

using LitMotion;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class ScoreUIViewModel
    {
        private MotionHandle _currentScoreScaleHandle;
        private Vector3 _currentScoreScale = Vector3.one;

        public Vector3 CurrentScoreScale
        {
            get => _currentScoreScale;
            private set => SetProperty(ref _currentScoreScale, value);
        }

        public void SetCurrentScore(int score)
        {
            CurrentScoreTxt = score.ToString();
        }

        public void SetBestScore(int score)
        {
            BestScoreTxt = score.ToString();
        }

        public void PlayCurrentScoreScaleAnimation()
        {
            StopCurrentScoreScaleAnimation();

            _currentScoreScaleHandle = LMotion.Create(1f, 1.2f, 0.08f)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() =>
                {
                    _currentScoreScaleHandle = LMotion.Create(1.2f, 1f, 0.12f)
                        .WithEase(Ease.OutBack)
                        .Bind(SetCurrentScoreScale);
                })
                .Bind(SetCurrentScoreScale);
        }

        public void StopCurrentScoreScaleAnimation()
        {
            if (_currentScoreScaleHandle.IsActive())
            {
                _currentScoreScaleHandle.Cancel();
                _currentScoreScaleHandle = default;
            }

            CurrentScoreScale = Vector3.one;
        }

        private void SetCurrentScoreScale(float scale)
        {
            CurrentScoreScale = new Vector3(scale, scale, 1f);
        }
    }
}
