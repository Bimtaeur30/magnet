using GameLib.EventChannelSystem;
using GameLib.SoundSystem;
using UnityEngine;

namespace _Shared.Magnet.Core.SceneTransition
{
    /// <summary>
    /// 씬 전환 요청의 진입점. 호출부는 SceneDefSO만 알면 되고, 문자열/이벤트 발행 세부는 여기서 감춘다.
    /// SceneTransitionController와 같은 상주 오브젝트에 배치한다.
    /// </summary>
    public class SceneLoadManager : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private TransitionPresetSO defaultPreset;

        public SceneDefSO CurrentScene { get; private set; }

        private void Awake()
        {
            Debug.Assert(magnetGameChannel != null, "[SceneLoadManager] EventChannelSO is not assigned.", this);
            magnetGameChannel.AddListener<SceneLoadCompletedEvent>(HandleSceneLoadCompleted);
        }

        private void OnDestroy()
        {
            magnetGameChannel.RemoveListener<SceneLoadCompletedEvent>(HandleSceneLoadCompleted);
        }

        public void LoadScene(SceneDefSO sceneDef, SoundClipSO bgmClip = null, TransitionPresetSO preset = null)
        {
            if (sceneDef == null)
            {
                Debug.LogError("[SceneLoadManager] SceneDefSO is null.", this);
                return;
            }

            magnetGameChannel.RaiseEvent(new LoadSceneEvent().Init(sceneDef, preset != null ? preset : defaultPreset, bgmClip));
        }

        private void HandleSceneLoadCompleted(SceneLoadCompletedEvent evt)
        {
            CurrentScene = evt.SceneDef;
        }
    }
}
