using UnityEngine;

namespace JTH.Scripts.Domain.Score
{
    /// <summary>
    /// Block Blast 스타일 line-clear 점수 순수 계산.
    /// S = λ(n) × base × clearIndexInChain × tier
    /// clearIndexInChain: 체인 안 N번째 클리어(1부터). UI 콤보(=clearIndex-1)와 다를 수 있음.
    /// </summary>
    public static class ScoreCalculator
    {
        public static int LineMultiplier(int clearedLineCount)
        {
            if (clearedLineCount <= 1)
            {
                return 1;
            }

            return clearedLineCount * (clearedLineCount - 1);
        }

        public static float ResolveTier(int clearIndexInChain)
        {
            if (clearIndexInChain <= 5)
            {
                return 1f;
            }

            if (clearIndexInChain <= 10)
            {
                return 1.5f;
            }

            return 2f;
        }

        public static int ClearScore(int clearedLineCount, int clearIndexInChain, int sessionBase)
        {
            if (clearedLineCount <= 0 || clearIndexInChain <= 0 || sessionBase <= 0)
            {
                return 0;
            }

            float raw = LineMultiplier(clearedLineCount)
                * sessionBase
                * clearIndexInChain
                * ResolveTier(clearIndexInChain);
            return Mathf.RoundToInt(raw);
        }
    }
}
