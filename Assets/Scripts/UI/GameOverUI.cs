using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ZenGrid.UI
{
    /// <summary>
    /// Game Over screen. Shown by MenuManager when the game ends.
    /// ZenGridManager calls Show(score, bestScore) to populate the labels before opening this menu.
    /// </summary>
    public class GameOverUI : BaseMenu
    {
        public static GameOverUI Instance { get; private set; }

        public override MenuType MenuType => MenuType.GameOver;

        [Header("Game Over Labels")]
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _finalBestScoreText;

        [Header("Buttons")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        private void Start()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);
            else
                Debug.LogWarning("[GameOverUI] Restart Button not assigned.", this);

            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnDestroy()
        {
            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        /// <summary>
        /// Populate score labels. Call this BEFORE MenuManager.OpenMenu(GameOver).
        /// </summary>
        public void Populate(int finalScore, int bestScore)
        {
            if (_finalScoreText != null)
                _finalScoreText.text = finalScore.ToString();
            if (_finalBestScoreText != null)
                _finalBestScoreText.text = bestScore.ToString();
        }

        private void OnRestartClicked()
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.SFX.ButtonClick);

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        private void OnMainMenuClicked()
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.SFX.ButtonClick);

            // Go back to main menu without reloading the scene
            MenuManager.Instance.OpenMenu(MenuType.MainMenu);
        }
    }
}
