using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

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
        [SerializeField] private Button _spectateButton;

        [Header("References")] 
        [SerializeField] private List<RectTransform> gameOverElements;
        
        bool _isSpectate = false;
        

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

            if (_spectateButton != null)
                _spectateButton.onClick.AddListener(OnSpectateClicked);
        }

        private void OnDestroy()
        {
            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            if (_spectateButton != null)
                _spectateButton.onClick.RemoveListener(OnSpectateClicked);
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

        public override void Show()
        {
            base.Show();
            ChangeElementsVisibility(true); // Reset
        }

        private void OnRestartClicked()
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.SFXType.ButtonClick);

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        private void OnSpectateClicked()
        {
            // Effect
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.SFXType.ButtonClick);
            
            // Logic
            _isSpectate = !_isSpectate;
            ZenGridManager.Instance.ToggleSpectateInGameOver();

            ChangeElementsVisibility(!_isSpectate);
        }

        private void ChangeElementsVisibility(bool isVisible)
        {
            foreach (var element in gameOverElements)
            {
                element.gameObject.SetActive(isVisible);
            }
        }
    }
}
