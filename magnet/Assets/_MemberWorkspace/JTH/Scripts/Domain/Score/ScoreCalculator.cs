using System;
using JTH.Scripts.Data;

namespace JTH.Scripts.Domain.Score
{
    //TODO 고치기
    public sealed class ScoreCalculator
    {
        // private readonly ScoreConfigSO _config;
        //
        // public ScoreCalculator(ScoreConfigSO config)
        // {
        //     _config = config != null ? config : throw new ArgumentNullException(nameof(config));
        // }
        //
        // public int ComputeWaveScore(int comboAfterIncrement, int squareSize, int waveIndex1Based)
        // {
        //     if (comboAfterIncrement < 1 || squareSize < 1 || waveIndex1Based < 1)
        //     {
        //         return 0;
        //     }
        //
        //     float k = _config.GetK(comboAfterIncrement);
        //     float streakMult = _config.GetStreakMultiplier(waveIndex1Based);
        //     return (int)Math.Round(k * comboAfterIncrement * squareSize * streakMult);
        // }
    }
}
