using System.Collections.Generic;
using GameLib.EventChannelSystem;
using Magnet.Contracts;
using UnityEngine;

namespace Magnet.Core.Events
{
    public static class MagnetGameEvents
    {
        public static readonly ComboChangedEvent ComboChangedEvent = new();
        public static readonly GameOverEvent GameOverEvent = new();
        public static readonly BlockCandidatesUpdatedEvent BlockCandidatesUpdatedEvent = new();
        public static readonly BlockSelectedOnUIEvent BlockSelectedOnUIEvent = new();
    }
    
    public sealed class ComboChangedEvent : GameEvent
    {
        public int Combo { get; private set; }
        public Vector3 WorldPosition { get; private set; }

        public ComboChangedEvent Init(int combo, Vector3 worldPosition)
        {
            Combo = combo;
            WorldPosition = worldPosition;
            return this;
        }
    }

    public sealed class GameOverEvent : GameEvent
    {
        public int FinalStage { get; private set; }

        public GameOverEvent Init(int finalStage)
        {
            FinalStage = finalStage;
            return this;
        }
    }
    
    public sealed class BlockCandidatesUpdatedEvent : GameEvent
    {
        public IReadOnlyList<ShapeBlockData> Candidates { get; set; }

        public BlockCandidatesUpdatedEvent Init(IReadOnlyList<ShapeBlockData> candidates)
        {
            Candidates = candidates;
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
