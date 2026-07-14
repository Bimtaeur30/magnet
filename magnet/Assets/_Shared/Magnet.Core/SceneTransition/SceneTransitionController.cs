using Cysharp.Threading.Tasks;
using GameLib.EventChannelSystem;
using GameLib.SoundSystem;
using LitMotion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Shared.Magnet.Core.SceneTransition
{
    /// <summary>
    /// 부트스트랩 씬에 1개만 배치하는 씬 전환 리스너. SoundManager와 동일하게
    /// DI 컨테이너 없이 EventChannelSO만으로 동작하는 상주 오브젝트(DontDestroyOnLoad).
    /// 아래에서 올라와 화면을 덮으면 로드, 로드 완료 후 위로 빠져나가며 끝난다.
    /// </summary>
    public class SceneTransitionController : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private CanvasGroup inputBlockerCanvasGroup;
        [SerializeField] private RectTransform transitionPanel;
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
            inputBlockerCanvasGroup.blocksRaycasts = false;
            ResetPanelBelowScreen();
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
            inputBlockerCanvasGroup.blocksRaycasts = true;

            TransitionPresetSO preset = evt.Preset != null ? evt.Preset : defaultPreset;
            float duration = preset != null ? preset.duration : 0.5f;
            Ease ease = preset != null ? preset.ease : Ease.OutQuad;
            float travelDistance = transitionPanel.rect.height;

            ResetPanelBelowScreen();

            await UniTask.WhenAll(
                SlidePanelAsync(-travelDistance, 0f, duration, ease),
                FadeAudioAsync(1f, 0f, duration, ease));

            AsyncOperation op = SceneManager.LoadSceneAsync(evt.SceneDef.sceneName, LoadSceneMode.Single);
            if (op == null)
            {
                Debug.LogError($"[SceneTransitionController] '{evt.SceneDef.sceneName}' is not registered in Build Settings.", this);
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
                SlidePanelAsync(0f, travelDistance, duration, ease),
                FadeAudioAsync(0f, 1f, duration, ease));

            inputBlockerCanvasGroup.blocksRaycasts = false;
            _isTransitioning = false;

            magnetGameChannel.RaiseEvent(new SceneLoadCompletedEvent().Init(evt.SceneDef));
        }

        private void ResetPanelBelowScreen()
        {
            Vector2 position = transitionPanel.anchoredPosition;
            position.y = -transitionPanel.rect.height;
            transitionPanel.anchoredPosition = position;
        }

        private UniTask SlidePanelAsync(float from, float to, float duration, Ease ease)
        {
            return LMotion.Create(from, to, duration)
                .WithEase(ease)
                .Bind(y =>
                {
                    Vector2 position = transitionPanel.anchoredPosition;
                    position.y = y;
                    transitionPanel.anchoredPosition = position;
                })
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
