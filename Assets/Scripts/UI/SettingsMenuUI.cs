using UnityEngine;
using UnityEngine.Audio;
using ZenGrid.UI;

namespace ZenUI
{
    public class SettingsMenuUI : BaseMenu
    {
        public override MenuType MenuType => MenuType.Settings;

        [Header("UI References")]
        [SerializeField] ButtonUI _musicToggleButton;
        [SerializeField] ButtonUI _sfxToggleButton;
        [SerializeField] ButtonUI _returnToMainMenuButton;

        // PlayerPrefs keys — persistent across sessions
        private const string PREF_MUSIC_MUTED = "MusicMuted";
        private const string PREF_SFX_MUTED   = "SFXMuted";

        private bool _musicMuted;
        private bool _sfxMuted;

        // ── Lifecycle ──────────────────────────────────────────

        private void Awake()
        {
            // Restore persisted state
            base.Awake();
            _musicMuted = PlayerPrefs.GetInt(PREF_MUSIC_MUTED, 0) == 1;
            _sfxMuted   = PlayerPrefs.GetInt(PREF_SFX_MUTED,   0) == 1;
        }

        private void OnEnable()
        {
            _musicToggleButton.Button.onClick.AddListener(ToggleMusic);
            _sfxToggleButton.Button.onClick.AddListener(ToggleSFX);

            if (_returnToMainMenuButton != null)
                _returnToMainMenuButton.Button.onClick.AddListener(OnReturnToMainMenu);

            // Apply persisted state to mixer on open (in case SoundManager restarted)
            ApplyMusicMute(_musicMuted);
            ApplySFXMute(_sfxMuted);

            RefreshMusicButton();
            RefreshSFXButton();
        }

        private void OnDisable()
        {
            _musicToggleButton.Button.onClick.RemoveListener(ToggleMusic);
            _sfxToggleButton.Button.onClick.RemoveListener(ToggleSFX);

            if (_returnToMainMenuButton != null)
                _returnToMainMenuButton.Button.onClick.RemoveListener(OnReturnToMainMenu);
        }

        public override void Show()
        {
            RefreshMusicButton();
            RefreshSFXButton();
            base.Show();
        }

        // ── Toggle logic ───────────────────────────────────────

        private void ToggleMusic()
        {
            _musicMuted = !_musicMuted;
            PlayerPrefs.SetInt(PREF_MUSIC_MUTED, _musicMuted ? 1 : 0);
            PlayerPrefs.Save();

            ApplyMusicMute(_musicMuted);
            RefreshMusicButton();

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.SFXType.ButtonClick);
        }

        private void ToggleSFX()
        {
            _sfxMuted = !_sfxMuted;
            PlayerPrefs.SetInt(PREF_SFX_MUTED, _sfxMuted ? 1 : 0);
            PlayerPrefs.Save();

            ApplySFXMute(_sfxMuted);
            RefreshSFXButton();

            // Play a click ONLY if SFX is now unmuted so the user hears the confirmation
            if (!_sfxMuted && SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.SFXType.ButtonClick);
        }

        // ── Mixer application ──────────────────────────────────

        private void ApplyMusicMute(bool mute)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.SetMusicMuted(mute);
        }

        private void ApplySFXMute(bool mute)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.SetSFXMuted(mute);
        }

        // ── Button text refresh ────────────────────────────────

        private void RefreshMusicButton()
        {
            _musicToggleButton.Text.SetText($"Music\n{(_musicMuted ? "Off" : "On")}");
        }

        private void RefreshSFXButton()
        {
            _sfxToggleButton.Text.SetText($"SFX\n{(_sfxMuted ? "Off" : "On")}");
        }

        // ── Navigation ─────────────────────────────────────────

        private void OnReturnToMainMenu()
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.SFXType.ButtonClick);

            MenuManager.Instance.OpenMenu(MenuType.MainMenu);
        }
    }
}

