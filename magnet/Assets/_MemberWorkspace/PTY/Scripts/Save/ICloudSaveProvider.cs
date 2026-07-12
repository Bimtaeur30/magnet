using Cysharp.Threading.Tasks;

namespace PTY.Scripts.Save
{
    /// <summary>
    /// GPGS(Android) / Game Center(iOS) 클라우드 저장을 추상화한다.
    /// 플랫폼별 구현체는 Save/Cloud/ 아래에 둔다.
    /// </summary>
    public interface ICloudSaveProvider
    {
        bool IsAuthenticated { get; }

        UniTask<bool> AuthenticateAsync();
        UniTask<GameSaveData> LoadAsync();
        UniTask<bool> SaveAsync(GameSaveData data);
    }
}
