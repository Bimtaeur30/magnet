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

    private void Awake()
    {
        magnetGameEventChannel.AddListener<BlockClearedEvent >(HandleBlockClearedEvent);
    }
    private void OnDisable()
    {
        magnetGameEventChannel.RemoveListener<BlockClearedEvent >(HandleBlockClearedEvent);
    }

    private void HandleBlockClearedEvent(BlockClearedEvent  obj)
    {
        LMotion.Create(0f, 1f, 0.5f).WithLoops(2, LoopType.Yoyo).Bind(alpha =>
        {
            Color color = glowEffectSpriteRenderer.color;
            color.a = alpha;
            glowEffectSpriteRenderer.color = color;
        }).AddTo(gameObject);
        
        glowEffectParticle.Play();
    }
}
