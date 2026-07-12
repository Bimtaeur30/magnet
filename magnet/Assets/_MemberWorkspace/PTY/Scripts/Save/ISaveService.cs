using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace PTY.Scripts.Save
{
    /// <summary>
    /// 로컬/클라우드 저장을 오케스트레이션하는 퍼사드. 소비자(UI 등)는 이 인터페이스만 [Inject]한다.
    /// </summary>
    public interface ISaveService
    {
        int BestScore { get; }
        IReadOnlyList<string> UnlockedSkinIds { get; }

        UniTask InitializeAsync();
        void SubmitScore(int score);
        void UnlockSkin(string skinId);
        UniTask<bool> ForceSyncAsync();
    }
}
