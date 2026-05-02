#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

namespace ZenGrid.Editor
{
    public static class SetupDebugHUD
    {
        [MenuItem("ZenGrid/Setup/Create Debug HUD")]
        public static void CreateHUD()
        {
            // 1. Create Canvas if needed
            GameObject canvasObj = GameObject.Find("DebugCanvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("DebugCanvas");
                Canvas c = canvasObj.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                c.sortingOrder = 999; // Topmost
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // 2. Create Root
            GameObject root = new GameObject("HUD_Root");
            root.transform.SetParent(canvasObj.transform, false);
            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0, 1);
            rootRT.anchorMax = new Vector2(0, 1);
            rootRT.pivot = new Vector2(0, 1);
            rootRT.anchoredPosition = new Vector2(20, -20);
            rootRT.sizeDelta = new Vector2(250, 150);

            // Background
            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            // 3. FPS Text
            GameObject fpsObj = new GameObject("FPS_Text");
            fpsObj.transform.SetParent(root.transform, false);
            TextMeshProUGUI fpsText = fpsObj.AddComponent<TextMeshProUGUI>();
            fpsText.fontSize = 18;
            fpsText.alignment = TextAlignmentOptions.Left;
            RectTransform fpsRT = fpsObj.GetComponent<RectTransform>();
            fpsRT.anchorMin = new Vector2(0, 1);
            fpsRT.anchorMax = new Vector2(1, 1);
            fpsRT.pivot = new Vector2(0, 1);
            fpsRT.anchoredPosition = new Vector2(10, -10);
            fpsRT.sizeDelta = new Vector2(-20, 30);

            // 4. Info Text
            GameObject infoObj = new GameObject("Info_Text");
            infoObj.transform.SetParent(root.transform, false);
            TextMeshProUGUI infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.fontSize = 14;
            infoText.alignment = TextAlignmentOptions.TopLeft;
            infoText.color = Color.white;
            RectTransform infoRT = infoObj.GetComponent<RectTransform>();
            infoRT.anchorMin = new Vector2(0, 0);
            infoRT.anchorMax = new Vector2(1, 1);
            infoRT.pivot = new Vector2(0.5f, 0.5f);
            infoRT.anchoredPosition = new Vector2(0, -35); // Below FPS
            infoRT.sizeDelta = new Vector2(-20, -50);

            // 5. Component
            GameObject managerObj = GameObject.Find("GameManager") ?? new GameObject("DebugManager");
            DebugHUD hud = managerObj.GetComponent<DebugHUD>() ?? managerObj.AddComponent<DebugHUD>();
            
            // Reflection or direct assignment? Direct since it's the same assembly
            var propRoot = typeof(DebugHUD).GetField("_displayRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var propFps = typeof(DebugHUD).GetField("_fpsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var propInfo = typeof(DebugHUD).GetField("_infoText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            propRoot.SetValue(hud, root);
            propFps.SetValue(hud, fpsText);
            propInfo.SetValue(hud, infoText);

            Selection.activeGameObject = root;
            Debug.Log("Debug HUD Created! Press '~' (tilde) to toggle it.");
        }
    }
}
#endif
