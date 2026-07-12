using Cysharp.Threading.Tasks;

namespace PTY.Scripts.Save.Cloud
{
    /// <summary>
    /// 클라우드 저장을 지원하지 않는 환경(에디터, 미지원 플랫폼)의 폴백.
    /// 항상 미인증 상태를 반환해 SaveService가 로컬 저장만으로 동작하게 한다.
    /// </summary>
    public sealed class NullCloudSaveProvider : ICloudSaveProvider
    {
        public bool IsAuthenticated => false;

        public UniTask<bool> AuthenticateAsync() => UniTask.FromResult(false);

        public UniTask<GameSaveData> LoadAsync() => UniTask.FromResult<GameSaveData>(null);

        public UniTask<bool> SaveAsync(GameSaveData data) => UniTask.FromResult(false);
    }
}
