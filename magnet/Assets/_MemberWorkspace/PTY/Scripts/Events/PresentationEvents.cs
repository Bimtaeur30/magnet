using GameLib.EventChannelSystem;
using GameLib.ObjectPool.Runtime;
using UnityEngine;

namespace PTY.Scripts.Events
{
    /// <summary>
    /// SCRUM-26 연출 이벤트 구조 첫 항목. JTH 소유 MagnetGameEvents.cs는 수정하지 않고 별도 파일로 둔다
    /// (EventChannelSO는 Type 기준 라우팅이라 같은 magnetGameChannel에 raise 가능).
    /// </summary>
    public static class PresentationEvents
    {
        public static readonly PlayParticleEffectEvent PlayParticleEffectEvent = new();
    }

    public sealed class PlayParticleEffectEvent : GameEvent
    {
        public PoolItemSO Effect { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Texture Texture { get; private set; }

        /// <summary>texture가 null이면 프리팹/머티리얼에 이미 설정된 텍스처를 그대로 사용한다.</summary>
        public PlayParticleEffectEvent Init(PoolItemSO effect, Vector3 position, Quaternion rotation, Texture texture = null)
        {
            Effect = effect;
            Position = position;
            Rotation = rotation;
            Texture = texture;
            return this;
        }
    }
}
