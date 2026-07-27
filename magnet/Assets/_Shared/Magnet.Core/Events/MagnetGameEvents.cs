using System.Collections.Generic;
using GameLib.EventChannelSystem;
using Magnet.Core.SO.Block;
using UnityEngine;

namespace Magnet.Core.Events
{
    public static class MagnetGameEvents
    {
        public static readonly BlockPlacedEvent BlockPlacedEvent = new();
        public static readonly ScoreChangedEvent ScoreChangedEvent = new();
        public static readonly ComboChangedEvent ComboChangedEvent = new();
        public static readonly GameOverEvent GameOverEvent = new();
        public static readonly BlockCandidatesUpdatedEvent BlockCandidatesUpdatedEvent = new();
        public static readonly BlockSelectedOnUIEvent BlockSelectedOnUIEvent = new();
    }

    public sealed class BlockPlacedEvent : GameEvent
    {
        public Vector2Int Pivot { get; private set; }
        public IReadOnlyList<Vector2Int> CellPositions { get; private set; }

        public BlockPlacedEvent Init(
            Vector2Int pivot,
            IReadOnlyList<Vector2Int> cellPositions)
        {
            Pivot = pivot;
            CellPositions = cellPositions;
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

    public sealed class ComboChangedEvent : GameEvent
    {
        public int Combo { get; private set; }

        public ComboChangedEvent Init(int combo)
        {
            Combo = combo;
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
    
    public sealed class BlockCandidatesUpdatedEvent : GameEvent
    {
        public IReadOnlyList<BlockShapeSO> Candidates { get; private set; }
        public IReadOnlyList<int> CandidateDegreesClockwise { get; private set; }

        public BlockCandidatesUpdatedEvent Init(
            IReadOnlyList<BlockShapeSO> candidates,
            IReadOnlyList<int> candidateDegreesClockwise)
        {
            Candidates = candidates;
            CandidateDegreesClockwise = candidateDegreesClockwise;
            return this;
        }
    }
    
    public sealed class BlockSelectedOnUIEvent : GameEvent
    {
        public int Index { get; private set; }

        public BlockSelectedOnUIEvent Init(int index)
        {
            Index = index;
            
            return this;
        }
    }
}
