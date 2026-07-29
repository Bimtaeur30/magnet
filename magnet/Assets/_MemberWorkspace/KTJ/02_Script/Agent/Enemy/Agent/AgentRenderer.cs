using System.Collections;
using GGMLib.Anim;
using GGMLib.ModuleSystem;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AgentRenderer : MonoBehaviour, IModule, IAgentRenderer
{
    private static readonly int BlinkAmountId = Shader.PropertyToID("_BlinkAmount");

    [SerializeField] private Material blinkMaterial;
    [SerializeField, Min(0.01f)] private float blinkDuration = 0.12f;
    [SerializeField, Min(1)] private int blinkCount = 2;

    private Animator animator;
    private SpriteRenderer[] _spriteRenderers;
    private MaterialPropertyBlock _propertyBlock;
    private Coroutine _blinkCoroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        _propertyBlock = new MaterialPropertyBlock();

        if (blinkMaterial != null)
        {
            foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
                spriteRenderer.sharedMaterial = blinkMaterial;
        }

        SetBlinkAmount(0f);
    }

    public void Initialize(ModuleOwner owner) { }

    public void PlayAnimation(AnimationParamSO param)
    {
        animator.Play(param.Hash, 0, 0f);
        animator.Update(0f);
    }

    public bool IsAnimationFinished(AnimationParamSO param)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isTargetState = stateInfo.shortNameHash == param.Hash || stateInfo.fullPathHash == param.Hash;

        return isTargetState && !animator.IsInTransition(0) && stateInfo.normalizedTime >= 1f;
    }

    public void PlayBlink()
    {
        if (_blinkCoroutine != null)
            StopCoroutine(_blinkCoroutine);

        _blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            float elapsedTime = 0f;

            while (elapsedTime < blinkDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / blinkDuration);
                SetBlinkAmount(1f - normalizedTime);
                yield return null;
            }
        }

        SetBlinkAmount(0f);
        _blinkCoroutine = null;
    }

    private void OnDisable()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        SetBlinkAmount(0f);
    }

    private void SetBlinkAmount(float amount)
    {
        if (_spriteRenderers == null)
            return;

        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            if (spriteRenderer == null)
                continue;

            spriteRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(BlinkAmountId, amount);
            spriteRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
