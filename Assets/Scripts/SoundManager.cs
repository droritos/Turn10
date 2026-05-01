using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Smart SoundManager — Singleton.
/// Call SoundManager.Instance.PlaySFX(SFX.PlaceShape) etc. from anywhere.
/// Mixer asset is loaded from Resources/ZenGridMixer.
/// </summary>
public class SoundManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────
    //  Enum Catalogue — add new SFX here freely
    // ──────────────────────────────────────────────────────────
    public enum SFX
    {
        PlaceShape,
        SelectShape,
        ClearLine,
        MultiLineClear,
        TranquilityBonus,
        GameOver,
        ButtonClick,
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

    // Exposed mixer group name constants (match the mixer asset)
    private const string MASTER_PARAM = "MasterVol";
    private const string SFX_PARAM    = "SFXVol";
    private const string MUSIC_PARAM  = "MusicVol";

    // ──────────────────────────────────────────────────────────
    //  Lifecycle
    // ──────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildSources();

        // Load mixer from Resources if not assigned in Inspector
        if (mixer == null)
            mixer = Resources.Load<AudioMixer>("ZenGridMixer");

        SyncVolumes();
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

    private void Start()
    {
        AssignMixerGroups();
        PlayNextMusic();
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

    /// <summary>Play a catalogued SFX.</summary>
    public void PlaySFX(SFX sfx)
    {
        if (audioData == null) return;
        
        AudioClip clip = audioData.GetSFX(sfx, out float vol, out float pitch);
        if (clip == null) { Debug.LogWarning($"[SoundManager] No clip assigned for SFX.{sfx}"); return; }
        
        _sfxSource.pitch = pitch;
        _sfxSource.PlayOneShot(clip, vol);
    }

    /// <summary>Play any clip directly through the SFX bus.</summary>
    public void PlaySFXClip(AudioClip clip)
    {
        if (clip == null) return;
        _sfxSource.pitch = 1f;
        _sfxSource.PlayOneShot(clip);
    }

    /// <summary>Cross-fade to the next background music track.</summary>
    public void PlayNextMusic()
    {
        if (audioData == null || audioData.musicPlaylist.Count == 0) return;
        _currentMusicIndex = (_currentMusicIndex + 1) % audioData.musicPlaylist.Count;
        PlayMusicTrack(_currentMusicIndex);
    }

    /// <summary>Play a specific music track index (0-based).</summary>
    public void PlayMusicTrack(int index)
    {
        if (audioData == null || index < 0 || index >= audioData.musicPlaylist.Count) return;
        AudioClip track = audioData.musicPlaylist[index];
        if (track == null) return;

        _currentMusicIndex = index;
        _musicSource.clip = track;
        _musicSource.Play();
    }

    /// <summary>Stop music immediately.</summary>
    public void StopMusic() => _musicSource.Stop();

    /// <summary>Set master volume (0–1).</summary>
    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        SetMixerParam(MASTER_PARAM, masterVolume);
    }

    /// <summary>Set SFX group volume (0–1).</summary>
    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        SetMixerParam(SFX_PARAM, sfxVolume);
    }

    /// <summary>Set Music group volume (0–1).</summary>
    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        SetMixerParam(MUSIC_PARAM, musicVolume);
    }

    // ──────────────────────────────────────────────────────────
    //  Private helpers
    // ──────────────────────────────────────────────────────────



    private void SyncVolumes()
    {
        SetMixerParam(MASTER_PARAM, masterVolume);
        SetMixerParam(SFX_PARAM,    sfxVolume);
        SetMixerParam(MUSIC_PARAM,  musicVolume);
    }

    private void SetMixerParam(string param, float linearVolume)
    {
        if (mixer == null) return;
        // Mixer uses decibels; convert linear [0,1] → dB with -80 floor
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
