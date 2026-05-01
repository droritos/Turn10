using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor.Audio;
#endif

/// <summary>
/// ZenGrid > Setup Audio
/// Creates:
///   • Assets/Resources/ZenGridMixer.mixer  (Master → SFX, Music groups + exposed params)
///   • SoundManager GameObject in the scene  (wired up, ready to receive clips)
/// </summary>
public class SetupAudioEditor : EditorWindow
{
    [MenuItem("ZenGrid/Setup Audio")]
    public static void Run()
    {
        // ── 1. Audio Mixer ──────────────────────────────────────────────
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Resources/ZenGridMixer.mixer");

        if (mixer == null)
        {
            // AudioMixerController is internal; we create via menu and then move
            mixer = CreateMixerViaReflection();

            if (mixer == null)
            {
                Debug.LogError("[SetupAudio] Could not create AudioMixer via reflection. " +
                               "Please create it manually: Assets > Create > Audio > Audio Mixer, " +
                               "name it 'ZenGridMixer', save to Assets/Resources/, " +
                               "add 'SFX' and 'Music' child groups, and expose 'MasterVol', 'SFXVol', 'MusicVol' params.");
            }
        }

        // ── 2. SoundManager GameObject ──────────────────────────────────
        SoundManager existing = Object.FindAnyObjectByType<SoundManager>();
        if (existing != null)
        {
            Debug.LogWarning("[SetupAudio] SoundManager already exists in scene. Skipping creation.");
            BindMixer(existing, mixer);
            return;
        }

        GameObject go = new GameObject("SoundManager");
        SoundManager sm = go.AddComponent<SoundManager>();
        BindMixer(sm, mixer);

        EditorUtility.SetDirty(go);
        Debug.Log("[SetupAudio] SoundManager created! Drag your audio clips into the Inspector slots, then play!");
    }

    // ────────────────────────────────────────────────────────────────────
    //  Reflection-based mixer creation (Unity internal API)
    // ────────────────────────────────────────────────────────────────────
    private static AudioMixer CreateMixerViaReflection()
    {
        try
        {
            // Use the same internal method the Unity menu item calls
            var asm = System.Reflection.Assembly.Load("UnityEditor");
            var t   = asm.GetType("UnityEditor.AudioMixerController");
            if (t == null) return null;

            // Create the controller (derives from AudioMixer)
            var ctrl = ScriptableObject.CreateInstance(t);
            if (ctrl == null) return null;

            // Save as asset
            string path = "Assets/Resources/ZenGridMixer.mixer";
            AssetDatabase.CreateAsset(ctrl, path);

            // Add child groups: SFX and Music
            var masterGroup = t.GetProperty("masterGroup")?.GetValue(ctrl);
            if (masterGroup != null)
            {
                var addGroup = t.GetMethod("CreateNewGroup");
                if (addGroup != null)
                {
                    addGroup.Invoke(ctrl, new object[] { "SFX",   masterGroup });
                    addGroup.Invoke(ctrl, new object[] { "Music", masterGroup });
                }
            }

            // Expose volume parameters
            ExposeParam(t, ctrl, "MasterVol");
            ExposeParam(t, ctrl, "SFXVol");
            ExposeParam(t, ctrl, "MusicVol");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return (AudioMixer)(object)ctrl;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[SetupAudio] Reflection mixer creation failed: " + e.Message);
            return null;
        }
    }

    private static void ExposeParam(System.Type t, ScriptableObject ctrl, string paramName)
    {
        try
        {
            var addParam = t.GetMethod("AddExposedParameter");
            addParam?.Invoke(ctrl, new object[] { paramName });
        }
        catch { /* graceful skip */ }
    }

    private static void BindMixer(SoundManager sm, AudioMixer mixer)
    {
        sm.mixer = mixer;
        EditorUtility.SetDirty(sm);
    }
}
