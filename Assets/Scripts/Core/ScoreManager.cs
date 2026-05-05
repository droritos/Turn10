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

        public int Score        => _score;
        public int BestScore    => _bestScore;
        public int CurrentPhase => _currentPhase;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        }

        // ── Event Subscriptions ──

        private void Start()
        {
            // Subscribe to the event when this script starts
            if (ZenGridManager.Instance != null)
            {
                ZenGridManager.Instance.OnGameStartedEvent += ResetScore;
            }
        }

        private void OnDestroy()
        {
            // ALWAYS unsubscribe when destroyed to prevent memory leaks!
            if (ZenGridManager.Instance != null)
            {
                ZenGridManager.Instance.OnGameStartedEvent -= ResetScore;
            }
        }

        // ── Core Logic ──

        public void UpdateScore(int add)
        {
            _score += add;

            // This automatically handles updating the best score when you beat it!
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
            // Resets the current score back to 0 for the new game
            _score = 0;
            _currentPhase = 1;
            
            if (GameplayUI.Instance != null)
            {
                GameplayUI.Instance.UpdateScoreDisplay(_score, _bestScore);
            }
        }
    }
}