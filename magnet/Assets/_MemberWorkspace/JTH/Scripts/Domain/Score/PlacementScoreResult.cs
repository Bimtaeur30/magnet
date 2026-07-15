using System;
using System.Collections.Generic;

namespace JTH.Scripts.Domain.Score
{
    /// <summary>
    /// 블록 1개 배치에 대한 점수 반영 결과.
    /// </summary>
    public sealed class PlacementScoreResult
    {
        public PlacementScoreResult(
            int scoreDelta,
            int totalScore,
            int comboAfter,
            IReadOnlyList<int> waveScores,
            bool hadClear)
        {
            ScoreDelta = scoreDelta;
            TotalScore = totalScore;
            ComboAfter = comboAfter;
            WaveScores = waveScores ?? Array.Empty<int>();
            HadClear = hadClear;
        }

        public int ScoreDelta { get; }
        public int TotalScore { get; }
        public int ComboAfter { get; }
        public IReadOnlyList<int> WaveScores { get; }
        public bool HadClear { get; }
    }
}
