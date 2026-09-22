using UnityEngine;

namespace Magnet.Core.Settings
{
    /// <summary>
    /// 진동 on/off 전역 상태. 설정 UI(SettingsFuncManager)가 값을 바꾸고,
    /// 진동을 내는 쪽(GameFeedbackBootstrap 등)은 반드시 Vibrate()를 거쳐 호출한다.
    /// 설정 토글(Toggle_UI)이 자기 isOn을 직렬화로 들고 있어서 여기서는 세션 간 저장을 하지 않는다.
    /// </summary>
    public static class VibrationSettings
    {
        public static bool IsEnabled { get; set; } = true;

        public static void Vibrate()
        {
            if (!IsEnabled)
            {
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        // Domain Reload를 끈 에디터 플레이에서도 기본값으로 시작하도록 리셋.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            IsEnabled = true;
        }
    }
}
