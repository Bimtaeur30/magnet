using GameLib.EventChannelSystem;

namespace JTH.Scripts.Events
{
    public static class MagnetGameEvents
    {
        public static readonly Phase0ReadyEvent Phase0ReadyEvent = new();
        public static readonly BlockPlacedEvent BlockPlacedEvent = new();
        public static readonly BoundaryViolationEvent BoundaryViolationEvent = new();
        public static readonly SquareClearedEvent SquareClearedEvent = new();
        public static readonly BoardRotatedEvent BoardRotatedEvent = new();
        public static readonly ScoreChangedEvent ScoreChangedEvent = new();
        public static readonly SkinUnlockedEvent SkinUnlockedEvent = new();
        public static readonly GameOverEvent GameOverEvent = new();
    }

    public sealed class BlockPlacedEvent : GameEvent
    {
        public int BlockId { get; private set; }

        public BlockPlacedEvent Init(int blockId)
        {
            BlockId = blockId;
            return this;
        }
    }

    public sealed class BoundaryViolationEvent : GameEvent { }

    public sealed class SquareClearedEvent : GameEvent
    {
        public int SquareSize { get; private set; }
        public int ScoreAwarded { get; private set; }

        public SquareClearedEvent Init(int squareSize, int scoreAwarded)
        {
            SquareSize = squareSize;
            ScoreAwarded = scoreAwarded;
            return this;
        }
    }

    public sealed class BoardRotatedEvent : GameEvent
    {
        public int DegreesClockwise { get; private set; }

        public BoardRotatedEvent Init(int degreesClockwise)
        {
            DegreesClockwise = degreesClockwise;
            return this;
        }
    }

    public sealed class ScoreChangedEvent : GameEvent
    {
        public int TotalScore { get; private set; }

        public ScoreChangedEvent Init(int totalScore)
        {
            TotalScore = totalScore;
            return this;
        }
    }

    public sealed class SkinUnlockedEvent : GameEvent
    {
        public string SkinId { get; private set; }

        public SkinUnlockedEvent Init(string skinId)
        {
            SkinId = skinId;
            return this;
        }
    }

    public sealed class GameOverEvent : GameEvent
    {
        public int FinalScore { get; private set; }

        public GameOverEvent Init(int finalScore)
        {
            FinalScore = finalScore;
            return this;
        }
    }

    /// <summary>Phase 0 검증용. 이후 Phase에서 제거 가능.</summary>
    public sealed class Phase0ReadyEvent : GameEvent { }
}
