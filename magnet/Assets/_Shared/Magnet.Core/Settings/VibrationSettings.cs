using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Magnet.Core.Settings
{
    /// <summary>
    /// 진동 on/off 전역 상태. 설정 UI(SettingsFuncManager)가 값을 바꾸고,
    /// 진동을 내는 쪽(GameFeedbackBootstrap 등)은 Vibrate()/VibratePlacement()를 거쳐 호출한다.
    /// 설정 토글(Toggle_UI)이 자기 isOn을 직렬화로 들고 있어서 여기서는 세션 간 저장을 하지 않는다.
    /// </summary>
    public static class VibrationSettings
    {
        private const long PlacementDurationMilliseconds = 20;

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void Magnet_PlayPlacementHaptic();
#endif

        public static bool IsEnabled { get; set; } = true;

        /// <summary>블록 설치 성공 시 짧은 단발 탭. 기존 진동 설정을 따른다.</summary>
        public static void VibratePlacement()
        {
            if (!IsEnabled)
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    if (vibrator == null || !vibrator.Call<bool>("hasVibrator"))
                        return;

                    using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    {
                        int sdk = version.GetStatic<int>("SDK_INT");
                        if (sdk >= 26)
                        {
                            using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                            using (var effect = sdk >= 29
                                ? effectClass.CallStatic<AndroidJavaObject>("createPredefined", 2) // EFFECT_TICK
                                : effectClass.CallStatic<AndroidJavaObject>("createOneShot", PlacementDurationMilliseconds, -1))
                            {
                                vibrator.Call("vibrate", effect);
                            }
                        }
                        else
                        {
                            vibrator.Call("vibrate", PlacementDurationMilliseconds);
                        }
                    }
                }
            }
            catch (AndroidJavaException exception)
            {
                Debug.LogWarning($"[VibrationSettings] 설치 진동을 재생하지 못했습니다: {exception.Message}");
            }
#elif UNITY_IOS && !UNITY_EDITOR
            Magnet_PlayPlacementHaptic();
#endif
        }

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
