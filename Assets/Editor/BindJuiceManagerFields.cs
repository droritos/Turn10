using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class BindJuiceManagerFields
{
    static BindJuiceManagerFields()
    {
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        EditorApplication.delayCall -= Run;

        if (EditorPrefs.GetBool("HasBoundJuiceFields", false))
            return;

        JuiceManager jm = Object.FindAnyObjectByType<JuiceManager>();
        if (jm != null)
        {
            GameObject ftPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FloatingText.prefab");
            jm.floatingTextPrefab = ftPrefab;

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                jm.canvasTransform = canvas.transform;
            }

            if (jm.sfxSource == null)
            {
                jm.sfxSource = jm.gameObject.AddComponent<AudioSource>();
            }

            EditorUtility.SetDirty(jm);
            Debug.Log("JuiceManager fields bound successfully!");
        }

        EditorPrefs.SetBool("HasBoundJuiceFields", true);
    }
}
