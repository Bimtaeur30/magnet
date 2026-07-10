using System.Collections.Generic;
using GameLib.EventChannelSystem;
using Magnet.Contracts.BlockShapes;
using UnityEngine;

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
        public static readonly GameOverEvent GameOverEvent = new();
        public static readonly BlockSelectedEvent BlockSelectedEvent = new();
        public static readonly BlockCandidatesUpdatedEvent BlockCandidatesUpdatedEvent = new();
    }

    public sealed class BlockPlacedEvent : GameEvent
    {
        public int BlockId { get; private set; }
        public int SlotIndex { get; private set; }
        public string ShapeId { get; private set; }
        public Vector2Int Pivot { get; private set; }
        public IReadOnlyList<Vector2Int> CellPositions { get; private set; }

        public BlockPlacedEvent Init(
            int blockId,
            int slotIndex,
            string shapeId,
            Vector2Int pivot,
            IReadOnlyList<Vector2Int> cellPositions)
        {
            BlockId = blockId;
            SlotIndex = slotIndex;
            ShapeId = shapeId;
            Pivot = pivot;
            CellPositions = cellPositions;
            return this;
        }
    }

    public sealed class BoundaryViolationEvent : GameEvent { }

    public sealed class SquareClearedEvent : GameEvent
    {
        public int SquareSize { get; private set; }
        public int ScoreAwarded { get; private set; }
        public IReadOnlyList<Vector2Int> ClearedCells { get; private set; }

        public SquareClearedEvent Init(int squareSize, int scoreAwarded, IReadOnlyList<Vector2Int> clearedCells)
        {
            SquareSize = squareSize;
            ScoreAwarded = scoreAwarded;
            ClearedCells = clearedCells;
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
    
    public sealed class BlockSelectedEvent : GameEvent
    {
        public int SlotIndex { get; private set; }
        public IBlockShape Shape { get; private set; }

        public BlockSelectedEvent Init(int slotIndex, IBlockShape shape)
        {
            SlotIndex = slotIndex;
            Shape = shape;
            return this;
        }
    }

    public sealed class BlockCandidatesUpdatedEvent : GameEvent
    {
        public IReadOnlyList<IBlockShape> Candidates { get; private set; }

        public BlockCandidatesUpdatedEvent Init(IReadOnlyList<IBlockShape> candidates)
        {
            Candidates = candidates;
            return this;
        }
    }
}
