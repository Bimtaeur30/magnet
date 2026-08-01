namespace JTH.Scripts.Domain.BlockSelection.Blame
{
    public readonly struct TurnFeedback
    {
        public bool IsGoodTurn { get; }
        public float LastTurnDelta { get; }
        public float TotalBlame { get; }

        public TurnFeedback(bool isGoodTurn, float lastTurnDelta, float totalBlame)
        {
            IsGoodTurn = isGoodTurn;
            LastTurnDelta = lastTurnDelta;
            TotalBlame = totalBlame;
        }
    }
}
