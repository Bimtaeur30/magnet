using GameLib.EventChannelSystem;
using GameLib.SoundSystem;

namespace _Shared.Magnet.Core.SceneTransition
{
    public static class SceneTransitionEvents
    {
        public static readonly LoadSceneEvent LoadSceneEvent = new();
    }

    public sealed class LoadSceneEvent : GameEvent
    {
        public string SceneName { get; private set; }
        public TransitionPresetSO Preset { get; private set; }
        public SoundClipSO BgmClip { get; private set; }

        public LoadSceneEvent Init(string sceneName, TransitionPresetSO preset = null, SoundClipSO bgmClip = null)
        {
            SceneName = sceneName;
            Preset = preset;
            BgmClip = bgmClip;
            return this;
        }
    }
}
