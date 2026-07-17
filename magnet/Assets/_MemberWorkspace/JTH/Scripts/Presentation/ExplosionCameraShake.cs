using Cinemachine;
using JTH.Scripts.Data;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 클리어 폭발 시 Cinemachine Impulse로 좌우 감쇠 카메라 쉐이크.
    /// Main Camera에 <see cref="CinemachineIndependentImpulseListener"/>를 붙인다.
    /// </summary>
    public static class ExplosionCameraShake
    {
        private static CinemachineImpulseSource _source;
        private static AnimationCurve _leftRightDecayCurve;

        public static void Play(ExplosionBorderConfigSO config)
        {
            if (config == null || config.ShakeAmplitude <= 0f || config.ShakeDuration <= 0f)
            {
                return;
            }

            if (!EnsureListener())
            {
                return;
            }

            EnsureSource(config);
            _source.GenerateImpulseWithVelocity(Vector3.right * config.ShakeAmplitude);
        }

        private static bool EnsureListener()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return false;
            }

            CinemachineIndependentImpulseListener listener =
                camera.GetComponent<CinemachineIndependentImpulseListener>();
            if (listener == null)
            {
                listener = camera.gameObject.AddComponent<CinemachineIndependentImpulseListener>();
            }

            listener.m_ChannelMask = 1;
            listener.m_Gain = 1f;
            listener.m_Use2DDistance = true;
            listener.m_UseLocalSpace = true;
            // 1차 Impulse(좌우 감쇠)만 사용. 2차 Noise 반응 끔.
            listener.m_ReactionSettings = new CinemachineImpulseListener.ImpulseReaction
            {
                m_AmplitudeGain = 0f,
                m_FrequencyGain = 1f,
                m_Duration = 0f,
            };

            return true;
        }

        private static void EnsureSource(ExplosionBorderConfigSO config)
        {
            if (_source == null)
            {
                var go = new GameObject("ExplosionCameraImpulseSource");
                Object.DontDestroyOnLoad(go);
                _source = go.AddComponent<CinemachineImpulseSource>();
            }

            CinemachineImpulseDefinition definition = _source.m_ImpulseDefinition;
            definition.m_ImpulseChannel = 1;
            definition.m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
            definition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Custom;
            definition.m_CustomImpulseShape = GetLeftRightDecayCurve();
            definition.m_ImpulseDuration = config.ShakeDuration;
            definition.m_DissipationDistance = 100f;
            definition.m_DissipationRate = 0.25f;
            _source.m_DefaultVelocity = Vector3.right;
        }

        /// <summary>
        /// +X → −X → +X … 진폭이 줄어드는 커스텀 Impulse 곡선.
        /// </summary>
        private static AnimationCurve GetLeftRightDecayCurve()
        {
            if (_leftRightDecayCurve != null)
            {
                return _leftRightDecayCurve;
            }

            _leftRightDecayCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.12f, 1f),
                new Keyframe(0.28f, -0.7f),
                new Keyframe(0.44f, 0.4f),
                new Keyframe(0.6f, -0.2f),
                new Keyframe(0.76f, 0.08f),
                new Keyframe(1f, 0f));
            return _leftRightDecayCurve;
        }
    }
}
