using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class FixInput : EditorWindow {
    [MenuItem("ZenGrid/Fix Input System")]
    public static void Run() {
        EventSystem es = Object.FindAnyObjectByType<EventSystem>();
        if (es != null) {
            StandaloneInputModule standalone = es.GetComponent<StandaloneInputModule>();
            if (standalone != null) {
                Object.DestroyImmediate(standalone);
#if ENABLE_INPUT_SYSTEM
                es.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("Replaced StandaloneInputModule with InputSystemUIInputModule.");
#else
                Debug.LogWarning("ENABLE_INPUT_SYSTEM is not defined, could not add InputSystemUIInputModule.");
#endif
            } else {
                Debug.Log("StandaloneInputModule not found on EventSystem.");
            }
        } else {
            Debug.LogWarning("EventSystem not found in scene.");
        }
    }
}