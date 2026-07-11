using Mvvm;

namespace Game.UI
{
    public sealed partial class ScoreUIViewModel
    {
        public void SetCurrentScore(int score)
        {
            CurrentScoreTxt = score.ToString();
        }

        public void SetBestScore(int score)
        {
            BestScoreTxt = score.ToString();
        }
    }
}
