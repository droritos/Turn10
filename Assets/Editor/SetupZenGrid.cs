using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SetupZenGrid : EditorWindow
{
    [MenuItem("ZenGrid/Setup Scene")]
    public static void Run()
    {
        // 1. Create the ShapeDatabase asset
        ShapeDatabase db = ScriptableObject.CreateInstance<ShapeDatabase>();
        
        db.shapes.Add(CreateShape("Dot", "#ffb7b2", new int[]{1}, 1, 1));
        db.shapes.Add(CreateShape("Square", "#e2f0cb", new int[]{1,1,1,1}, 2, 2));
        db.shapes.Add(CreateShape("BigSquare", "#b5ead7", new int[]{1,1,1,1,1,1,1,1,1}, 3, 3));
        db.shapes.Add(CreateShape("Line4", "#c7ceea", new int[]{1,1,1,1}, 4, 1));
        db.shapes.Add(CreateShape("Line5", "#ffdac1", new int[]{1,1,1,1,1}, 5, 1));
        db.shapes.Add(CreateShape("Line3", "#a0e8af", new int[]{1,1,1}, 3, 1));
        db.shapes.Add(CreateShape("Line2", "#ff9aa2", new int[]{1,1}, 2, 1));
        db.shapes.Add(CreateShape("Corner", "#ffdfba", new int[]{1,0,1,1}, 2, 2));
        db.shapes.Add(CreateShape("BigCorner", "#bae1ff", new int[]{1,0,0, 1,0,0, 1,1,1}, 3, 3));
        db.shapes.Add(CreateShape("T", "#e0bbe4", new int[]{1,1,1, 0,1,0}, 3, 2));
        
        if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
        AssetDatabase.CreateAsset(db, "Assets/Settings/ShapeDatabase.asset");
        AssetDatabase.SaveAssets();

        // 2. Set up Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.9f, 0.95f, 1f); 
        RectTransform bgRt = bgImg.rectTransform;
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        
        // Header Text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "ZenGrid";
        titleTxt.fontSize = 120;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = new Color(0.1f, 0.5f, 0.4f);
        RectTransform titleRt = titleTxt.rectTransform;
        titleRt.anchorMin = new Vector2(0, 1); titleRt.anchorMax = new Vector2(1, 1);
        titleRt.sizeDelta = new Vector2(0, 200);
        titleRt.anchoredPosition = new Vector2(0, -150);
        
        // Score Text
        GameObject scoreObj = new GameObject("Score");
        scoreObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI scoreTxt = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreTxt.text = "0";
        scoreTxt.fontSize = 80;
        scoreTxt.alignment = TextAlignmentOptions.Right;
        scoreTxt.color = new Color(0.8f, 0.2f, 0.5f);
        RectTransform scoreRt = scoreTxt.rectTransform;
        scoreRt.anchorMin = new Vector2(1, 1); scoreRt.anchorMax = new Vector2(1, 1);
        scoreRt.sizeDelta = new Vector2(400, 200);
        scoreRt.anchoredPosition = new Vector2(-150, -150);
        
        // Grid Container
        GameObject gridObj = new GameObject("GridContainer");
        gridObj.transform.SetParent(canvasObj.transform, false);
        Image gridBg = gridObj.AddComponent<Image>();
        gridBg.color = new Color(1, 1, 1, 0.3f);
        RectTransform gridRt = gridBg.rectTransform;
        gridRt.sizeDelta = new Vector2(1000, 1000); 
        gridRt.anchoredPosition = new Vector2(0, 150);
        GridLayoutGroup gridLayout = gridObj.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(96, 96);
        gridLayout.spacing = new Vector2(4, 4);
        
        // Tray Container
        GameObject trayObj = new GameObject("TrayContainer");
        trayObj.transform.SetParent(canvasObj.transform, false);
        HorizontalLayoutGroup trayLayout = trayObj.AddComponent<HorizontalLayoutGroup>();
        trayLayout.childAlignment = TextAnchor.MiddleCenter;
        trayLayout.childControlWidth = false;
        trayLayout.spacing = 20;
        RectTransform trayRt = trayObj.GetComponent<RectTransform>();
        trayRt.anchorMin = new Vector2(0, 0); trayRt.anchorMax = new Vector2(1, 0);
        trayRt.sizeDelta = new Vector2(0, 400);
        trayRt.anchoredPosition = new Vector2(0, 450);
        
        // Setup Tray Prefab
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        GameObject trayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TraySlot.prefab");
        if (trayPrefab == null) {
            GameObject tempTray = new GameObject("TraySlot");
            tempTray.AddComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
            trayPrefab = PrefabUtility.SaveAsPrefabAsset(tempTray, "Assets/Prefabs/TraySlot.prefab");
            GameObject.DestroyImmediate(tempTray);
        }

        Transform[] trays = new Transform[3];
        for (int i = 0; i < 3; i++) {
            GameObject t = (GameObject)PrefabUtility.InstantiatePrefab(trayPrefab);
            t.name = "Tray" + i;
            t.transform.SetParent(trayObj.transform, false);
            trays[i] = t.transform;
        }

        // Game Over Screen
        GameObject goObj = new GameObject("GameOverScreen");
        goObj.transform.SetParent(canvasObj.transform, false);
        Image goBg = goObj.AddComponent<Image>();
        goBg.color = new Color(1, 1, 1, 0.9f);
        RectTransform goRt = goBg.rectTransform;
        goRt.anchorMin = Vector2.zero; goRt.anchorMax = Vector2.one;
        goRt.offsetMin = Vector2.zero; goRt.offsetMax = Vector2.zero;
        
        GameObject goTextObj = new GameObject("GO_Title");
        goTextObj.transform.SetParent(goObj.transform, false);
        TextMeshProUGUI goText = goTextObj.AddComponent<TextMeshProUGUI>();
        goText.text = "Garden Full";
        goText.fontSize = 150;
        goText.alignment = TextAlignmentOptions.Center;
        goText.rectTransform.anchoredPosition = new Vector2(0, 200);
        
        GameObject finalScoreObj = new GameObject("FinalScore");
        finalScoreObj.transform.SetParent(goObj.transform, false);
        TextMeshProUGUI finalScoreText = finalScoreObj.AddComponent<TextMeshProUGUI>();
        finalScoreText.text = "0";
        finalScoreText.fontSize = 120;
        finalScoreText.alignment = TextAlignmentOptions.Center;
        finalScoreText.rectTransform.anchoredPosition = new Vector2(0, 0);

        Button restartBtn = new GameObject("RestartBtn").AddComponent<Button>();
        restartBtn.transform.SetParent(goObj.transform, false);
        restartBtn.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.6f);
        restartBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 150);
        restartBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -250);
        TextMeshProUGUI btnTxt = new GameObject("Text").AddComponent<TextMeshProUGUI>();
        btnTxt.transform.SetParent(restartBtn.transform, false);
        btnTxt.text = "RAKE THE SAND";
        btnTxt.alignment = TextAlignmentOptions.Center;
        btnTxt.color = Color.white;
        btnTxt.fontSize = 60;
        RectTransform btnTxtRt = btnTxt.rectTransform;
        btnTxtRt.anchorMin = Vector2.zero; btnTxtRt.anchorMax = Vector2.one;
        btnTxtRt.offsetMin = Vector2.zero; btnTxtRt.offsetMax = Vector2.zero;

        goObj.SetActive(false);

        // Manager setup
        GameObject managerObj = new GameObject("GameManager");
        ZenGridManager manager = managerObj.AddComponent<ZenGridManager>();
        manager.shapeDatabase = db;
        manager.gridContainer = gridObj.transform;
        manager.scoreText = scoreTxt;
        manager.gameOverScreen = goObj;
        manager.finalScoreText = finalScoreText;
        manager.trayPositions = trays;

        UnityEditor.Events.UnityEventTools.AddPersistentListener(restartBtn.onClick, manager.RestartGame);

        // Create Prefabs
        GameObject cellPref = new GameObject("GridCell");
        cellPref.AddComponent<RectTransform>().sizeDelta = new Vector2(96, 96);
        Image cellBg = cellPref.AddComponent<Image>();
        cellBg.color = new Color(1, 1, 1, 0.5f);
        GameObject cellFill = new GameObject("Fill");
        cellFill.transform.SetParent(cellPref.transform, false);
        Image fillImg = cellFill.AddComponent<Image>();
        RectTransform fillRt = fillImg.rectTransform;
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = new Vector2(4, 4); fillRt.offsetMax = new Vector2(-4, -4);
        
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        GameObject savedCell = PrefabUtility.SaveAsPrefabAsset(cellPref, "Assets/Prefabs/GridCell.prefab");
        manager.cellPrefab = savedCell;
        GameObject.DestroyImmediate(cellPref);

        GameObject dragPref = new GameObject("DraggableShape");
        dragPref.AddComponent<RectTransform>();
        dragPref.AddComponent<CanvasGroup>();
        dragPref.AddComponent<DraggableShape>();
        GameObject savedDrag = PrefabUtility.SaveAsPrefabAsset(dragPref, "Assets/Prefabs/DraggableShape.prefab");
        manager.draggableShapePrefab = savedDrag;
        GameObject.DestroyImmediate(dragPref);
        
        // Add EventSystem if missing
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null) {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Debug.Log("Scene Setup Complete");
    }

    private static ShapeData CreateShape(string name, string hexColor, int[] matrix, int w, int h) {
        ShapeData sd = new ShapeData();
        sd.name = name;
        sd.width = w;
        sd.height = h;
        sd.matrix = matrix;
        ColorUtility.TryParseHtmlString(hexColor, out sd.color);
        return sd;
    }
}