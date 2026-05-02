using UnityEngine;
using ZenGrid.UI;

namespace ZenGrid
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance;

        // ── PlayerPrefs key is mode-specific so Classic and Pure Zen track separately ──
        // Legacy key (pre-mode-system) is migrated to Classic on first run.
        private const string LegacyBestScoreKey = "ZenGrid_BestScore";
        private const string BestScoreKeyPrefix  = "ZenGrid_BestScore_";

        private string BestScoreKey =>
            BestScoreKeyPrefix + (GameModeManager.Instance?.ModeName ?? "Classic");

        private int _score = 0;
        private int _bestScore = 0;
        private int _currentPhase = 1;

        public int Score        => _score;
        public int BestScore    => _bestScore;
        public int CurrentPhase => _currentPhase;

        private void Awake()
        {
            Instance = this;
            MigrateLegacyBestScore();
            _bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        }

        /// <summary>
        /// One-time migration: moves the old single best-score key to the Classic key.
        /// Runs only when the old key exists and the Classic key does not yet.
        /// </summary>
        private void MigrateLegacyBestScore()
        {
            const string classicKey = BestScoreKeyPrefix + "Classic";
            if (PlayerPrefs.HasKey(LegacyBestScoreKey) && !PlayerPrefs.HasKey(classicKey))
            {
                int legacyScore = PlayerPrefs.GetInt(LegacyBestScoreKey, 0);
                PlayerPrefs.SetInt(classicKey, legacyScore);
                PlayerPrefs.DeleteKey(LegacyBestScoreKey);
                PlayerPrefs.Save();
                Debug.Log($"[ScoreManager] Migrated legacy best score ({legacyScore}) → {classicKey}");
            }
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

            // Reload the best score for the current mode (may differ from previous mode)
            _bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);

            if (GameplayUI.Instance != null)
            {
                GameplayUI.Instance.UpdateScoreDisplay(_score, _bestScore);
            }
        }
    }
}
