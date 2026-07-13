using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameLib.ObjectPool.Runtime;
using UnityEngine;

namespace PTY.Scripts.Vfx
{
    /// <summary>
    /// GameLib.ObjectPool(IPoolable)을 구현하는 파티클 전용 풀 아이템.
    /// SoundPlayer(Assets/_Shared/Sound)와 동일한 패턴: 재생 시작 시 CancellationTokenSource를 새로 만들고,
    /// 파티클이 끝나면 OnEffectFinished를 발행해 소유자(ParticleEffectManager)가 풀에 반환하게 한다.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class PooledParticleEffect : AbstractMonoPoolable
    {
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

        [SerializeField] private ParticleSystem rootParticleSystem;
        [SerializeField] private ParticleSystemRenderer particleRenderer;

        private MaterialPropertyBlock propertyBlock;
        private CancellationTokenSource cts;

        public event Action<PooledParticleEffect> OnEffectFinished;

        private void Reset()
        {
            rootParticleSystem = GetComponent<ParticleSystem>();
            particleRenderer = GetComponent<ParticleSystemRenderer>();
        }

        /// <summary>
        /// 텍스처 시트 애니메이션(랜덤 타일)이 이 텍스처의 랜덤한 부분을 표시하도록 스킨 텍스처를 지정한다.
        /// 아틀라스에 패킹된 Sprite는 지원하지 않는다 — 독립된 텍스처를 전달해야 한다.
        /// </summary>
        public void SetTexture(Texture texture)
        {
            propertyBlock ??= new MaterialPropertyBlock();

            particleRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture(MainTexId, texture);
            propertyBlock.SetTexture(BaseMapId, texture);
            particleRenderer.SetPropertyBlock(propertyBlock);
        }

        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        public void Play()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();

            rootParticleSystem.Play(true);
            WatchForCompletionAsync(cts.Token).Forget();
        }

        public override void ResetItem()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            OnEffectFinished = null;

            rootParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            transform.SetParent(null);
        }

        private async UniTaskVoid WatchForCompletionAsync(CancellationToken token)
        {
            try
            {
                await UniTask.WaitUntil(() => !rootParticleSystem.IsAlive(true), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            OnEffectFinished?.Invoke(this);
        }
    }
}
