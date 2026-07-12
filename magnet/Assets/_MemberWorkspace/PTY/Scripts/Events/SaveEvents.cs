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
        public bool CloudSyncSucceeded { get; private set; }

        public SaveSyncCompletedEvent Init(bool cloudSyncSucceeded)
        {
            CloudSyncSucceeded = cloudSyncSucceeded;
            return this;
        }
    }
}
