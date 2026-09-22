using GameLib.EventChannelSystem;
using Magnet.Core.Events;
using Mvvm;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.UI
{
    public sealed partial class ComboUIView : MvvmView<ComboUIViewModel>
    {
        [SerializeField] private EventChannelSO MagnetGameChannel;

        [Tooltip("콤보 UI의 화면 위치를 월드 좌표로 변환해 재생할 파티클 인스턴스. 비워도 콤보 UI는 동작합니다.")]
        [SerializeField] private ParticleSystem comboParticle;

        private readonly Vector3[] boundsCorners = new Vector3[4];
        private const float ScreenPadding = 16f;
        // OutBack 및 역방향 InBack의 최대 확대(약 1.1001)보다 조금 크게 측정한다.
        private const float PeakScale = 1.11f;

        protected override void OnEnable()
        {
            base.OnEnable();
            MagnetGameChannel.AddListener<ComboChangedEvent>(OnComboChanged);
        }

        protected override void OnDisable()
        {
            MagnetGameChannel.RemoveListener<ComboChangedEvent>(OnComboChanged);
            ViewModel.StopComboAnimation();
            if (comboParticle != null)
                comboParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
            if (canvas != null) canvas = canvas.rootCanvas;
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

            // 부모 로컬 좌표와 anchoredPosition은 앵커/피벗에 따라 다르다.
            comboUIComboAnchoredPosition.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            ViewModel.TextTMP1 = evt.Combo.ToString();
            float fitScale = ClampComboToScreen(canvas, uiCamera);
            ViewModel.ShowCombo(evt.Combo, comboUIComboAnchoredPosition.anchoredPosition, fitScale);
            PlayComboParticle(worldCamera, uiCamera, evt.WorldPosition);
        }

        private float ClampComboToScreen(Canvas canvas, Camera uiCamera)
        {
            Rect visible = Screen.safeArea;
            Rect viewport = canvas != null ? canvas.pixelRect : new Rect(0, 0, Screen.width, Screen.height);
            visible = Rect.MinMaxRect(Mathf.Max(visible.xMin, viewport.xMin) + ScreenPadding,
                Mathf.Max(visible.yMin, viewport.yMin) + ScreenPadding,
                Mathf.Min(visible.xMax, viewport.xMax) - ScreenPadding,
                Mathf.Min(visible.yMax, viewport.yMax) - ScreenPadding);
            if (visible.width <= 0f || visible.height <= 0f) return 1f;

            var scaleRect = comboUIComboLocalScale;
            Vector3 savedScale = scaleRect.localScale;
            try
            {
                scaleRect.localScale = Vector3.one * PeakScale;
                Canvas.ForceUpdateCanvases();
                var graphics = comboUIComboAnchoredPosition.GetComponentsInChildren<Graphic>();
                Rect bounds = GetScreenBounds(graphics, uiCamera);
                float fit = Mathf.Min(1f, visible.width / Mathf.Max(1f, bounds.width),
                    visible.height / Mathf.Max(1f, bounds.height));
                scaleRect.localScale *= fit;
                bounds = GetScreenBounds(graphics, uiCamera);
                Vector2 shift = new Vector2(
                    bounds.width >= visible.width ? visible.center.x - bounds.center.x :
                        Mathf.Clamp(0f, visible.xMin - bounds.xMin, visible.xMax - bounds.xMax),
                    bounds.height >= visible.height ? visible.center.y - bounds.center.y :
                        Mathf.Clamp(0f, visible.yMin - bounds.yMin, visible.yMax - bounds.yMax));
                Vector2 pivot = RectTransformUtility.WorldToScreenPoint(uiCamera, comboUIComboAnchoredPosition.position);
                var parent = (RectTransform)comboUIComboAnchoredPosition.parent;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, pivot + shift, uiCamera, out var world))
                    comboUIComboAnchoredPosition.position = world;
                return fit;
            }
            finally
            {
                scaleRect.localScale = savedScale;
            }
        }

        private Rect GetScreenBounds(Graphic[] graphics, Camera uiCamera)
        {
            // 축소 중에도 안전하도록 스케일 피벗까지 포함한 전체 이동 범위를 측정한다.
            Vector2 min = RectTransformUtility.WorldToScreenPoint(uiCamera, comboUIComboLocalScale.position);
            Vector2 max = min;
            foreach (var graphic in graphics)
            {
                if (!graphic.isActiveAndEnabled) continue;
                if (graphic is TMP_Text text)
                {
                    text.ForceMeshUpdate();
                    Bounds bounds = text.textBounds;
                    var rect = text.rectTransform;
                    boundsCorners[0] = rect.TransformPoint(new Vector3(bounds.min.x, bounds.min.y));
                    boundsCorners[1] = rect.TransformPoint(new Vector3(bounds.min.x, bounds.max.y));
                    boundsCorners[2] = rect.TransformPoint(new Vector3(bounds.max.x, bounds.max.y));
                    boundsCorners[3] = rect.TransformPoint(new Vector3(bounds.max.x, bounds.min.y));
                }
                else graphic.rectTransform.GetWorldCorners(boundsCorners);
                foreach (var corner in boundsCorners)
                {
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(uiCamera, corner);
                    min = Vector2.Min(min, point);
                    max = Vector2.Max(max, point);
                }
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private void PlayComboParticle(Camera worldCamera, Camera uiCamera, Vector3 eventWorldPosition)
        {
            if (comboParticle == null)
                return;

            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
                uiCamera, comboUIComboAnchoredPosition.position);
            float depth = worldCamera.WorldToScreenPoint(eventWorldPosition).z;
            if (depth <= 0f)
                return;

            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, depth));
            comboParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            comboParticle.transform.position = worldPosition;
            comboParticle.Play(true);
        }
    }
}
