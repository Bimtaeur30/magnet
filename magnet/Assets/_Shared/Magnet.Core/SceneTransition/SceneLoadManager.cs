using GameLib.EventChannelSystem;
using GameLib.SoundSystem;
using System;
using UnityEngine;

namespace _Shared.Magnet.Core.SceneTransition
{
    /// <summary>
    /// 씬 전환 요청의 진입점. 호출부는 SceneDefSO만 알면 되고, 문자열/이벤트 발행 세부는 여기서 감춘다.
    /// SceneTransitionController와 같은 상주 오브젝트에 배치한다.
    /// </summary>
    public class SceneLoadManager : MonoBehaviour
    {
        [SerializeField] private EventChannelSO SceneTransitionChannel;
        [SerializeField] private TransitionPresetSO defaultPreset;

        public SceneDefSO CurrentScene { get; private set; }

        private void Awake()
        {
            Debug.Assert(SceneTransitionChannel != null, "[SceneLoadManager] EventChannelSO is not assigned.", this);
            SceneTransitionChannel.AddListener<SceneLoadCompletedEvent>(HandleSceneLoadCompleted);
        }

        private void OnDestroy()
        {
            SceneTransitionChannel.RemoveListener<SceneLoadCompletedEvent>(HandleSceneLoadCompleted);
        }

        private void HandleLoadScene(LoadSceneEvent @event)
        {
            if (@event.SceneDef == null)
            {
                Debug.LogError("[SceneLoadManager] SceneDefSO is null.", this);
                return;
            }

            SceneTransitionChannel.RaiseEvent(new LoadSceneEvent().Init(@event.SceneDef, @event.Preset != null ? @event.Preset : defaultPreset, @event.BgmClip));
        }

        private void HandleSceneLoadCompleted(SceneLoadCompletedEvent evt)
        {
            CurrentScene = evt.SceneDef;
        }
    }
}
