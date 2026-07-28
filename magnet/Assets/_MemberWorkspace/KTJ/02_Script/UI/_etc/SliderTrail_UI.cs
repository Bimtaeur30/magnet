using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public sealed class SliderTrail_UI : MonoBehaviour
{
    [SerializeField] private Slider targetSlider;
    [SerializeField, Min(0f)] private float followSpeed = 1f;

    private Slider _trailSlider;
    private float _targetValue;

    private void Awake()
    {
        _trailSlider = GetComponent<Slider>();
    }

    private void Start()
    {
        if (targetSlider == null)
        {
            Debug.LogError("SliderTrail의 Target Slider를 할당하세요.", this);
            enabled = false;
            return;
        }

        _trailSlider.minValue = targetSlider.minValue;
        _trailSlider.maxValue = targetSlider.maxValue;
        _trailSlider.wholeNumbers = targetSlider.wholeNumbers;
        _trailSlider.value = targetSlider.value;
        _targetValue = targetSlider.value;

        targetSlider.onValueChanged.AddListener(OnTargetValueChanged);
    }

    private void OnDisable()
    {
        if (targetSlider != null)
            targetSlider.onValueChanged.RemoveListener(OnTargetValueChanged);
    }

    private void Update()
    {
        _trailSlider.value = Mathf.MoveTowards(
            _trailSlider.value,
            _targetValue,
            followSpeed * Time.deltaTime);
    }

    private void OnTargetValueChanged(float value)
    {
        _targetValue = value;
    }
}
