using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    /// <summary>
    /// 앱 시작 시 목표 프레임을 60으로 고정한다. 안드로이드는 지정하지 않으면 30fps로 제한된다.
    /// 씬에 배치할 필요 없이 첫 씬 로드 전에 자동 실행된다.
    /// </summary>
    public static class FrameRateInitializer
    {
        private const int TargetFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // 모바일에선 vSyncCount가 무시되지만, 에디터/PC에서 targetFrameRate가 먹도록 꺼 둔다.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFrameRate;
        }
    }
}
