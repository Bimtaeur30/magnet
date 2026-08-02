using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class RotateRectTransform_UI : MonoBehaviour
{
    [SerializeField] private float degreesPerSecond = 90f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectTransform.Rotate(0f, 0f, degreesPerSecond * Time.unscaledDeltaTime);
    }
}
