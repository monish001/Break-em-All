using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
namespace Break_em_All
{
    class HighScores
    {

        private IsolatedStorageSettings userSettings = IsolatedStorageSettings.ApplicationSettings;
        private string HIGH_SCORE_KEY = "highScoresList";
        private int MAX_COUNT = 5;

        private void SetHighScores(List<int> highScores)
        {
            this.userSettings[this.HIGH_SCORE_KEY] = highScores;
        }

        public List<int> GetHighScores()
        {
            try
            {
                return (List<int>)this.userSettings[this.HIGH_SCORE_KEY];
            }
            catch (KeyNotFoundException)
            {
                return new List<int>();
            }
        }

        public void updateHighScore(int score)
        {
            List<int> highScores = this.GetHighScores();

            if (highScores.Contains(score))
            {
                return;
            }
            highScores.Add(score);
            highScores = highScores.OrderByDescending(s => s).ToList();
            if (highScores.Count > this.MAX_COUNT)
            {
                highScores.RemoveAt(this.MAX_COUNT);
            }
            this.SetHighScores(highScores);
        }
    }
}
