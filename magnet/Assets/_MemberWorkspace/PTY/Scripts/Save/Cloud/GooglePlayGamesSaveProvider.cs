using System;
using Cysharp.Threading.Tasks;

namespace PTY.Scripts.Save.Cloud
{
    /// <summary>
    /// TODO(SCRUM-28 기능 구현, Android): Assets/GooglePlayGames의 PlayGamesPlatform으로 로그인하고,
    /// ISavedGameClient(Saved Games)로 GameSaveData를 읽고 쓴다.
    /// </summary>
    public class GooglePlayGamesSaveProvider : ICloudSaveProvider
    {
        public bool IsAuthenticated => throw new NotImplementedException();

        public UniTask<bool> AuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        public UniTask<GameSaveData> LoadAsync()
        {
            throw new NotImplementedException();
        }

        public UniTask<bool> SaveAsync(GameSaveData data)
        {
            throw new NotImplementedException();
        }
    }
}
