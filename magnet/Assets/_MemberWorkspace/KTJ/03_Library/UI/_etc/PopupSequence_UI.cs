using System.Collections;
using UnityEngine;

public class PopupSequence_UI : MonoBehaviour
{
    [Header("등장 순서")]
    [SerializeField] private GameObject[] uiObjects;

    [Header("애니메이션")]
    [Min(0f)]
    [SerializeField] private float duration = 0.3f;
    [Min(0f)]
    [SerializeField] private float interval = 0.08f;
    [SerializeField] private AnimationCurve scaleCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 4f),
        new Keyframe(0.7f, 1.08f, 0f, 0f),
        new Keyframe(1f, 1f, -0.3f, 0f));

    private Vector3[] originalScales;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        CacheOriginalScales();
    }

    private void OnEnable()
    {
        InitializeScales();
        PlayPopupAnimation();
    }

    private void OnDisable()
    {
        StopAnimation();
        RestoreOriginalScales();
    }

    private void OnValidate()
    {
        duration = Mathf.Max(0f, duration);
        interval = Mathf.Max(0f, interval);
    }

    private void PlayPopupAnimation()
    {
        StopAnimation();

        if (originalScales == null || originalScales.Length != uiObjects.Length)
        {
            CacheOriginalScales();
            InitializeScales();
        }

        animationCoroutine = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        if (duration <= 0f)
        {
            RestoreOriginalScales();
            animationCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        float totalDuration = duration + interval * Mathf.Max(0, uiObjects.Length - 1);

        while (elapsed < totalDuration)
        {
            for (int i = 0; i < uiObjects.Length; i++)
            {
                if (uiObjects[i] == null)
                {
                    continue;
                }

                float startTime = interval * i;
                float progress = Mathf.Clamp01((elapsed - startTime) / duration);
                float scale = scaleCurve.Evaluate(progress);
                uiObjects[i].transform.localScale = originalScales[i] * scale;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        RestoreOriginalScales();
        animationCoroutine = null;
    }

    private void CacheOriginalScales()
    {
        originalScales = new Vector3[uiObjects.Length];

        for (int i = 0; i < uiObjects.Length; i++)
        {
            originalScales[i] = uiObjects[i] != null
                ? uiObjects[i].transform.localScale
                : Vector3.one;
        }
    }

    private void InitializeScales()
    {
        for (int i = 0; i < uiObjects.Length; i++)
        {
            if (uiObjects[i] != null)
            {
                uiObjects[i].transform.localScale = Vector3.zero;
            }
        }
    }

    private void RestoreOriginalScales()
    {
        if (originalScales == null)
        {
            return;
        }

        int count = Mathf.Min(uiObjects.Length, originalScales.Length);
        for (int i = 0; i < count; i++)
        {
            if (uiObjects[i] != null)
            {
                uiObjects[i].transform.localScale = originalScales[i];
            }
        }
    }

    private void StopAnimation()
    {
        if (animationCoroutine == null)
        {
            return;
        }

        StopCoroutine(animationCoroutine);
        animationCoroutine = null;
    }
}
