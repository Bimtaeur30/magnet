using System.Collections.Generic;
using GameLib.EventChannelSystem;
using LitMotion;
using Magnet.Core.Events;
using Mvvm;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed partial class NiceBlockUIView : MvvmView<NiceBlockUIViewModel>
    {
        [SerializeField] private EventChannelSO MagnetGameChannel;
        [SerializeField] private RectTransform niceIconPrefab;
        [SerializeField] private RectTransform iconParent;
        [SerializeField] private Camera worldCamera;
        [Range(0.01f, 0.5f), SerializeField] private float rotationDuration = 0.25f;
        [Min(0.01f), SerializeField] private float fadeDuration = 0.25f;

        private sealed class IconEffect
        {
            public RectTransform Icon;
            public MotionHandle Motion;
        }

        private readonly List<IconEffect> effects = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            if (MagnetGameChannel != null)
                MagnetGameChannel.AddListener<UniqueCorrectPlacementEvent>(OnCorrectPlacement);
        }

        protected override void OnDisable()
        {
            if (MagnetGameChannel != null)
                MagnetGameChannel.RemoveListener<UniqueCorrectPlacementEvent>(OnCorrectPlacement);
            foreach (var effect in effects)
            {
                if (effect.Motion.IsActive()) effect.Motion.Cancel();
                if (effect.Icon != null) Destroy(effect.Icon.gameObject);
            }
            effects.Clear();
            base.OnDisable();
        }

        private void OnCorrectPlacement(UniqueCorrectPlacementEvent evt)
        {
            if (evt.WorldPositions == null) return;
            var parent = iconParent != null ? iconParent : transform as RectTransform;
            var camera = worldCamera != null ? worldCamera : Camera.main;
            var canvas = parent != null ? parent.GetComponentInParent<Canvas>() : null;
            if (niceIconPrefab == null || parent == null || camera == null || canvas == null)
            {
                Debug.LogWarning("[NiceBlockUIView] NiceIcon 프리팹, Canvas 아래 부모, 월드 카메라를 확인하세요.", this);
                return;
            }
            var rootCanvas = canvas.rootCanvas;
            var uiCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
            if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay && uiCamera == null)
            {
                Debug.LogWarning("[NiceBlockUIView] Canvas의 World Camera를 지정하세요.", this);
                return;
            }
            foreach (var position in evt.WorldPositions)
            {
                var screen = camera.WorldToScreenPoint(position);
                if (screen.z <= 0f) continue;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, uiCamera, out var local))
                    CreateIcon(parent, local);
            }
        }

        private void CreateIcon(RectTransform parent, Vector2 position)
        {
            var icon = Instantiate(niceIconPrefab, parent, false);
            icon.localPosition = new Vector3(position.x, position.y, 0f);
            icon.gameObject.SetActive(true);
            foreach (var graphic in icon.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
            var group = icon.GetComponent<CanvasGroup>();
            if (group == null) group = icon.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = false;
            var initialRotation = icon.localRotation;
            var turnTime = Mathf.Clamp(rotationDuration, 0.01f, 0.5f);
            var fadeTime = Mathf.Max(0.01f, fadeDuration);
            var effect = new IconEffect { Icon = icon };
            effects.Add(effect);
            effect.Motion = LMotion.Create(0f, 1f + fadeTime, 1f + fadeTime)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() =>
                {
                    effects.Remove(effect);
                    if (icon != null) Destroy(icon.gameObject);
                })
                .Bind(elapsed =>
                {
                    if (icon == null) return;
                    // UI 정면에서 왼쪽 회전은 Z축 양의 방향이다.
                    float progress = elapsed < turnTime
                        ? Mathf.SmoothStep(0f, 1f, elapsed / turnTime)
                        : 1f - Mathf.SmoothStep(0f, 1f, (elapsed - turnTime) / turnTime);
                    icon.localRotation = initialRotation * Quaternion.Euler(0f, 0f, 30f * progress);
                    group.alpha = 1f - Mathf.Clamp01((elapsed - 1f) / fadeTime);
                });
        }
    }
}
