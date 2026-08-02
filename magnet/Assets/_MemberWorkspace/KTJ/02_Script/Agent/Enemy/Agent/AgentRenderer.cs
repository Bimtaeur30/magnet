using System.Collections;
using GGMLib.Anim;
using GGMLib.ModuleSystem;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AgentRenderer : MonoBehaviour, IModule, IAgentRenderer
{
    private static readonly int BlinkAmountId = Shader.PropertyToID("_BlinkAmount");
    private static readonly int PetrifyAmountId = Shader.PropertyToID("_PetrifyAmount");
    private static readonly int ShatterAmountId = Shader.PropertyToID("_ShatterAmount");
    private static readonly int DustAmountId = Shader.PropertyToID("_DustAmount");

    [SerializeField] private Material blinkMaterial;
    [SerializeField, Min(0.01f)] private float blinkDuration = 0.12f;
    [SerializeField, Min(1)] private int blinkCount = 2;
    [SerializeField, Min(0.01f)] private float petrifyDuration = 0.5f;
    [Header("Death Shatter")]
    [SerializeField, Min(4f)] private float fragmentPixelSize = 96f;
    [SerializeField, Min(0.01f)] private float fragmentDuration = 1.8f;
    [SerializeField, Min(0f)] private float burstForce = 1.87f;
    [SerializeField, Min(0f)] private float upwardForce = 1.47f;
    [SerializeField, Min(0f)] private float fragmentGravity = 2.5f;
    [SerializeField, Min(0f)] private float rotationSpeed = 360f;
    [SerializeField, Min(1f)] private float fragmentStartScale = 3f;

    private Animator animator;
    private SpriteRenderer[] _spriteRenderers;
    private MaterialPropertyBlock _propertyBlock;
    private Coroutine _blinkCoroutine;
    private Coroutine _deathEffectCoroutine;
    private GameObject _fragmentRoot;

    public bool IsDeathEffectFinished { get; private set; }

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
        SetDeathEffectAmounts(0f, 0f, 0f);
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

    public void PlayDeathEffect()
    {
        if (_deathEffectCoroutine != null)
            StopCoroutine(_deathEffectCoroutine);

        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        SetBlinkAmount(0f);
        SetDeathEffectAmounts(0f, 0f, 0f);
        IsDeathEffectFinished = false;
        _deathEffectCoroutine = StartCoroutine(DeathEffectRoutine());
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

    private IEnumerator DeathEffectRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < petrifyDuration)
        {
            elapsedTime += Time.deltaTime;
            float amount = Mathf.Clamp01(elapsedTime / petrifyDuration);
            SetDeathEffectAmounts(amount, 0f, 0f);
            yield return null;
        }

        SetDeathEffectAmounts(1f, 0f, 0f);
        CreateShatterFragments();

        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
        }

        yield return new WaitForSeconds(fragmentDuration);

        if (_fragmentRoot != null)
        {
            Destroy(_fragmentRoot);
            _fragmentRoot = null;
        }

        IsDeathEffectFinished = true;
        _deathEffectCoroutine = null;
    }

    private void OnDisable()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        if (_deathEffectCoroutine != null)
        {
            StopCoroutine(_deathEffectCoroutine);
            _deathEffectCoroutine = null;
        }

        if (_fragmentRoot != null)
        {
            Destroy(_fragmentRoot);
            _fragmentRoot = null;
        }

        if (_spriteRenderers != null)
        {
            foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
            {
                if (spriteRenderer != null)
                    spriteRenderer.enabled = true;
            }
        }

        SetBlinkAmount(0f);
        SetDeathEffectAmounts(0f, 0f, 0f);
        IsDeathEffectFinished = false;
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

    private void SetDeathEffectAmounts(
        float petrifyAmount,
        float shatterAmount,
        float dustAmount)
    {
        if (_spriteRenderers == null)
            return;

        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            if (spriteRenderer == null)
                continue;

            spriteRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(PetrifyAmountId, petrifyAmount);
            _propertyBlock.SetFloat(ShatterAmountId, shatterAmount);
            _propertyBlock.SetFloat(DustAmountId, dustAmount);
            spriteRenderer.SetPropertyBlock(_propertyBlock);
        }
    }

    private void CreateShatterFragments()
    {
        if (_fragmentRoot != null)
            Destroy(_fragmentRoot);

        _fragmentRoot = new GameObject($"{name}_ShatterFragments");

        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            if (spriteRenderer == null ||
                !spriteRenderer.enabled ||
                spriteRenderer.sprite == null)
            {
                continue;
            }

            CreateFragmentsForSprite(spriteRenderer);
        }
    }

    private void CreateFragmentsForSprite(SpriteRenderer spriteRenderer)
    {
        Sprite sprite = spriteRenderer.sprite;
        Rect textureRect = sprite.textureRect;
        float pixelsPerUnit = sprite.pixelsPerUnit;
        int columnCount = Mathf.CeilToInt(textureRect.width / fragmentPixelSize);
        int rowCount = Mathf.CeilToInt(textureRect.height / fragmentPixelSize);

        for (int y = 0; y < rowCount; y++)
        {
            for (int x = 0; x < columnCount; x++)
            {
                float pixelX = x * fragmentPixelSize;
                float pixelY = y * fragmentPixelSize;
                float width = Mathf.Min(fragmentPixelSize, textureRect.width - pixelX);
                float height = Mathf.Min(fragmentPixelSize, textureRect.height - pixelY);

                CreateFragment(
                    spriteRenderer,
                    sprite,
                    textureRect,
                    pixelX,
                    pixelY,
                    width,
                    height,
                    pixelsPerUnit);
            }
        }
    }

    private void CreateFragment(
        SpriteRenderer sourceRenderer,
        Sprite sprite,
        Rect textureRect,
        float pixelX,
        float pixelY,
        float width,
        float height,
        float pixelsPerUnit)
    {
        float localCenterX = (pixelX + width * 0.5f - sprite.pivot.x) / pixelsPerUnit;
        float localCenterY = (pixelY + height * 0.5f - sprite.pivot.y) / pixelsPerUnit;

        if (sourceRenderer.flipX)
            localCenterX = -localCenterX;
        if (sourceRenderer.flipY)
            localCenterY = -localCenterY;

        Vector3 worldPosition = sourceRenderer.transform.TransformPoint(
            new Vector3(localCenterX, localCenterY));

        var fragmentObject = new GameObject("ShatterFragment");
        fragmentObject.transform.SetParent(_fragmentRoot.transform, true);
        fragmentObject.transform.SetPositionAndRotation(
            worldPosition,
            sourceRenderer.transform.rotation);
        fragmentObject.transform.localScale = sourceRenderer.transform.lossyScale;

        Mesh mesh = CreateFragmentMesh(
            sprite,
            textureRect,
            width,
            height,
            pixelsPerUnit);

        MeshFilter meshFilter = fragmentObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = fragmentObject.AddComponent<MeshRenderer>();
        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = blinkMaterial;
        meshRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        meshRenderer.sortingOrder = sourceRenderer.sortingOrder;

        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetTexture("_MainTex", sprite.texture);
        propertyBlock.SetColor("_Color", sourceRenderer.color);
        propertyBlock.SetFloat(PetrifyAmountId, 0f);
        propertyBlock.SetFloat(ShatterAmountId, 0f);
        propertyBlock.SetFloat(DustAmountId, 0f);
        meshRenderer.SetPropertyBlock(propertyBlock);

        Vector3 outwardDirection = (worldPosition - transform.position).normalized;
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 velocity =
            outwardDirection * burstForce +
            new Vector3(randomDirection.x, randomDirection.y) * burstForce * 0.7f +
            Vector3.up * upwardForce * Random.Range(0.7f, 1.3f);

        EnemyShatterFragment fragment =
            fragmentObject.AddComponent<EnemyShatterFragment>();
        fragment.Initialize(
            velocity,
            Random.insideUnitSphere * rotationSpeed,
            fragmentGravity,
            fragmentDuration,
            fragmentStartScale);
    }

    private static Mesh CreateFragmentMesh(
        Sprite sprite,
        Rect textureRect,
        float width,
        float height,
        float pixelsPerUnit)
    {
        float halfWidth = width / pixelsPerUnit * 0.5f;
        float halfHeight = height / pixelsPerUnit * 0.5f;
        float textureWidth = sprite.texture.width;
        float textureHeight = sprite.texture.height;

        const int tileCount = 3;
        int randomTileX = Random.Range(0, tileCount);
        int randomTileY = Random.Range(0, tileCount);

        float samplePixelX =
            textureRect.x +
            (randomTileX + 0.5f) * (textureRect.width / tileCount);
        float samplePixelY =
            textureRect.y +
            (randomTileY + 0.5f) * (textureRect.height / tileCount);
        var sampledColorUV = new Vector2(
            samplePixelX / textureWidth,
            samplePixelY / textureHeight);

        var mesh = new Mesh
        {
            vertices = new[]
            {
                new Vector3(-halfWidth, -halfHeight),
                new Vector3(-halfWidth, halfHeight),
                new Vector3(halfWidth, halfHeight),
                new Vector3(halfWidth, -halfHeight)
            },
            uv = new[]
            {
                sampledColorUV,
                sampledColorUV,
                sampledColorUV,
                sampledColorUV
            },
            triangles = new[] { 0, 1, 2, 0, 2, 3 }
        };

        mesh.RecalculateBounds();
        return mesh;
    }
}
