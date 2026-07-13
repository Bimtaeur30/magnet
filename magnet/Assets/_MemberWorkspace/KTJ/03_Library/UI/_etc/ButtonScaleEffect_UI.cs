using LitMotion;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonScaleEffect_UI : MonoBehaviour
{
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 pressedScale = Vector3.one * 1.1f;
    [Min(0f)]
    [SerializeField] private float scaleUpDuration = 0.08f;
    [Min(0f)]
    [SerializeField] private float scaleDownDuration = 0.12f;
    [SerializeField] private Ease ease = Ease.OutQuad;

    private Button btn;
    private MotionHandle scaleHandle;

    private void Awake()
    {
        btn = GetComponent<Button>();
        transform.localScale = normalScale;
    }

    private void OnValidate()
    {
        scaleUpDuration = Mathf.Max(0f, scaleUpDuration);
        scaleDownDuration = Mathf.Max(0f, scaleDownDuration);
    }

    private void OnEnable()
    {
        btn.onClick.AddListener(Play);
    }

    private void OnDisable()
    {
        btn.onClick.RemoveListener(Play);
        StopMotion();
        transform.localScale = normalScale;
    }

    private void Play()
    {
        StopMotion();

        scaleHandle = LMotion.Create(transform.localScale, pressedScale, scaleUpDuration)
            .WithEase(ease)
            .WithOnComplete(() =>
            {
                scaleHandle = LMotion.Create(pressedScale, normalScale, scaleDownDuration)
                    .WithEase(ease)
                    .Bind(value => transform.localScale = value);
            })
            .Bind(value => transform.localScale = value);
    }

    private void StopMotion()
    {
        if (!scaleHandle.IsActive()) return;

        scaleHandle.Cancel();
        scaleHandle = default;
    }
}
