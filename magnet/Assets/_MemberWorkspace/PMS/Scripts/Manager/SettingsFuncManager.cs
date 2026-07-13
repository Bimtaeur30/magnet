using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace PMS.Scripts.Manager
{
    public class SettingsFuncManager : MonoBehaviour
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("Scene")]
        [SerializeField] private string titleSceneName = "TitleScene";

        // 이거 "무적권" 오디오 믹서 파라미터랑 맞춰야 함
        private const string BgmMixerParameter = "BGMVolume";
        private const string SfxMixerParameter = "SFXVolume";

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

            SetMixerVolume(BgmMixerParameter, isBgmOn);
        }

        // Sfx 토글 (UI에 연결할.)
        public void ToggleSfx()
        {
            isSfxOn = !isSfxOn;

            SetMixerVolume(SfxMixerParameter, isSfxOn);
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
            SceneManager.LoadScene(titleSceneName);
            //SceneManager.LoadScene(0);
        }

        // 리스타트 (UI에 연결할.)
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // 헬퍼 함수
        private void SetMixerVolume(string parameterName, bool isOn)
        {
            if (audioMixer == null)
            {
                Debug.LogError("AudioMixer 연결!!!!!!!!!!!!!!!!!!!");
                return;
            }

            float volume = isOn ? 0f : -80f;

            audioMixer.SetFloat(parameterName, volume);
        }
    }
}