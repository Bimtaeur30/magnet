using System;
using GameLib.EventChannelSystem;
using LitMotion;
using LitMotion.Extensions;
using Magnet.Core.Events;
using UnityEngine;

public class BlockBreakGlowEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer glowEffectSpriteRenderer;
    [SerializeField] private EventChannelSO inGameEventChannelSO;
    [SerializeField] private ParticleSystem glowEffectParticle;

    private void Awake()
    {
        inGameEventChannelSO.AddListener<BlockDestroyedEvent>(HandleBlockDestroyedEvent);
    }
    private void OnDisable()
    {
        inGameEventChannelSO.RemoveListener<BlockDestroyedEvent>(HandleBlockDestroyedEvent);
    }

    private void HandleBlockDestroyedEvent(BlockDestroyedEvent obj)
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
