using UnityEngine;
using UnityEngine.UI;

namespace ZenGrid.UI
{
    public class MainMenuUI : BaseMenu
    {
        public override MenuType MenuType => MenuType.MainMenu;

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;

        private void Start()
        {
            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlayClicked);
            else
                Debug.LogError("[MainMenuUI] Play Button is NOT assigned in the Inspector!", this);

            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuitClicked);
            else
                Debug.LogWarning("[MainMenuUI] Quit Button is NOT assigned in the Inspector.", this);
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
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.SFX.ButtonClick);

            MenuManager.Instance.OpenMenu(MenuType.Gameplay);

            if (ZenGridManager.Instance != null)
                ZenGridManager.Instance.StartGame();
            else
                Debug.LogError("[MainMenuUI] ZenGridManager.Instance is null! Cannot start game.");
        }

        private void OnQuitClicked()
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.SFX.ButtonClick);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}