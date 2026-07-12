using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameLib.EventChannelSystem;
using PTY.Scripts.Save.Local;

namespace PTY.Scripts.Save
{
    /// <summary>
    /// 로컬/클라우드 저장 오케스트레이션 뼈대. 실제 로직(초기 동기화, 필드별 최고값 우선 병합,
    /// 로컬 즉시저장 + 클라우드 비동기 push, SaveEvents 발행)은 SCRUM-28 기능 구현 담당자가 채운다.
    /// </summary>
    public sealed class SaveService : ISaveService
    {
        private readonly ILocalSaveRepository _localRepository;
        private readonly ICloudSaveProvider _cloudProvider;
        private readonly EventChannelSO _magnetGameChannel;

        public SaveService(
            ILocalSaveRepository localRepository,
            ICloudSaveProvider cloudProvider,
            EventChannelSO magnetGameChannel)
        {
            _localRepository = localRepository;
            _cloudProvider = cloudProvider;
            _magnetGameChannel = magnetGameChannel;
        }

        public int BestScore => throw new NotImplementedException();
        public IReadOnlyList<string> UnlockedSkinIds => throw new NotImplementedException();

        public UniTask InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public void SubmitScore(int score)
        {
            throw new NotImplementedException();
        }

        public void UnlockSkin(string skinId)
        {
            throw new NotImplementedException();
        }

        public UniTask<bool> ForceSyncAsync()
        {
            throw new NotImplementedException();
        }
    }
}
