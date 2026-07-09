using System.Collections.Generic;
using HwanLib.MVP.System.BaseMVP;
using HwanLib.MVP.System.BaseMVP.Form;
using LitMotion;
using UnityEngine;

namespace TitleUI.TitleUI
{
    public class TitleUIView : BaseView
    {
        [Min(0f)]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private Ease fadeInEase = Ease.OutQuad;

        private CanvasGroup _canvasGroup;
        private MotionHandle _fadeHandle;

        private void Start()
        {
            if (RootCanvas != null && !RootCanvas.gameObject.activeSelf) return;

            ResolveCanvasGroup();
            PlayFadeIn();
        }

        public override void InitializeView(IReadOnlyList<BaseForm> forms)
        {
            base.InitializeView(forms);
            ResolveCanvasGroup();
        }

        public override void OpenView()
        {
            base.OpenView();
            PlayFadeIn();
        }

        public override void CloseView()
        {
            StopFade();
            base.CloseView();
        }

        public override void OnDestroyView()
        {
            StopFade();
        }

        private void PlayFadeIn()
        {
            StopFade();

            _canvasGroup.alpha = 0f;
            _fadeHandle = LMotion.Create(0f, 1f, fadeInDuration)
                .WithEase(fadeInEase)
                .Bind(alpha => _canvasGroup.alpha = alpha);
        }

        private void ResolveCanvasGroup()
        {
            if (_canvasGroup != null) return;

            if (RootCanvas != null)
            {
                _canvasGroup = RootCanvas.GetComponent<CanvasGroup>();

                if (_canvasGroup == null)
                    _canvasGroup = RootCanvas.gameObject.AddComponent<CanvasGroup>();

                return;
            }

            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void StopFade()
        {
            if (!_fadeHandle.IsActive()) return;

            _fadeHandle.Cancel();
            _fadeHandle = default;
        }
    }
}
