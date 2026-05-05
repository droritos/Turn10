using UnityEngine;
using UnityEngine.UI;

namespace ZenGrid.UI
{
    public class MainMenuUI : BaseMenu
    {
        public override MenuType MenuType => MenuType.MainMenu;

        [Header("Game Mode Configs")]
        [SerializeField] private GameModeConfig _classicConfig;
        [SerializeField] private GameModeConfig _pureZenConfig;

        [Header("Buttons")]
        //[SerializeField] private Button _playClassicButton;
        [SerializeField] private Button _playPureZenButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        private void Start()
        {
            /*
            if (_playClassicButton != null)
                _playClassicButton.onClick.AddListener(OnPlayClassicClicked);
            else
                Debug.LogError("[MainMenuUI] Play Classic Button is NOT assigned in the Inspector!", this);
            */
            if (_playPureZenButton != null)
                _playPureZenButton.onClick.AddListener(OnPlayPureZenClicked);
            else
                Debug.LogError("[MainMenuUI] Play Pure Zen Button is NOT assigned in the Inspector!", this);

            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuitClicked);
            else
                Debug.LogWarning("[MainMenuUI] Quit Button is NOT assigned in the Inspector.", this);
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            else
                Debug.LogWarning("[MainMenuUI] Quit Button is NOT assigned in the Inspector.", this);
        }

        

        private void OnDestroy()
        {
            /*
            if (_playClassicButton != null)
                _playClassicButton.onClick.RemoveListener(OnPlayClassicClicked);
                */
            if (_playPureZenButton != null)
                _playPureZenButton.onClick.RemoveListener(OnPlayPureZenClicked);
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnPlayClassicClicked()
        {
            PlayWithMode(_classicConfig);
        }

        private void OnPlayPureZenClicked()
        {
            PlayWithMode(_pureZenConfig);
        }
        private void OnSettingsClicked()
        {
            SoundManager.Instance.PlaySFX(SoundManager.SFXType.ButtonClick);
            MenuManager.Instance.OpenMenu(MenuType.Settings);
        }
        private void PlayWithMode(GameModeConfig config)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.SFXType.ButtonClick);

            // Set mode BEFORE StartGame so all systems read the correct config
            if (GameModeManager.Instance != null)
                GameModeManager.Instance.SetMode(config);
            else
                Debug.LogWarning("[MainMenuUI] GameModeManager.Instance is null! Mode will default to Classic.");

            MenuManager.Instance.OpenMenu(MenuType.Gameplay);

            if (ZenGridManager.Instance != null)
                ZenGridManager.Instance.StartGame();
            else
                Debug.LogError("[MainMenuUI] ZenGridManager.Instance is null! Cannot start game.");
        }

        private void OnQuitClicked()
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.SFXType.ButtonClick);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}