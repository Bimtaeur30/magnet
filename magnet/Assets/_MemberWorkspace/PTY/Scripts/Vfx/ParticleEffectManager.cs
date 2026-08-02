using _Shared.Magnet.Core.Events;
using GameLib.EventChannelSystem;
using GameLib.ObjectPool.Runtime;
using UnityEngine;

namespace PTY.Scripts.Vfx
{
    /// <summary>
    /// SCRUM-26 연출 이벤트 구조. magnetGameChannel/PresentationChannel의 PlayParticleEffectEvent를 구독해
    /// GameLib.ObjectPool(PoolManagerSO)에서 파티클을 꺼내 재생하고, 종료 시 자동으로 풀에 반환한다.
    /// </summary>
    public sealed class ParticleEffectManager : MonoBehaviour
    {
        [SerializeField] private EventChannelSO presentationChannel;
        [SerializeField] private PoolManagerSO particlePool;

        private void Awake()
        {
            Debug.Assert(presentationChannel != null, "[ParticleEffectManager] EventChannelSO is not assigned.", this);
            Debug.Assert(particlePool != null, "[ParticleEffectManager] PoolManagerSO is not assigned.", this);
        }

        private void OnEnable()
        {
            presentationChannel.AddListener<PlayParticleEffectEvent>(HandlePlayParticleEffect);
        }

        private void OnDisable()
        {
            presentationChannel.RemoveListener<PlayParticleEffectEvent>(HandlePlayParticleEffect);
        }

        private void HandlePlayParticleEffect(PlayParticleEffectEvent evt)
        {
            var pooled = particlePool.Pop<PooledParticleEffect>(evt.Effect);
            if (pooled == null)
            {
                Debug.LogWarning($"[ParticleEffectManager] No pooled instance for {evt.Effect.name}.");
                return;
            }

            pooled.transform.SetPositionAndRotation(evt.Position, evt.Rotation);

            if (evt.Texture != null)
            {
                pooled.SetTexture(evt.Texture);
            }

            pooled.OnEffectFinished += HandleEffectFinished;
            pooled.Play();
        }

        private void HandleEffectFinished(PooledParticleEffect pooled)
        {
            pooled.OnEffectFinished -= HandleEffectFinished;
            particlePool.Push(pooled);
        }
    }
}
