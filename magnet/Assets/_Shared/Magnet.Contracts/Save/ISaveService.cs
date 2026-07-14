using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Magnet.Contracts.Save
{
    /// <summary>
    /// 로컬 저장을 오케스트레이션하는 퍼사드. 소비자(UI 등)는 이 인터페이스만 [Inject]한다.
    /// (클라우드 로그인 미구현 상태라 현재는 로컬 저장만 수행)
    /// </summary>
    public interface ISaveService
    {
        int BestScore { get; }
        IReadOnlyList<string> UnlockedSkinIds { get; }
        string EquippedSkinId { get; }
        float TotalPlayTime { get; }
        int MaxExplosionCombo { get; }
        int GameOverCount { get; }

        UniTask InitializeAsync();
        void SubmitScore(int score);
        void UnlockSkin(string skinId);
        void EquipSkin(string skinId);
        void AddPlayTime(float seconds);
        void SubmitExplosionCombo(int comboCount);
        void RecordGameOver();
        void ValidateUnlockedSkins(IReadOnlyCollection<string> validSkinIds);
    }
}
