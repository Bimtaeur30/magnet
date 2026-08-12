using GameLib.EventChannelSystem;
using Magnet.Core.Events;
using Mvvm;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class ComboUIView : MvvmView<ComboUIViewModel>
    {
        [SerializeField] private EventChannelSO MagnetGameChannel;

        protected override void OnEnable()
        {
            base.OnEnable();
            MagnetGameChannel.AddListener<ComboChangedEvent>(OnComboChanged);
        }

        protected override void OnDisable()
        {
            MagnetGameChannel.RemoveListener<ComboChangedEvent>(OnComboChanged);
            ViewModel.StopComboAnimation();
            base.OnDisable();
        }

        private void OnComboChanged(ComboChangedEvent evt)
        {
            Camera worldCamera = Camera.main;
            RectTransform parentRect = comboUIComboAnchoredPosition.parent as RectTransform;

            if (worldCamera == null || parentRect == null)
            {
                Debug.LogWarning("[ComboUIView] 좌표 변환에 필요한 Camera 또는 부모 RectTransform이 없습니다.", this);
                return;
            }

            Vector2 screenPosition = worldCamera.WorldToScreenPoint(evt.WorldPosition);
            Canvas canvas = comboUIComboAnchoredPosition.GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    screenPosition,
                    uiCamera,
                    out Vector2 localPosition))
            {
                return;
            }

            ViewModel.ShowCombo(evt.Combo, localPosition);
        }
    }
}
