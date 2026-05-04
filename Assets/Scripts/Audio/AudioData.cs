using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AudioData", menuName = "ZenGrid/Audio Data")]
public class AudioData : ScriptableObject
{
    [System.Serializable]
    public struct SFXEntry
    {
        public SoundManager.SFXType key;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
        [Range(0.5f, 2f)] public float pitch;

        public SFXEntry(SoundManager.SFXType key)
        {
            this.key = key;
            this.clip = null;
            this.volume = 1f;
            this.pitch = 1f;
        }
    }

    public List<SFXEntry> sfxList = new List<SFXEntry>();
    public List<AudioClip> musicPlaylist = new List<AudioClip>();

    public AudioClip GetSFX(SoundManager.SFXType key, out float vol, out float pitch)
    {
        vol = 1f;
        pitch = 1f;
        foreach (var entry in sfxList)
        {
            if (entry.key == key)
            {
                vol = entry.volume;
                pitch = entry.pitch;
                return entry.clip;
            }
        }
        return null;
    }
}
