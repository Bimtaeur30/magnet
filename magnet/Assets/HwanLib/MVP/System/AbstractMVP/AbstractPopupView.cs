using System.Collections.Generic;
using MVP.Forms;
using MVP.System.BaseMVP;
using MVP.System.BaseMVP.Form;
using HwanLib.Util;
using UnityEngine;

namespace MVP.System.AbstractMVP
{
    // Popup 연출 전용 View. 닫기 트리거는 BasePresenter.closeTriggers에서 처리한다.
    public abstract class AbstractPopupView : BaseView
    {
        [SerializeField] protected UITransition  transition;
        [SerializeField] private BackgroundForm backgroundForm;

        public override bool IsOpen { get; protected set; }

        private CanvasGroup _canvasGroup;

        public override void InitializeView(IReadOnlyList<BaseForm> forms)
        {
            base.InitializeView(forms);

            _canvasGroup = RootCanvas.gameObject.GetOrAddComponent<CanvasGroup>();
            IsOpen = false;
        }

        public override void OpenView()
        {
            if (IsOpen) return;
            base.OpenView();
            IsOpen = true;
            _canvasGroup.interactable   = true;
            _canvasGroup.blocksRaycasts = true;
            if (backgroundForm != null) backgroundForm.DoFade(true, transition.ShowDuration);
            transition.PlayShow();
        }

        public override void CloseView()
        {
            if (!IsOpen) return;
            IsOpen = false;
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = false;
            if (backgroundForm != null) backgroundForm.DoFade(false, transition.HideDuration);
            transition.PlayHide(() =>
            {
                RootCanvas.gameObject.SetActive(false);
                RaiseOnViewClosed();
            });
        }
    }
}
