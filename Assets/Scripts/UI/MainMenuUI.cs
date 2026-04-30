using UnityEngine;
using UnityEngine.UI;

namespace ZenGrid.UI
{
    public class MainMenuUI : BaseMenu
    {
        public override MenuType MenuType => MenuType.MainMenu;

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;

        protected override void Awake()
        {
            base.Awake();
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(OnPlayClicked);
            }
            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void OnDestroy()
        {
            if (_playButton != null)
                _playButton.onClick.RemoveListener(OnPlayClicked);
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnPlayClicked()
        {
            MenuManager.Instance.OpenMenu(MenuType.Gameplay);

            // Start the gameplay
            if (ZenGridManager.Instance != null)
            {
                ZenGridManager.Instance.StartGame();
            }
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}