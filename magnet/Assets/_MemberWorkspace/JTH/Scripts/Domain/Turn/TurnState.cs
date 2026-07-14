namespace JTH.Scripts.Domain.Turn
{
    /// <summary>
    /// 핸드(후보 슬롯 전부) 소진 단위의 턴 번호. 배치마다의 회전과는 무관하다.
    /// </summary>
    public sealed class TurnState
    {
        public int TurnIndex { get; private set; }

        /// <summary>게임 시작 시 첫 턴을 1로 연다.</summary>
        public void BeginFirstTurn()
        {
            TurnIndex = 1;
        }

        /// <summary>핸드 소진·리필 후 다음 턴 번호로 진행한다.</summary>
        public void AdvanceAfterHandExhausted()
        {
            TurnIndex++;
        }
    }
}
