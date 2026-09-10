using System.Collections.Generic;
using GameLib.EventChannelSystem;
using Magnet.Contracts;
using UnityEngine;

namespace Magnet.Core.Events
{
    public static class MagnetGameEvents
    {
        public static readonly ComboChangedEvent ComboChangedEvent = new();
        public static readonly ScoreChangedEvent ScoreChangedEvent = new();
        public static readonly GameOverEvent GameOverEvent = new();
        public static readonly BlockCandidatesUpdatedEvent BlockCandidatesUpdatedEvent = new();
        public static readonly BlockSelectedOnUIEvent BlockSelectedOnUIEvent = new();
        public static readonly RelifeOfferedEvent RelifeOfferedEvent = new();
        public static readonly RelifeAcceptedEvent RelifeAcceptedEvent = new();
        public static readonly UniqueCorrectPlacementEvent UniqueCorrectPlacementEvent = new();
        public static readonly PerfectClearEvent PerfectClearEvent = new();
        public static readonly AllClearEvent AllClearEvent = new();
    }

    /// <summary>
    /// 핸드 3피스를 전부 놓았고, 누적 클리어 라인 수가 그 핸드로 낼 수 있는 최적값과 같을 때.
    /// 같은 배치로 올클리어까지 났다면 <see cref="AllClearEvent"/>만 나가고 이 이벤트는 생략된다.
    /// </summary>
    public sealed class PerfectClearEvent : GameEvent
    {
    }

    /// <summary>배치 후 라인 클리어가 끝난 시점에 보드가 완전히 빈 상태가 됐을 때.</summary>
    public sealed class AllClearEvent : GameEvent
    {
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

    public sealed class RelifeOfferedEvent : GameEvent
    {
        public IReadOnlyList<IReadOnlyList<Vector2Int>> CellOffsetsList { get; private set; }

        public RelifeOfferedEvent Init(IReadOnlyList<IReadOnlyList<Vector2Int>> cellOffsetsList)
        {
            CellOffsetsList = cellOffsetsList;
            return this;
        }
    }

    public sealed class RelifeAcceptedEvent : GameEvent
    {
        public RelifeAcceptedEvent Init()
        {
            return this;
        }
    }

    /// <summary>
    /// 유일수 손에서 정답 칸에 놓았을 때. UI 피드백용 셀 월드 중심 배열.
    /// </summary>
    public sealed class UniqueCorrectPlacementEvent : GameEvent
    {
        public IReadOnlyList<Vector3> WorldPositions { get; private set; }

        public UniqueCorrectPlacementEvent Init(IReadOnlyList<Vector3> worldPositions)
        {
            WorldPositions = worldPositions;
            return this;
        }
    }
}
