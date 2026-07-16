using _Shared.Magnet.Core.SceneTransition;
using GameLib.EventChannelSystem;
using GameLib.SoundSystem;
using JTH.Scripts.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PMS.Scripts.Manager
{
    public class SettingsFuncManager : MonoBehaviour
    {
        [Header("SceneMove")]
        [SerializeField] private EventChannelSO SceneTransitionChannel;
        [SerializeField] private SceneDefSO MainTItleScene;
        [SerializeField] private SceneDefSO InGameScene;
        [Header("Sound")]
        [SerializeField] private EventChannelSO magnetGameChannel;

        [Header("Scene")]
        [SerializeField] private string titleSceneName = "TitleScene";

        private bool isVibrationOn = true;
        private bool isBgmOn = true;
        private bool isSfxOn = true;

        // 진동 토글 (UI에 연결할.)
        public void ToggleVibration()
        {
            isVibrationOn = !isVibrationOn;
        }

        // BGM 토글 (UI에 연결할.)
        public void ToggleBgm()
        {
            isBgmOn = !isBgmOn;
            SetBgmVolume(isBgmOn ? 1f : 0f);
        }

        // Sfx 토글 (UI에 연결할.)
        public void ToggleSfx()
        {
            isSfxOn = !isSfxOn;
            SetSfxVolume(isSfxOn ? 1f : 0f);
        }

        // Master 볼륨 슬라이더 (UI에 연결할. 0~1)
        public void SetMasterVolume(float volume01)
        {
            magnetGameChannel.RaiseEvent(SoundSystemEvents.SetVolumeEvent.Init(AudioBus.Master, volume01));
        }

        // BGM 볼륨 슬라이더 (UI에 연결할. 0~1)
        public void SetBgmVolume(float volume01)
        {
            isBgmOn = volume01 > 0f;
            magnetGameChannel.RaiseEvent(SoundSystemEvents.SetVolumeEvent.Init(AudioBus.Bgm, volume01));
        }

        // Sfx 볼륨 슬라이더 (UI에 연결할. 0~1)
        public void SetSfxVolume(float volume01)
        {
            isSfxOn = volume01 > 0f;
            magnetGameChannel.RaiseEvent(SoundSystemEvents.SetVolumeEvent.Init(AudioBus.Sfx, volume01));
        }

        // 휴대폰을 진동 시켜줌
        public void PlayVibration()
        {
            if (!isVibrationOn) return;

            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif
        }

        // 타이틀로. 타이틀 인덱스가 0이면 주석에 있는거 써도 됨
        public void GoToTitle()
        {
            Time.timeScale = 1f;
            SceneTransitionChannel.RaiseEvent(SceneTransitionEvents.LoadSceneEvent.Init(MainTItleScene));
            //SceneManager.LoadScene(titleSceneName);
            //SceneManager.LoadScene(0);
        }

        // 리스타트 (UI에 연결할.)
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneTransitionChannel.RaiseEvent(SceneTransitionEvents.LoadSceneEvent.Init(InGameScene));
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}