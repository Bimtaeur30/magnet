using System;
using System.Collections.Generic;
using GameLib.EventChannelSystem;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class UIBackgroundController_UI : MonoBehaviour
{
    [SerializeField] private EventChannelSO magnetGameChannel;
    [SerializeField] private Ease fadeEase = Ease.OutQuad;

    private readonly List<BackgroundRequest> requests = new();
    private Image backgroundImage;
    private MotionHandle fadeHandle;

    private void Awake()
    {
        Debug.Assert(magnetGameChannel != null,
            "[UIBackgroundController_UI] EventChannelSO is not assigned.", this);

        backgroundImage = GetComponent<Image>();
        SetAlpha(0f);
        backgroundImage.raycastTarget = false;
        backgroundImage.enabled = false;
    }

    private void OnEnable()
    {
        magnetGameChannel.AddListener<UIBackgroundRequestEvent>(HandleRequest);
    }

    private void OnDisable()
    {
        magnetGameChannel.RemoveListener<UIBackgroundRequestEvent>(HandleRequest);
        StopFade();
        requests.Clear();

        if (backgroundImage == null)
        {
            return;
        }

        SetAlpha(0f);
        backgroundImage.raycastTarget = false;
        backgroundImage.enabled = false;
    }

    private void HandleRequest(UIBackgroundRequestEvent evt)
    {
        if (evt.Requester == null)
        {
            return;
        }

        RemoveInvalidRequests();
        RemoveRequest(evt.Requester);

        if (evt.IsUsing)
        {
            requests.Add(new BackgroundRequest(
                evt.Requester,
                evt.Color,
                evt.Alpha,
                evt.FadeDuration,
                evt.RaycastTarget));
        }

        ApplyLatestRequest(evt.FadeDuration);
    }

    private void ApplyLatestRequest(float releaseFadeDuration)
    {
        StopFade();

        if (requests.Count == 0)
        {
            FadeOut(releaseFadeDuration);
            return;
        }

        BackgroundRequest request = requests[^1];
        Color targetColor = request.Color;
        targetColor.a = request.Alpha;

        backgroundImage.enabled = true;
        backgroundImage.raycastTarget = request.RaycastTarget;

        if (request.FadeDuration <= 0f)
        {
            backgroundImage.color = targetColor;
            return;
        }

        Color startColor = backgroundImage.color;
        fadeHandle = LMotion.Create(startColor, targetColor, request.FadeDuration)
            .WithEase(fadeEase)
            .Bind(value => backgroundImage.color = value);
    }

    private void FadeOut(float duration)
    {
        backgroundImage.raycastTarget = false;

        if (!backgroundImage.enabled)
        {
            SetAlpha(0f);
            return;
        }

        if (duration <= 0f)
        {
            SetAlpha(0f);
            backgroundImage.enabled = false;
            return;
        }

        float startAlpha = backgroundImage.color.a;
        fadeHandle = LMotion.Create(startAlpha, 0f, duration)
            .WithEase(fadeEase)
            .WithOnComplete(() =>
            {
                SetAlpha(0f);
                backgroundImage.enabled = false;
                fadeHandle = default;
            })
            .Bind(SetAlpha);
    }

    private void RemoveRequest(UnityEngine.Object requester)
    {
        for (int i = requests.Count - 1; i >= 0; i--)
        {
            if (ReferenceEquals(requests[i].Requester, requester))
            {
                requests.RemoveAt(i);
            }
        }
    }

    private void RemoveInvalidRequests()
    {
        for (int i = requests.Count - 1; i >= 0; i--)
        {
            if (requests[i].Requester == null)
            {
                requests.RemoveAt(i);
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        Color color = backgroundImage.color;
        color.a = alpha;
        backgroundImage.color = color;
    }

    private void StopFade()
    {
        if (!fadeHandle.IsActive())
        {
            return;
        }

        fadeHandle.Cancel();
        fadeHandle = default;
    }

    private readonly struct BackgroundRequest
    {
        public readonly UnityEngine.Object Requester;
        public readonly Color Color;
        public readonly float Alpha;
        public readonly float FadeDuration;
        public readonly bool RaycastTarget;

        public BackgroundRequest(
            UnityEngine.Object requester,
            Color color,
            float alpha,
            float fadeDuration,
            bool raycastTarget)
        {
            Requester = requester;
            Color = color;
            Alpha = alpha;
            FadeDuration = fadeDuration;
            RaycastTarget = raycastTarget;
        }
    }
}
