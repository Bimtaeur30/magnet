using System.Collections.Generic;
using GameLib.EventChannelSystem;

namespace PTY.Scripts.Events
{
    /// <summary>
    /// SCRUM-28 저장 구조 이벤트. JTH 소유 MagnetGameEvents.cs는 수정하지 않고 별도 파일로 둔다
    /// (EventChannelSO는 Type 기준 라우팅이라 같은 magnetGameChannel에 raise 가능).
    /// </summary>
    public static class SaveEvents
    {
        public static readonly BestScoreUpdatedEvent BestScoreUpdatedEvent = new();
        public static readonly SaveSyncCompletedEvent SaveSyncCompletedEvent = new();
        public static readonly SaveDataLoadedEvent SaveDataLoadedEvent = new();
    }

    public sealed class BestScoreUpdatedEvent : GameEvent
    {
        public int NewBestScore { get; private set; }
        public int PreviousBestScore { get; private set; }

        public BestScoreUpdatedEvent Init(int newBestScore, int previousBestScore)
        {
            NewBestScore = newBestScore;
            PreviousBestScore = previousBestScore;
            return this;
        }
    }

    public sealed class SaveSyncCompletedEvent : GameEvent
    {
    }

    /// <summary>
    /// 저장 데이터 로드 완료 시점의 스냅샷. 스킨/점수 등 다른 워크스페이스 소비자가
    /// 각자 필요할 때 이 이벤트를 구독해 자기 상태를 복원하는 용도.
    /// </summary>
    public sealed class SaveDataLoadedEvent : GameEvent
    {
        public int BestScore { get; private set; }
        public IReadOnlyList<string> UnlockedSkinIds { get; private set; }
        public string EquippedSkinId { get; private set; }
        public float TotalPlayTime { get; private set; }
        public int MaxExplosionCombo { get; private set; }
        public int GameOverCount { get; private set; }

        public SaveDataLoadedEvent Init(
            int bestScore,
            IReadOnlyList<string> unlockedSkinIds,
            string equippedSkinId,
            float totalPlayTime,
            int maxExplosionCombo,
            int gameOverCount)
        {
            BestScore = bestScore;
            UnlockedSkinIds = unlockedSkinIds;
            EquippedSkinId = equippedSkinId;
            TotalPlayTime = totalPlayTime;
            MaxExplosionCombo = maxExplosionCombo;
            GameOverCount = gameOverCount;
            return this;
        }
    }
}
