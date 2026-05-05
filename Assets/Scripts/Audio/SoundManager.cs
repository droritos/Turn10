using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Smart SoundManager — Singleton.
/// Call SoundManager.Instance.PlaySFX(SFX.PlaceShape) etc. from anywhere.
/// Mixer asset is loaded from Resources/ZenGridMixer.
/// </summary>
public class SoundManager : MonoBehaviour
{
    // --- ADDED: Public keys so all scripts use the exact same strings ---
    public const string PREF_MUSIC_MUTED = "MusicMuted";
    public const string PREF_SFX_MUTED   = "SFXMuted";

    // ──────────────────────────────────────────────────────────
    //  Enum Catalogue — add new SFX here freely
    // ──────────────────────────────────────────────────────────
    public enum SFXType
    {
        PlaceShape,
        SelectShape,
        ClearLine,
        MultiLineClear,
        TranquilityBonus,
        GameOver,
        ButtonClick,
        BackToTray,
        // Future entries go here ↓
    }

    // ──────────────────────────────────────────────────────────
    //  Inspector-visible fields (wired by SetupAudioEditor)
    // ──────────────────────────────────────────────────────────
    [Header("Audio Data")]
    public AudioData audioData;
    public AudioMixer mixer;

    [Header("Volume (0–1)")]
    [Range(0f, 1f)] public float masterVolume  = 1f;
    [Range(0f, 1f)] public float sfxVolume     = 1f;
    [Range(0f, 1f)] public float musicVolume   = 0.6f;

    // ──────────────────────────────────────────────────────────
    //  Private runtime
    // ──────────────────────────────────────────────────────────
    public static SoundManager Instance { get; private set; }

    private AudioSource _sfxSource;
    private AudioSource _musicSource;
    private int _currentMusicIndex = -1;

    // Pre-mute volume memory
    private float _savedMusicVolume = -1f;
    private float _savedSfxVolume   = -1f;

    // Exposed mixer group name constants (match the mixer asset)
    private const string MASTER_PARAM = "MasterVol";
    private const string SFX_PARAM    = "SFXVol";
    private const string MUSIC_PARAM  = "MusicVol";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildSources();

        if (mixer == null)
            mixer = Resources.Load<AudioMixer>("ZenGridMixer");
    }

    private void Start()
    {
        AssignMixerGroups();
        
        SyncVolumes(); 
        
        PlayNextMusic();
    }

    private void BuildSources()
    {
        // SFX source
        GameObject sfxGO = new GameObject("SFX_Source");
        sfxGO.transform.SetParent(transform);
        _sfxSource = sfxGO.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;

        // Music source
        GameObject musGO = new GameObject("Music_Source");
        musGO.transform.SetParent(transform);
        _musicSource = musGO.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = false; // We'll manage looping/playlist manually
    }
    private void Update()
    {
        // Simple playlist progression
        if (!_musicSource.isPlaying && audioData != null && audioData.musicPlaylist.Count > 0)
        {
            PlayNextMusic();
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Public API
    // ──────────────────────────────────────────────────────────

    public void PlaySFX(SFXType sfxType)
    {
        if (audioData == null) return;
        
        AudioClip clip = audioData.GetSFX(sfxType, out float vol, out float pitch);
        if (clip == null) { Debug.LogWarning($"[SoundManager] No clip assigned for SFX.{sfxType}"); return; }
        
        _sfxSource.pitch = pitch;
        _sfxSource.PlayOneShot(clip, vol);
    }

    public void PlaySFXClip(AudioClip clip)
    {
        if (clip == null) return;
        _sfxSource.pitch = 1f;
        _sfxSource.PlayOneShot(clip);
    }

    public void PlayNextMusic()
    {
        if (audioData == null || audioData.musicPlaylist.Count == 0) return;
        _currentMusicIndex = (_currentMusicIndex + 1) % audioData.musicPlaylist.Count;
        PlayMusicTrack(_currentMusicIndex);
    }

    public void PlayMusicTrack(int index)
    {
        if (audioData == null || index < 0 || index >= audioData.musicPlaylist.Count) return;
        AudioClip track = audioData.musicPlaylist[index];
        if (track == null) return;

        _currentMusicIndex = index;
        _musicSource.clip = track;
        _musicSource.Play();
    }

    public void StopMusic() => _musicSource.Stop();

    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        SetMixerParam(MASTER_PARAM, masterVolume);
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        SetMixerParam(SFX_PARAM, sfxVolume);
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        SetMixerParam(MUSIC_PARAM, musicVolume);
    }

    public void SetMusicMuted(bool mute)
    {
        if (mute)
        {
            _savedMusicVolume = musicVolume > 0f ? musicVolume : 0.6f;
            SetMusicVolume(0f);
        }
        else
        {
            SetMusicVolume(_savedMusicVolume > 0f ? _savedMusicVolume : 0.6f);
        }
    }

    public void SetSFXMuted(bool mute)
    {
        if (mute)
        {
            _savedSfxVolume = sfxVolume > 0f ? sfxVolume : 1f;
            SetSFXVolume(0f);
        }
        else
        {
            SetSFXVolume(_savedSfxVolume > 0f ? _savedSfxVolume : 1f);
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Private helpers
    // ──────────────────────────────────────────────────────────

    private void SyncVolumes()
    {
        // 1. Set the baseline volumes
        SetMixerParam(MASTER_PARAM, masterVolume);
        SetMixerParam(SFX_PARAM,    sfxVolume);
        SetMixerParam(MUSIC_PARAM,  musicVolume);

        // 2. CHECK PLAYER PREFS IMMEDIATELY ON WAKEUP
        bool isMusicMuted = PlayerPrefs.GetInt(PREF_MUSIC_MUTED, 0) == 1;
        bool isSfxMuted   = PlayerPrefs.GetInt(PREF_SFX_MUTED, 0) == 1;

        // 3. Apply those mutes
        SetMusicMuted(isMusicMuted);
        SetSFXMuted(isSfxMuted);
    }

    private void SetMixerParam(string param, float linearVolume)
    {
        if (mixer == null) return;
        float db = linearVolume > 0.0001f ? Mathf.Log10(linearVolume) * 20f : -80f;
        mixer.SetFloat(param, db);
    }

    private void AssignMixerGroups()
    {
        if (mixer == null) return;
        AudioMixerGroup[] groups = mixer.FindMatchingGroups(string.Empty);
        foreach (var g in groups)
        {
            if (g.name == "SFX")   _sfxSource.outputAudioMixerGroup   = g;
            if (g.name == "Music") _musicSource.outputAudioMixerGroup = g;
        }
    }
}