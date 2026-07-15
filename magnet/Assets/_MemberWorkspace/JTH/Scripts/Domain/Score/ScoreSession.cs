using System;
using System.Collections.Generic;
using JTH.Scripts.Data;

namespace JTH.Scripts.Domain.Score
{
    /// <summary>
    /// 인게임 세션 누적 점수·콤보·턴 클리어 여부.
    /// Bootstrap 연동 전 Domain만 소유한다.
    /// </summary>
    public sealed class ScoreSession
    {
        private readonly ScoreCalculator _calculator;
        private bool _clearedThisTurn;

        public ScoreSession(ScoreConfigSO config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _calculator = new ScoreCalculator(config);
        }

        public int TotalScore { get; private set; }
        public int Combo { get; private set; }
        public bool ClearedThisTurn => _clearedThisTurn;

        /// <summary>
        /// 성공 배치 1회 반영. <paramref name="waveSquareSizes"/>가 비면 배치 칸 점수만.
        /// </summary>
        public PlacementScoreResult ApplyPlacement(int cellsPlaced, IReadOnlyList<int> waveSquareSizes)
        {
            if (waveSquareSizes == null || waveSquareSizes.Count == 0)
            {
                int placed = Math.Max(0, cellsPlaced);
                TotalScore += placed;
                return new PlacementScoreResult(
                    scoreDelta: placed,
                    totalScore: TotalScore,
                    comboAfter: Combo,
                    waveScores: Array.Empty<int>(),
                    hadClear: false);
            }

            _clearedThisTurn = true;
            var waveScores = new int[waveSquareSizes.Count];
            int delta = 0;

            for (int i = 0; i < waveSquareSizes.Count; i++)
            {
                Combo++;
                int waveIndex1Based = i + 1;
                int waveScore = _calculator.ComputeWaveScore(Combo, waveSquareSizes[i], waveIndex1Based);
                waveScores[i] = waveScore;
                delta += waveScore;
            }

            TotalScore += delta;
            return new PlacementScoreResult(
                scoreDelta: delta,
                totalScore: TotalScore,
                comboAfter: Combo,
                waveScores: waveScores,
                hadClear: true);
        }

        /// <summary>
        /// 핸드(턴) 종료 시 호출. 이번 턴 클리어가 없으면 콤보 리셋.
        /// </summary>
        public void NotifyTurnEnded()
        {
            if (!_clearedThisTurn)
            {
                Combo = 0;
            }

            _clearedThisTurn = false;
        }

        public void Reset()
        {
            TotalScore = 0;
            Combo = 0;
            _clearedThisTurn = false;
        }
    }
}
