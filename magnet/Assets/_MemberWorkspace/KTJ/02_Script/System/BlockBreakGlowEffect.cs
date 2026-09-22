using System;
using GameLib.EventChannelSystem;
using LitMotion;
using LitMotion.Extensions;
using Magnet.Core.Events;
using UnityEngine;

public class BlockBreakGlowEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer glowEffectSpriteRenderer;
    [SerializeField] private EventChannelSO magnetGameEventChannel;
    [SerializeField] private ParticleSystem glowEffectParticle;

    [Header("Combo Intensity")]
    [Tooltip("이 콤보부터 최대 세기로 고정하고 무지개 색으로 바꾼다")]
    [SerializeField] private int maxCombo = 7;
    [Tooltip("콤보 0~maxCombo 진행도(0~1)를 세기(0~1)로 바꾸는 곡선")]
    [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("파티클 개수 배율 (콤보 0 → maxCombo)")]
    [SerializeField] private Vector2 countScaleRange = new(0.15f, 1.5f);
    [Tooltip("파티클 속도 배율 (콤보 0 → maxCombo)")]
    [SerializeField] private Vector2 speedScaleRange = new(0.3f, 1.3f);
    [Tooltip("파티클 크기 배율 (콤보 0 → maxCombo)")]
    [SerializeField] private Vector2 sizeScaleRange = new(0.6f, 1.2f);
    [SerializeField] private Gradient rainbowGradient = CreateRainbowGradient();

    private ParticleSystem.MinMaxCurve _baseSpeed;
    private ParticleSystem.MinMaxCurve _baseSize;
    private ParticleSystem.MinMaxGradient _baseColor;
    private Vector2Int _baseBurstCount;

    // BlockClearedEvent가 칸마다, ComboChangedEvent보다 먼저 오므로 프레임 끝에 한 번만 재생한다.
    private bool _clearedThisFrame;
    private int _comboThisFrame;

    private void Awake()
    {
        CacheBaseSettings();
        magnetGameEventChannel.AddListener<BlockClearedEvent>(HandleBlockClearedEvent);
        magnetGameEventChannel.AddListener<ComboChangedEvent>(HandleComboChangedEvent);
    }

    private void OnDisable()
    {
        magnetGameEventChannel.RemoveListener<BlockClearedEvent>(HandleBlockClearedEvent);
        magnetGameEventChannel.RemoveListener<ComboChangedEvent>(HandleComboChangedEvent);
    }

    private void CacheBaseSettings()
    {
        ParticleSystem.MainModule main = glowEffectParticle.main;
        _baseSpeed = main.startSpeed;
        _baseSize = main.startSize;
        _baseColor = main.startColor;

        ParticleSystem.EmissionModule emission = glowEffectParticle.emission;
        if (emission.burstCount > 0)
        {
            ParticleSystem.MinMaxCurve count = emission.GetBurst(0).count;
            _baseBurstCount = new Vector2Int((int)count.constantMin, (int)count.constantMax);
            if (count.mode == ParticleSystemCurveMode.Constant)
            {
                _baseBurstCount = new Vector2Int((int)count.constant, (int)count.constant);
            }
        }
        else
        {
            _baseBurstCount = new Vector2Int(30, 50);
        }

        // 버스트는 Emit으로 직접 쏘므로 Play 시 자동 방출은 끈다.
        emission.enabled = false;
    }

    private void HandleBlockClearedEvent(BlockClearedEvent obj)
    {
        _clearedThisFrame = true;
    }

    private void HandleComboChangedEvent(ComboChangedEvent obj)
    {
        _comboThisFrame = obj.Combo;
    }

    private void LateUpdate()
    {
        if (!_clearedThisFrame)
        {
            _comboThisFrame = 0;
            return;
        }

        PlayEffect(_comboThisFrame);
        _clearedThisFrame = false;
        _comboThisFrame = 0;
    }

    private void PlayEffect(int combo)
    {
        LMotion.Create(0f, 1f, 0.5f).WithLoops(2, LoopType.Yoyo).Bind(alpha =>
        {
            Color color = glowEffectSpriteRenderer.color;
            color.a = alpha;
            glowEffectSpriteRenderer.color = color;
        }).AddTo(gameObject);

        bool isMax = combo >= maxCombo;
        float progress = maxCombo <= 0 ? 1f : Mathf.Clamp01((float)combo / maxCombo);
        float intensity = Mathf.Clamp01(intensityCurve.Evaluate(progress));

        ParticleSystem.MainModule main = glowEffectParticle.main;
        main.startSpeed = Scale(_baseSpeed, Mathf.Lerp(speedScaleRange.x, speedScaleRange.y, intensity));
        main.startSize = Scale(_baseSize, Mathf.Lerp(sizeScaleRange.x, sizeScaleRange.y, intensity));
        main.startColor = isMax
            ? new ParticleSystem.MinMaxGradient(rainbowGradient) { mode = ParticleSystemGradientMode.RandomColor }
            : _baseColor;

        float countScale = Mathf.Lerp(countScaleRange.x, countScaleRange.y, intensity);
        int baseCount = UnityEngine.Random.Range(_baseBurstCount.x, _baseBurstCount.y + 1);
        int count = Mathf.Max(1, Mathf.RoundToInt(baseCount * countScale));

        if (!glowEffectParticle.isPlaying)
        {
            glowEffectParticle.Play();
        }

        glowEffectParticle.Emit(count);
    }

    private static ParticleSystem.MinMaxCurve Scale(ParticleSystem.MinMaxCurve curve, float scale)
    {
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.Constant:
                curve.constant *= scale;
                break;
            case ParticleSystemCurveMode.TwoConstants:
                curve.constantMin *= scale;
                curve.constantMax *= scale;
                break;
            default:
                curve.curveMultiplier *= scale;
                break;
        }

        return curve;
    }

    private static Gradient CreateRainbowGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.2f, 0.2f), 0f),
                new GradientColorKey(new Color(1f, 0.6f, 0.1f), 0.17f),
                new GradientColorKey(new Color(1f, 0.95f, 0.2f), 0.33f),
                new GradientColorKey(new Color(0.3f, 1f, 0.3f), 0.5f),
                new GradientColorKey(new Color(0.2f, 0.8f, 1f), 0.67f),
                new GradientColorKey(new Color(0.3f, 0.4f, 1f), 0.83f),
                new GradientColorKey(new Color(0.8f, 0.3f, 1f), 1f),
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f),
            });
        return gradient;
    }
}
