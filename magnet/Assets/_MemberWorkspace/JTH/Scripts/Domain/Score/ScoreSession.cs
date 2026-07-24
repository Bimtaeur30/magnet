using JTH.Scripts.Data;
using UnityEngine;

namespace JTH.Scripts.Domain.Score
{
    public sealed class ScoreSession
    {
        private readonly ScoreConfigSO _config;

        private int _totalScore;
        /// <summary>체인 안 클리어 횟수(점수 배수). 첫 클리어=1. UI 콤보는 이 값 - 1.</summary>
        private int _chainClears;
        private int _sessionBase;

        private bool _clearedThisTurn;
        private bool _clearedLastTurn;
        private bool _clearedBeforeLastTurn;
        private bool _clearedTwoLineFirstDrop;

        private bool SaveCombo =>
            _clearedThisTurn
            || _clearedLastTurn
            || (_clearedBeforeLastTurn && _clearedTwoLineFirstDrop && HasCombo);

        /// <summary>UI 콤보 1 이상(체인 두 번 이상 클리어). 구조 예외는 콤보가 있을 때만.</summary>
        private bool HasCombo => _chainClears >= 2;

        public ScoreSession(ScoreConfigSO config)
        {
            Debug.Assert(config != null, "[ScoreSession] ScoreConfigSO is null.");
            _config = config;
            Reset();
        }

        public int TotalScore => _totalScore;

        /// <summary>표시 콤보. 첫 클리어 후 0, 그다음 클리어부터 1.</summary>
        public int Combo => _chainClears <= 0 ? 0 : _chainClears - 1;

        public int SessionBase => _sessionBase;

        public PlacementScoreResult ApplyPlacement(
            int clearedLineCount,
            int cellsPlaced,
            bool firstDrop,
            bool lastDrop)
        {
            if (firstDrop)
            {
                // 구조 예외: 직전 턴 무소거여도 이번 턴 첫 수가 2줄+이면 SaveCombo에 포함
                _clearedTwoLineFirstDrop = clearedLineCount > 1;

                if (!SaveCombo)
                {
                    _chainClears = 0;
                }
            }

            if (clearedLineCount > 0)
            {
                _clearedThisTurn = true;
            }

            int delta = cellsPlaced;

            if (clearedLineCount > 0)
            {
                _chainClears++;
                delta += ScoreCalculator.ClearScore(
                    clearedLineCount,
                    _chainClears,
                    _sessionBase);
            }

            _totalScore += delta;

            bool comboAlive = SaveCombo;

            if (lastDrop)
            {
                _clearedBeforeLastTurn = _clearedLastTurn;
                _clearedLastTurn = _clearedThisTurn;
                _clearedThisTurn = false;
            }

            return new PlacementScoreResult(delta, _totalScore, Combo, comboAlive);
        }

        public void Reset()
        {
            _totalScore = 0;
            _chainClears = 0;
            _clearedThisTurn = false;
            _clearedLastTurn = false;
            _clearedBeforeLastTurn = false;
            _clearedTwoLineFirstDrop = false;
            _sessionBase = RollSessionBase();
        }

        private int RollSessionBase()
        {
            int min = _config.BaseMin;
            int max = _config.BaseMax;
            if (max < min)
            {
                (min, max) = (max, min);
            }

            return Random.Range(min, max + 1);
        }
    }
}
