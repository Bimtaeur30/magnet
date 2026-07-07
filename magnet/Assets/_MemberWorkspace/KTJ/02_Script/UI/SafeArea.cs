using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    [SerializeField] private bool simulateInEditor = true;

    [Header("Normalized (0~1)")]
    [SerializeField] private Vector2 editorAnchorMin = new(0f, 0.03f);
    [SerializeField] private Vector2 editorAnchorMax = new(1f, 0.97f);

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        Apply();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            rect ??= GetComponent<RectTransform>();
            Apply();
        }
    }
#endif

    private void Apply()
    {
        if (Application.isPlaying || !simulateInEditor)
        {
            Rect safeArea = Screen.safeArea;

            Vector2 min = new(
                safeArea.xMin / Screen.width,
                safeArea.yMin / Screen.height);

            Vector2 max = new(
                safeArea.xMax / Screen.width,
                safeArea.yMax / Screen.height);

            rect.anchorMin = min;
            rect.anchorMax = max;
        }
        else
        {
            // 에디터에서는 직접 입력한 Safe Area 사용
            rect.anchorMin = editorAnchorMin;
            rect.anchorMax = editorAnchorMax;
        }
    }
}