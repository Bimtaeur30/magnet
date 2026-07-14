using GameLib.EventChannelSystem;
using GameLib.SoundSystem;

namespace _Shared.Magnet.Core.SceneTransition
{
    public static class SceneTransitionEvents
    {
        public static readonly LoadSceneEvent LoadSceneEvent = new();
        public static readonly SceneLoadCompletedEvent SceneLoadCompletedEvent = new();
    }

    public sealed class LoadSceneEvent : GameEvent
    {
        public SceneDefSO SceneDef { get; private set; }
        public TransitionPresetSO Preset { get; private set; }
        public SoundClipSO BgmClip { get; private set; }

        public LoadSceneEvent Init(SceneDefSO sceneDef, TransitionPresetSO preset = null, SoundClipSO bgmClip = null)
        {
            SceneDef = sceneDef;
            Preset = preset;
            BgmClip = bgmClip;
            return this;
        }
    }

    public sealed class SceneLoadCompletedEvent : GameEvent
    {
        public SceneDefSO SceneDef { get; private set; }

        public SceneLoadCompletedEvent Init(SceneDefSO sceneDef)
        {
            SceneDef = sceneDef;
            return this;
        }
    }
}
