using UnityEngine;
using UnityEditor;
using TMPro;

[InitializeOnLoad]
public class SetupFloatingTextPrefab
{
    static SetupFloatingTextPrefab()
    {
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        EditorApplication.delayCall -= Run;

        if (EditorPrefs.GetBool("HasSetupFloatingTextPrefab", false))
            return;

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        GameObject ftPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FloatingText.prefab");
        if (ftPrefab == null)
        {
            GameObject tempObj = new GameObject("FloatingText");
            RectTransform rt = tempObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 100);
            
            TextMeshProUGUI textMesh = tempObj.AddComponent<TextMeshProUGUI>();
            textMesh.text = "+100";
            textMesh.fontSize = 70;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.fontStyle = FontStyles.Bold;
            textMesh.color = Color.white;
            
            // Add outline/shadow juice to the text
            textMesh.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
            textMesh.outlineWidth = 0.2f;
            textMesh.outlineColor = new Color(0, 0, 0, 0.5f);

            FloatingText ftScript = tempObj.AddComponent<FloatingText>();
            ftScript.textMesh = textMesh;

            PrefabUtility.SaveAsPrefabAsset(tempObj, "Assets/Prefabs/FloatingText.prefab");
            GameObject.DestroyImmediate(tempObj);
            
            Debug.Log("FloatingText Prefab created successfully!");
        }

        EditorPrefs.SetBool("HasSetupFloatingTextPrefab", true);
    }
}
