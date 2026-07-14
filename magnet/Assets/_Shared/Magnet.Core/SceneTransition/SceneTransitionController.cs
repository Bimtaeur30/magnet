using Cysharp.Threading.Tasks;
using GameLib.EventChannelSystem;
using GameLib.SoundSystem;
using LitMotion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Shared.Magnet.Core.SceneTransition
{
    /// <summary>
    /// 부트스트랩 씬에 1개만 배치하는 씬 전환 리스너. SoundManager와 동일하게
    /// DI 컨테이너 없이 EventChannelSO만으로 동작하는 상주 오브젝트(DontDestroyOnLoad).
    /// </summary>
    public class SceneTransitionController : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private TransitionPresetSO defaultPreset;

        private bool _isTransitioning;

        private void Awake()
        {
            Debug.Assert(magnetGameChannel != null, "[SceneTransitionController] EventChannelSO is not assigned.", this);

            SceneTransitionController[] existing = FindObjectsByType<SceneTransitionController>(FindObjectsSortMode.None);
            if (existing.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            magnetGameChannel.AddListener<LoadSceneEvent>(HandleLoadScene);
        }

        private void OnDestroy()
        {
            magnetGameChannel.RemoveListener<LoadSceneEvent>(HandleLoadScene);
        }

        private void HandleLoadScene(LoadSceneEvent evt)
        {
            if (_isTransitioning) return;
            RunTransitionAsync(evt).Forget();
        }

        private async UniTaskVoid RunTransitionAsync(LoadSceneEvent evt)
        {
            _isTransitioning = true;
            fadeCanvasGroup.blocksRaycasts = true;

            TransitionPresetSO preset = evt.Preset != null ? evt.Preset : defaultPreset;
            float fadeDuration = preset != null ? preset.fadeDuration : 0.5f;
            Ease ease = preset != null ? preset.ease : Ease.OutQuad;

            await UniTask.WhenAll(
                FadeCanvasAsync(0f, 1f, fadeDuration, ease),
                FadeAudioAsync(1f, 0f, fadeDuration, ease));

            AsyncOperation op = SceneManager.LoadSceneAsync(evt.SceneName, LoadSceneMode.Single);
            if (op == null)
            {
                Debug.LogError($"[SceneTransitionController] '{evt.SceneName}' is not registered in Build Settings.", this);
            }
            else
            {
                await op.ToUniTask();
            }

            if (evt.BgmClip != null)
            {
                magnetGameChannel.RaiseEvent(SoundSystemEvents.PlaySoundEvent.Init(Vector3.zero, evt.BgmClip, evt.BgmClip));
            }

            await UniTask.WhenAll(
                FadeCanvasAsync(1f, 0f, fadeDuration, ease),
                FadeAudioAsync(0f, 1f, fadeDuration, ease));

            fadeCanvasGroup.blocksRaycasts = false;
            _isTransitioning = false;
        }

        private UniTask FadeCanvasAsync(float from, float to, float duration, Ease ease)
        {
            return LMotion.Create(from, to, duration)
                .WithEase(ease)
                .Bind(value => fadeCanvasGroup.alpha = value)
                .ToUniTask();
        }

        private UniTask FadeAudioAsync(float from, float to, float duration, Ease ease)
        {
            return LMotion.Create(from, to, duration)
                .WithEase(ease)
                .Bind(value => AudioListener.volume = value)
                .ToUniTask();
        }
    }
}
