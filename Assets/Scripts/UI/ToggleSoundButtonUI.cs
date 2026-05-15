using System;
using UnityEngine;
using UnityEngine.UI;
using ZenUI;

namespace GameUI
{
    public class ToggleSoundButtonUI : ButtonUI
    {
        [Header("Visual Elements")]
        [SerializeField] private Image soundOn;
        [SerializeField] private Image soundOff;

        private bool _sfxMuted;

        private void Awake()
        {
            // 1. Load the saved state
            _sfxMuted = PlayerPrefs.GetInt(SoundManager.PREF_SFX_MUTED, 0) == 1;
            
            // 2. Listen to the click event
            this.Button.onClick.AddListener(ToggleSFX);
        }

        private void OnEnable()
        {
            // 3. FIX: Apply the loaded state and refresh visuals on startup
            ApplySFXMute(_sfxMuted);
            RefreshSFXButton();
        }

        public void ToggleSFX()
        {
            _sfxMuted = !_sfxMuted;
            PlayerPrefs.SetInt(SoundManager.PREF_SFX_MUTED, _sfxMuted ? 1 : 0);
            PlayerPrefs.Save();

            ApplySFXMute(_sfxMuted);
            RefreshSFXButton();

            // Play a click ONLY if SFX is now unmuted so the user hears the confirmation
            if (!_sfxMuted && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.SFXType.ButtonClick);
            }
        }
        
        private void ApplySFXMute(bool mute)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetSFXMuted(mute);
            }
        }

        private void RefreshSFXButton()
        { 
            if (soundOff != null && soundOn != null)
            {
                soundOff.enabled = _sfxMuted;
                soundOn.enabled = !_sfxMuted;
            }
        }
    }
}