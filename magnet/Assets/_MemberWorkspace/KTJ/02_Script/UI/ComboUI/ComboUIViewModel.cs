using LitMotion;
using Mvvm;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class ComboUIViewModel
    {
        private const float ShowDuration = 0.2f;
        private const float VisibleDuration = 1f;
        private const float HideDuration = 0.2f;

        private MotionHandle _alphaHandle;
        private MotionHandle _scaleHandle;

        public void ShowCombo(int combo)
        {
            StopComboAnimation();
            TextTMP1 = combo.ToString();

            _alphaHandle = LMotion.Create(0f, 1f, ShowDuration)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() =>
                {
                    _alphaHandle = LMotion.Create(1f, 0f, HideDuration)
                        .WithDelay(VisibleDuration)
                        .WithEase(Ease.InQuad)
                        .Bind(SetComboAlpha);
                })
                .Bind(SetComboAlpha);

            _scaleHandle = LMotion.Create(0f, 1f, ShowDuration)
                .WithEase(Ease.OutBack)
                .WithOnComplete(() =>
                {
                    _scaleHandle = LMotion.Create(1f, 0f, HideDuration)
                        .WithDelay(VisibleDuration)
                        .WithEase(Ease.InBack)
                        .Bind(SetComboScale);
                })
                .Bind(SetComboScale);
        }

        public void StopComboAnimation()
        {
            Cancel(ref _alphaHandle);
            Cancel(ref _scaleHandle);
            ComboAlpha = 0f;
            ComboScale = Vector3.zero;
        }

        private void SetComboAlpha(float alpha)
        {
            ComboAlpha = alpha;
        }

        private void SetComboScale(float scale)
        {
            ComboScale = Vector3.one * scale;
        }

        private static void Cancel(ref MotionHandle handle)
        {
            if (handle.IsActive())
            {
                handle.Cancel();
            }

            handle = default;
        }
    }
}
