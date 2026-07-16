using System;
using JTH.Scripts.Data;

namespace JTH.Scripts.Domain.Score
{
    /// <summary>
    /// 클리어 웨이브 1건 점수: round(k × combo × squareSize × streakMult).
    /// </summary>
    public sealed class ScoreCalculator
    {
        private readonly ScoreConfigSO _config;

        public ScoreCalculator(ScoreConfigSO config)
        {
            _config = config != null ? config : throw new ArgumentNullException(nameof(config));
        }

        public int ComputeWaveScore(int comboAfterIncrement, int squareSize, int waveIndex1Based)
        {
            if (comboAfterIncrement < 1 || squareSize < 1 || waveIndex1Based < 1)
            {
                return 0;
            }

            float k = _config.GetK(comboAfterIncrement);
            float streakMult = _config.GetStreakMultiplier(waveIndex1Based);
            return (int)Math.Round(k * comboAfterIncrement * squareSize * streakMult);
        }
    }
}
