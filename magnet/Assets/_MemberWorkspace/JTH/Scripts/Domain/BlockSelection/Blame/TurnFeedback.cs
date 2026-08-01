namespace JTH.Scripts.Domain.BlockSelection.Blame
{
    public readonly struct TurnFeedback
    {
        public bool IsGoodTurn { get; }
        public float LastTurnDelta { get; }
        public float TotalBlame { get; }

        /// <summary>이번 턴 blame 가산에 기여한 새 dead zone 수 (0 이상).</summary>
        public int NewDeadZones { get; }

        /// <summary>이번 턴 새로 점유된 중앙 2×2 칸 수.</summary>
        public int CenterCellsGained { get; }

        /// <summary>healthScore 개선으로 차감된 blame (0 이상). 판을 좋게 만든 턴의 보상.</summary>
        public float HealthGainRelief { get; }

        /// <summary>큰 블록 슬롯 수가 줄어들어 1회 가산이 발생했는지.</summary>
        public bool BigSlotLost { get; }

        /// <summary>배치 자유도 감소량 (0 이상, blame 가산 기여분).</summary>
        public float FreedomDrop { get; }

        /// <summary>감쇠로 누적 blame에서 빠진 양 (0 이상).</summary>
        public float DecayLoss { get; }

        public TurnFeedback(
            bool isGoodTurn,
            float lastTurnDelta,
            float totalBlame,
            int newDeadZones,
            int centerCellsGained,
            bool bigSlotLost,
            float freedomDrop,
            float decayLoss,
            float healthGainRelief)
        {
            IsGoodTurn = isGoodTurn;
            LastTurnDelta = lastTurnDelta;
            TotalBlame = totalBlame;
            NewDeadZones = newDeadZones;
            CenterCellsGained = centerCellsGained;
            BigSlotLost = bigSlotLost;
            FreedomDrop = freedomDrop;
            DecayLoss = decayLoss;
            HealthGainRelief = healthGainRelief;
        }
    }
}
