using Magnet.Contracts;
using GameLib.EventChannelSystem;
using GameLib.SoundSystem;
using JTH.Scripts.Data;
using Magnet.Core.Events;
using Magnet.Core.Settings;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    /// <summary>
    /// 콤보 상승 / 대량 라인클리어 피드백(사운드 전환 + 진동) 담당.
    /// 배치·클리어 기본음(BoardPlacementBootstrap)과는 별개로 위에 얹는 추가 연출 레이어다.
    /// 실제 사운드 클립·티어 값은 GameFeedbackConfigSO 에셋에서
    /// 사운드 담당이 채우면 됨 (코드 수정 불필요).
    /// </summary>
    public sealed class GameFeedbackBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private EventChannelSO soundChannel;
        [SerializeField] private GameFeedbackConfigSO config;

        // 콤보가 새 이벤트 없이 끊길 수 있어 GameOverEvent에서만 확실히 리셋한다.
        private int _lastComboTierIndex = -1;

        private void Awake()
        {
            Debug.Assert(inGameChannel != null, "[GameFeedbackBootstrap] inGameChannel is not assigned.", this);
            Debug.Assert(magnetGameChannel != null, "[GameFeedbackBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(soundChannel != null, "[GameFeedbackBootstrap] soundChannel is not assigned.", this);
            Debug.Assert(config != null, "[GameFeedbackBootstrap] config is not assigned.", this);
        }

        private void OnEnable()
        {
            magnetGameChannel.AddListener<ComboChangedEvent>(OnComboChanged);
            magnetGameChannel.AddListener<AllClearEvent>(OnAllClear);
            magnetGameChannel.AddListener<GameOverEvent>(OnGameOver);
            inGameChannel.AddListener<BlockPlacedEvent>(OnBlockPlaced);
        }

        private void OnDisable()
        {
            magnetGameChannel?.RemoveListener<ComboChangedEvent>(OnComboChanged);
            magnetGameChannel?.RemoveListener<AllClearEvent>(OnAllClear);
            magnetGameChannel?.RemoveListener<GameOverEvent>(OnGameOver);
            inGameChannel?.RemoveListener<BlockPlacedEvent>(OnBlockPlaced);
        }

        // ComboChangedEvent는 콤보가 0이 아닌 값으로 바뀔 때만 오므로 사운드는 매번 재생한다.
        private void OnComboChanged(ComboChangedEvent evt)
        {
            int tierIndex = ResolveComboTierIndex(evt.Combo);
            if (tierIndex < 0)
            {
                return;
            }

            PlaySound(config.ComboTiers[tierIndex].Sound, evt.WorldPosition);

            if (tierIndex <= _lastComboTierIndex)
            {
                return;
            }

            Vibrate(config.ComboTierHaptics);
            _lastComboTierIndex = tierIndex;
        }

        private void OnAllClear(AllClearEvent evt)
        {
            PlaySound(config.AllClearSound, Vector3.zero);
        }

        private void OnBlockPlaced(BlockPlacedEvent evt)
        {
            int clearedLineCount = evt.PlacementResult.ClearedLineResult.ClearedLineCount;

            if (config.PlacementHaptics)
            {
                VibrationSettings.VibratePlacement();
            }

            if (clearedLineCount <= 0)
            {
                return;
            }

            if (config.LineClearHaptics)
            {
                Vibrate(true);
            }

            if (clearedLineCount >= config.BigClearLineThreshold)
            {
                PlaySound(config.BigClearSound, Vector3.zero);
                Vibrate(config.BigClearHaptics);
            }
        }

        private void OnGameOver(GameOverEvent evt)
        {
            _lastComboTierIndex = -1;
        }

        private int ResolveComboTierIndex(int combo)
        {
            for (int i = config.ComboTiers.Count - 1; i >= 0; i--)
            {
                if (combo >= config.ComboTiers[i].ComboThreshold)
                {
                    return i;
                }
            }

            return -1;
        }

        private void PlaySound(SoundClipSO clip, Vector3 position)
        {
            if (soundChannel == null || clip == null)
            {
                return;
            }

            soundChannel.RaiseEvent(SoundSystemEvents.PlaySoundEvent.Init(position, clip));
        }

        /// <summary>
        /// 세기 구분 없는 단발 진동. 강도 차등이 필요하면 별도 네이티브 진동 플러그인 도입이 필요하다(범위 밖).
        /// 설정 메뉴의 진동 on/off는 VibrationSettings가 처리한다.
        /// </summary>
        private void Vibrate(bool enabled)
        {
            if (!enabled)
            {
                return;
            }

            Debug.Log("[GameFeedbackBootstrap] Vibrate called");
            VibrationSettings.Vibrate();
        }
    }
}
