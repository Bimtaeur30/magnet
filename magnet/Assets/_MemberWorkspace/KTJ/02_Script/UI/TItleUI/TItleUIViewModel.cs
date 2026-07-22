using LitMotion;
using Mvvm;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class TItleUIViewModel
    {
        private MotionHandle fadeHandle;
        private MotionHandle scaleHandle;

        public TItleUIViewModel()
        {
            TItleUIAlpha = 0f;
            ButtonScale = Vector3.one;
        }

        public void PlayButtonScaleAnim(float scale)
        {
            if (scaleHandle.IsActive())
                scaleHandle.Cancel();

            ButtonScale = Vector3.one;

            scaleHandle = LMotion.Create(1f, scale, 0.08f)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() =>
                {
                    scaleHandle = LMotion.Create(scale, 1f, 0.12f)
                        .WithEase(Ease.OutBack)
                        .Bind(value => ButtonScale = new Vector3(value, value, value));
                })
                .Bind(value => ButtonScale = new Vector3(value, value, value));
        }

        public void PlayFadeIn(float duration)
        {
            StopFade();

            TItleUIAlpha = 0f;
            if (duration <= 0f)
            {
                TItleUIAlpha = 1f;
                return;
            }

            fadeHandle = LMotion.Create(0f, 1f, duration)
                .WithEase(Ease.OutQuad)
                .Bind(value => TItleUIAlpha = value);
        }

        public void StopFade()
        {
            if (!fadeHandle.IsActive()) return;

            fadeHandle.Cancel();
            fadeHandle = default;
        }
    }
}
