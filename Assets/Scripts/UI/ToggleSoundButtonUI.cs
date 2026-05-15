using System;
using UnityEngine;
using UnityEngine.UI;
using ZenUI;

namespace GameUI
{
    public class ToggleSoundButtonUI : ButtonUI
    {
        [SerializeField] Image soundOn;
        [SerializeField] Image soundOff;

        private bool _sfxMuted;
        private void Awake()
        {
            _sfxMuted = PlayerPrefs.GetInt(SoundManager.PREF_SFX_MUTED,   0) == 1;
            
            this.Button.onClick.AddListener(ToggleSFX);
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
                SoundManager.Instance.PlaySFX(SoundManager.SFXType.ButtonClick);
        }
        
        private void ApplySFXMute(bool mute)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.SetSFXMuted(mute);
        }
        private void RefreshSFXButton()
        { 
            //_sfxToggleButton.Text.SetText($"SFX\n{(_sfxMuted ? "Off" : "On")}");
            soundOff.enabled = _sfxMuted;
            soundOn.enabled = !_sfxMuted;
        }
    }
}
