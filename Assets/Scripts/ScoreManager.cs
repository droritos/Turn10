using UnityEngine;
using ZenGrid.UI;

namespace ZenGrid
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance;

        private const string BestScoreKey = "ZenGrid_BestScore";

        private int _score = 0;
        private int _bestScore = 0;
        private int _currentPhase = 1;

        public int Score => _score;
        public int BestScore => _bestScore;
        public int CurrentPhase => _currentPhase;

        private void Awake()
        {
            Instance = this;
            _bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        }

        public void UpdateScore(int add)
        {
            _score += add;

            if (_score > _bestScore)
            {
                _bestScore = _score;
                PlayerPrefs.SetInt(BestScoreKey, _bestScore);
                PlayerPrefs.Save();
            }

            _currentPhase = (_score / 150) + 1;

            if (GameplayUI.Instance != null)
            {
                GameplayUI.Instance.UpdateScoreDisplay(_score, _bestScore);
            }
        }

        public void ResetScore()
        {
            _score = 0;
            _currentPhase = 1;
            if (GameplayUI.Instance != null)
            {
                GameplayUI.Instance.UpdateScoreDisplay(_score, _bestScore);
            }
        }
    }
}
