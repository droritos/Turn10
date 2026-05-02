using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ZenGrid.UI
{
    /// <summary>
    /// HUD shown during active gameplay.
    /// Owns all in-game score display labels.
    /// ZenGridManager calls UpdateScoreDisplay() to push data — it never touches TextMeshPro directly.
    /// </summary>
    public class GameplayUI : BaseMenu
    {
        public static GameplayUI Instance { get; private set; }

        public override MenuType MenuType => MenuType.Gameplay;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _bestScoreText;
        [SerializeField] private Button _pauseButton;

        [Header("Mode Label (Optional)")]
        [Tooltip("Assign a TMP label to show the active mode name (e.g. 'Classic' / 'Pure Zen').")]
        [SerializeField] private TextMeshProUGUI _modeLabel;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        public override void Show()
        {
            base.Show();
            // Refresh the mode label every time gameplay starts
            if (_modeLabel != null && GameModeManager.Instance != null)
                _modeLabel.text = GameModeManager.Instance.ModeName;
        }

        private void Start()
        {
            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(OnPauseClicked);
        }

        private void OnDestroy()
        {
            if (_pauseButton != null)
                _pauseButton.onClick.RemoveListener(OnPauseClicked);
        }

        /// <summary>Called by ZenGridManager whenever the score changes.</summary>
        public void UpdateScoreDisplay(int score, int bestScore)
        {
            if (_scoreText != null)
                _scoreText.text = score.ToString();
            if (_bestScoreText != null)
                _bestScoreText.text = bestScore.ToString();
        }

        private void OnPauseClicked()
        {
            MenuManager.Instance.OpenMenu(MenuType.MainMenu);
        }
    }
}