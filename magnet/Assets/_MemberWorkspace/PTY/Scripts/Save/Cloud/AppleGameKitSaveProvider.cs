using System;
using Cysharp.Threading.Tasks;
using Magnet.Contracts.Save;

namespace PTY.Scripts.Save.Cloud
{
    /// <summary>
    /// TODO(SCRUM-28 기능 구현, iOS): Apple 공식 GameKit Unity 플러그인(github.com/apple/unityplugins) 설치 후
    /// GKLocalPlayer로 로그인하고 Saved Games API로 GameSaveData를 읽고 쓴다.
    /// 현재 프로젝트에 해당 패키지가 설치되어 있지 않으므로, 패키지 설치가 선행되어야 한다.
    /// </summary>
    public class AppleGameKitSaveProvider : ICloudSaveProvider
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
