namespace JTH.Scripts.Domain.Score
{
    public sealed class PlacementScoreResult
    {
        public PlacementScoreResult(
            int scoreDelta,
            int totalScore,
            int comboAfter,
            bool comboAlive)
        {
            ScoreDelta = scoreDelta;
            TotalScore = totalScore;
            ComboAfter = comboAfter;
            ComboAlive = comboAlive;
        }
        
        public int ScoreDelta { get; }
        public int TotalScore { get; }
        public int ComboAfter { get; }
        public bool ComboAlive { get; }
    }
}
