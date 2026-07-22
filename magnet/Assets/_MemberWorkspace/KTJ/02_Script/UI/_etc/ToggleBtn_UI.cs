using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Toggle_UI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image handleImage;
    [SerializeField] private Color onBackgroundColor = Color.white;
    [SerializeField] private Color offBackgroundColor = Color.gray;
    [SerializeField] private bool isOn;
    [SerializeField] private UnityEvent<bool> onToggleChanged = new UnityEvent<bool>();

    private Button _button;
    private RectTransform _handleRect;

    public bool IsOn => isOn;
    public UnityEvent<bool> OnToggleChanged => onToggleChanged;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _handleRect = handleImage != null ? handleImage.rectTransform : null;

        Debug.Assert(backgroundImage != null, "[Toggle_UI] Background Image is not assigned.", this);
        Debug.Assert(handleImage != null, "[Toggle_UI] Handle Image is not assigned.", this);
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(Toggle);
        ApplyState(true);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(Toggle);
    }

    public void SetState(bool value)
    {
        if (isOn == value) return;

        isOn = value;
        ApplyState(true);
    }

    private void Toggle()
    {
        SetState(!isOn);
    }

    private void ApplyState(bool invokeEvent)
    {
        if (backgroundImage != null)
            backgroundImage.color = isOn ? onBackgroundColor : offBackgroundColor;

        if (_handleRect != null)
        {
            Vector2 anchor = isOn ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f);
            _handleRect.anchorMin = anchor;
            _handleRect.anchorMax = anchor;
            _handleRect.pivot = anchor;
            _handleRect.anchoredPosition = Vector2.zero;
        }

        if (invokeEvent)
            onToggleChanged?.Invoke(isOn);
    }
}
