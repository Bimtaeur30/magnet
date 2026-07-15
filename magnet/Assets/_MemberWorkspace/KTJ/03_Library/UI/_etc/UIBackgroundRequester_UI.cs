using GameLib.EventChannelSystem;
using UnityEngine;

public sealed class UIBackgroundRequester_UI : MonoBehaviour
{
    [SerializeField] private EventChannelSO magnetGameChannel;
    [SerializeField] private bool requestOnEnable = true;
    [SerializeField] private Color backgroundColor = Color.black;
    [Range(0f, 1f)]
    [SerializeField] private float backgroundAlpha = 0.7f;
    [Min(0f)]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private bool raycastTarget = true;

    private bool isRequesting;

    private void Awake()
    {
        Debug.Assert(magnetGameChannel != null,
            "[UIBackgroundRequester_UI] EventChannelSO is not assigned.", this);
    }

    private void OnEnable()
    {
        if (requestOnEnable)
        {
            RequestBackground();
        }
    }

    private void OnDisable()
    {
        ReleaseBackground();
    }

    private void OnValidate()
    {
        backgroundAlpha = Mathf.Clamp01(backgroundAlpha);
        fadeDuration = Mathf.Max(0f, fadeDuration);
    }

    public void RequestBackground()
    {
        isRequesting = true;
        magnetGameChannel.RaiseEvent(UIEvents.BackgroundRequestEvent.Init(
            this,
            true,
            backgroundColor,
            backgroundAlpha,
            fadeDuration,
            raycastTarget));
    }

    public void ReleaseBackground()
    {
        if (!isRequesting)
        {
            return;
        }

        isRequesting = false;
        magnetGameChannel.RaiseEvent(UIEvents.BackgroundRequestEvent.Init(
            this,
            false,
            backgroundColor,
            backgroundAlpha,
            fadeDuration,
            raycastTarget));
    }
}
