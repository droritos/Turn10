using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using DG.Tweening;
public class ZenGridManager : MonoBehaviour
{
    public static ZenGridManager Instance;

    [Header("Grid Settings")]
    public int columns = 10;
    public int rows = 10;
    public GameObject cellPrefab;
    public Transform gridContainer;
    
    [Header("Shapes Settings")]
    public ShapeDatabase shapeDatabase;
    public GameObject draggableShapePrefab;
    public Transform[] trayPositions;
    
    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverScreen;
    public TextMeshProUGUI finalScoreText;
    
    [Header("VFX")]
    public ParticleSystem petalParticles;
    
    // Grid state
    private Color?[,] gridState;
    private Image[,] gridImages;
    private Image[,] lotusOverlays;
    private bool[,] isLotusCell;
    
    private List<DraggableShape> currentTrayShapes = new List<DraggableShape>();
    private int score = 0;
    public int currentPhase = 1;
    
    // Lotus Protocol
    private int turnsSinceLastLotus = 0;
    public int turnsBetweenLotus = 5;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeGrid();
        SpawnTrayShapes();
        UpdateScore(0);
        gameOverScreen.SetActive(false);
    }

    private void InitializeGrid()
    {
        gridState = new Color?[columns, rows];
        gridImages = new Image[columns, rows];
        lotusOverlays = new Image[columns, rows];
        isLotusCell = new bool[columns, rows];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                GameObject cell = Instantiate(cellPrefab, gridContainer);
                
                // Tint the cell background so it's visible against the board
                Image cellBg = cell.GetComponent<Image>();
                if (cellBg != null) {
                    cellBg.color = new Color(0.9f, 0.9f, 0.9f, 0.8f); // Slightly dark and transparent
                }
                
                gridImages[c, r] = cell.transform.Find("Fill").GetComponent<Image>();
                gridImages[c, r].gameObject.SetActive(false);
                
                // Create Lotus Overlay
                GameObject overlay = new GameObject("LotusOverlay");
                overlay.transform.SetParent(cell.transform, false);
                Image img = overlay.AddComponent<Image>();
                img.color = new Color(1f, 0.4f, 0.8f, 0.5f); // Pinkish glowing tint
                RectTransform rt = img.rectTransform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                overlay.SetActive(false);
                lotusOverlays[c, r] = img;
            }
        }
    }

    public void SpawnTrayShapes()
    {
        currentTrayShapes.Clear();
        List<ShapeData> availableShapes = new List<ShapeData>();
        foreach (var shape in shapeDatabase.shapes)
        {
            if (shape.minPhase <= currentPhase)
            {
                availableShapes.Add(shape);
            }
        }
        
        if (availableShapes.Count == 0) return;

        for (int i = 0; i < trayPositions.Length; i++)
        {
            ShapeData randomShape = availableShapes[Random.Range(0, availableShapes.Count)];
            GameObject shapeObj = Instantiate(draggableShapePrefab, trayPositions[i]);
            DraggableShape draggable = shapeObj.GetComponent<DraggableShape>();
            draggable.Initialize(randomShape, trayPositions[i]);
            currentTrayShapes.Add(draggable);
        }
    }

    public bool CanPlaceShape(ShapeData shape, int gridX, int gridY)
    {
        for (int y = 0; y < shape.height; y++)
        {
            for (int x = 0; x < shape.width; x++)
            {
                if (shape.GetCell(x, y) == 1)
                {
                    int checkX = gridX + x;
                    int checkY = gridY + y;

                    if (checkX < 0 || checkX >= columns || checkY < 0 || checkY >= rows)
                        return false;

                    if (gridState[checkX, checkY].HasValue)
                        return false;
                }
            }
        }
        return true;
    }

    private List<Vector2Int> currentGhostCells = new List<Vector2Int>();

    public void ShowGhost(ShapeData shape, int gridX, int gridY)
    {
        ClearGhost();
        if (!CanPlaceShape(shape, gridX, gridY)) return;

        for (int y = 0; y < shape.height; y++)
        {
            for (int x = 0; x < shape.width; x++)
            {
                if (shape.GetCell(x, y) == 1)
                {
                    int placeX = gridX + x;
                    int placeY = gridY + y;
                    
                    gridImages[placeX, placeY].color = new Color(shape.color.r, shape.color.g, shape.color.b, 0.4f);
                    gridImages[placeX, placeY].gameObject.SetActive(true);
                    currentGhostCells.Add(new Vector2Int(placeX, placeY));
                }
            }
        }
    }

    public void ClearGhost()
    {
        foreach (var cell in currentGhostCells)
        {
            if (!gridState[cell.x, cell.y].HasValue)
            {
                gridImages[cell.x, cell.y].gameObject.SetActive(false);
            }
            else
            {
                gridImages[cell.x, cell.y].color = gridState[cell.x, cell.y].Value;
            }
        }
        currentGhostCells.Clear();
    }

    public void PlaceShape(ShapeData shape, int gridX, int gridY)
    {
        int pointsGained = 0;
        for (int y = 0; y < shape.height; y++)
        {
            for (int x = 0; x < shape.width; x++)
            {
                if (shape.GetCell(x, y) == 1)
                {
                    int placeX = gridX + x;
                    int placeY = gridY + y;
                    
                    gridState[placeX, placeY] = shape.color;
                    gridImages[placeX, placeY].color = shape.color;
                    gridImages[placeX, placeY].gameObject.SetActive(true);
                    
                    // Juice: pop in!
                    gridImages[placeX, placeY].rectTransform.localScale = Vector3.zero;
                    gridImages[placeX, placeY].rectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                    
                    pointsGained += 10;
                    
                    if (JuiceManager.Instance != null)
                    {
                        JuiceManager.Instance.PlayPetals(gridImages[placeX, placeY].transform.position, shape.color);
                        JuiceManager.Instance.PopBlock(gridImages[placeX, placeY].rectTransform);
                    }
                }
            }
        }
        
        if (JuiceManager.Instance != null)
        {
            JuiceManager.Instance.ScreenShake(0.1f, 5f);
        }
        
        UpdateScore(pointsGained);
        
        // Lotus Protocol: Spread existing Lotuses
        SpreadLotus();
        
        // Lotus Protocol: Spawn new Lotus Seed if phase >= 2
        turnsSinceLastLotus++;
        if (currentPhase >= 2 && turnsSinceLastLotus >= turnsBetweenLotus)
        {
            SpawnLotusSeed();
        }

        CheckLines();
    }

    private void SpreadLotus()
    {
        List<Vector2Int> newLotusCells = new List<Vector2Int>();
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (isLotusCell[x, y])
                {
                    // Spread to one adjacent filled block randomly
                    List<Vector2Int> neighbors = new List<Vector2Int>();
                    Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                    foreach (var d in dirs)
                    {
                        int nx = x + d.x; int ny = y + d.y;
                        if (nx >= 0 && nx < columns && ny >= 0 && ny < rows)
                        {
                            if (gridState[nx, ny].HasValue && !isLotusCell[nx, ny])
                            {
                                neighbors.Add(new Vector2Int(nx, ny));
                            }
                        }
                    }
                    if (neighbors.Count > 0)
                    {
                        // 30% chance to spread per turn per lotus
                        if (Random.value < 0.3f)
                        {
                            newLotusCells.Add(neighbors[Random.Range(0, neighbors.Count)]);
                        }
                    }
                }
            }
        }

        foreach (var cell in newLotusCells)
        {
            SetLotus(cell.x, cell.y);
        }
    }

    private void SpawnLotusSeed()
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (!gridState[x, y].HasValue)
                {
                    emptyCells.Add(new Vector2Int(x, y));
                }
            }
        }

        if (emptyCells.Count > 0)
        {
            Vector2Int pos = emptyCells[Random.Range(0, emptyCells.Count)];
            // Spawn a Lotus block
            gridState[pos.x, pos.y] = Color.magenta; // Base color for lotus
            gridImages[pos.x, pos.y].color = Color.magenta;
            gridImages[pos.x, pos.y].gameObject.SetActive(true);
            gridImages[pos.x, pos.y].rectTransform.localScale = Vector3.zero;
            gridImages[pos.x, pos.y].rectTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutElastic);
            SetLotus(pos.x, pos.y);
            turnsSinceLastLotus = 0;
            
            if (JuiceManager.Instance != null)
            {
                JuiceManager.Instance.PopBlock(gridImages[pos.x, pos.y].rectTransform);
                JuiceManager.Instance.PlayPetals(gridImages[pos.x, pos.y].transform.position, Color.magenta);
            }
        }
    }

    private void SetLotus(int x, int y)
    {
        isLotusCell[x, y] = true;
        
        // Actually replace the underlying color instead of an overlay
        gridState[x, y] = Color.magenta;
        gridImages[x, y].DOColor(Color.magenta, 0.4f).SetEase(Ease.InOutSine);
        
        if (lotusOverlays != null && lotusOverlays.Length > 0 && lotusOverlays[x,y] != null) {
            lotusOverlays[x, y].gameObject.SetActive(false); // remove overlay if it exists
        }
    }

    public void OnShapePlaced(DraggableShape shape)
    {
        currentTrayShapes.Remove(shape);
        Destroy(shape.gameObject);

        if (currentTrayShapes.Count == 0)
        {
            SpawnTrayShapes();
        }

        CheckGameOver();
    }

    private void CheckLines()
    {
        List<int> rowsToClear = new List<int>();
        List<int> colsToClear = new List<int>();

        // Check rows
        for (int y = 0; y < rows; y++)
        {
            bool full = true;
            for (int x = 0; x < columns; x++)
            {
                if (!gridState[x, y].HasValue)
                {
                    full = false;
                    break;
                }
            }
            if (full) rowsToClear.Add(y);
        }

        // Check columns
        for (int x = 0; x < columns; x++)
        {
            bool full = true;
            for (int y = 0; y < rows; y++)
            {
                if (!gridState[x, y].HasValue)
                {
                    full = false;
                    break;
                }
            }
            if (full) colsToClear.Add(x);
        }

        // Keep track of lotus explosions
        List<Vector2Int> explosionCenters = new List<Vector2Int>();
        
        // Initial identify cells to clear
        HashSet<Vector2Int> cellsToClear = new HashSet<Vector2Int>();
        foreach (int y in rowsToClear)
        {
            for (int x = 0; x < columns; x++)
            {
                cellsToClear.Add(new Vector2Int(x, y));
            }
        }
        foreach (int x in colsToClear)
        {
            for (int y = 0; y < rows; y++)
            {
                cellsToClear.Add(new Vector2Int(x, y));
            }
        }

        // Check for lotuses in the cleared lines
        foreach (var cell in cellsToClear)
        {
            if (isLotusCell[cell.x, cell.y])
            {
                explosionCenters.Add(cell);
            }
        }

        // Add explosion radius to cells to clear
        int emptySpacesCaught = 0;

        foreach (var center in explosionCenters)
        {
            for (int ex = -1; ex <= 1; ex++)
            {
                for (int ey = -1; ey <= 1; ey++)
                {
                    int nx = center.x + ex;
                    int ny = center.y + ey;
                    if (nx >= 0 && nx < columns && ny >= 0 && ny < rows)
                    {
                        if (gridState[nx, ny].HasValue)
                        {
                            cellsToClear.Add(new Vector2Int(nx, ny));
                        }
                        else
                        {
                            // Empty space caught in blast - TRANQUILITY BONUS
                            emptySpacesCaught++;
                            
                            // Visual indication of tranquility bonus on empty cell
                            if (JuiceManager.Instance != null) {
                                Vector3 pos = gridImages[nx, ny].transform.position;
                                JuiceManager.Instance.SpawnFloatingText(pos, "TRANQUIL!", new Color(0.4f, 1f, 0.8f));
                                // Flash the cell background
                                Image cellBg = gridImages[nx, ny].transform.parent.GetComponent<Image>();
                                if (cellBg != null) {
                                    cellBg.DOColor(Color.white, 0.2f).SetLoops(2, LoopType.Yoyo).OnComplete(() => {
                                        cellBg.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        // Clear them
        int clearedCells = 0;
        Vector3 lastClearPos = Vector3.zero;
        
        foreach (var cell in cellsToClear)
        {
            if (gridState[cell.x, cell.y].HasValue)
            {
                gridState[cell.x, cell.y] = null;
                isLotusCell[cell.x, cell.y] = false;
                
                Image img = gridImages[cell.x, cell.y];
                lastClearPos = img.transform.position;
                
                // DOTween juice out!
                img.rectTransform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
                    img.gameObject.SetActive(false);
                    img.rectTransform.localScale = Vector3.one; // reset for next use
                });
                
                lotusOverlays[cell.x, cell.y].gameObject.SetActive(false);
                clearedCells++;
                
                if (JuiceManager.Instance != null) {
                    // If it was a lotus cell, play the big explosion
                    if (explosionCenters.Contains(cell)) {
                        JuiceManager.Instance.PlayExplosion(img.transform.position, Color.magenta);
                    } else {
                        JuiceManager.Instance.PlayPetals(img.transform.position, Color.white);
                    }
                }
            }
        }

        if (clearedCells > 0)
        {
            int basePoints = clearedCells * 100; // Multiply by 100 for bigger numbers
            int points = basePoints;
            
            // Tranquility Bonus for lotus explosions
            if (explosionCenters.Count > 0)
            {
                int bonusPoints = emptySpacesCaught * 500; // Massive bonus for empty spaces
                points += bonusPoints;
                
                if (JuiceManager.Instance != null) {
                    // Big shake is handled in PlayExplosion, but we can spawn the total text here
                    JuiceManager.Instance.SpawnFloatingText(lastClearPos, "+" + points, new Color(1f, 0.5f, 0.8f));
                }
            }
            else
            {
                if (JuiceManager.Instance != null) {
                    JuiceManager.Instance.ScreenShake(0.3f, 15f);
                    JuiceManager.Instance.SpawnFloatingText(lastClearPos, "+" + points, Color.white);
                }
            }
            UpdateScore(points);
        }
    }

    private void CheckGameOver()
    {
        bool canPlaceAny = false;
        foreach (var shape in currentTrayShapes)
        {
            if (shape == null) continue;
            
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (CanPlaceShape(shape.shapeData, x, y))
                    {
                        canPlaceAny = true;
                        break;
                    }
                }
                if (canPlaceAny) break;
            }
            if (canPlaceAny) break;
        }

        if (!canPlaceAny && currentTrayShapes.Count > 0)
        {
            GameOver();
        }
    }

    private void UpdateScore(int add)
    {
        score += add;
        if (scoreText != null)
            scoreText.text = score.ToString();
            
        // Phase logic
        int oldPhase = currentPhase;
        currentPhase = (score / 150) + 1; // Testing value: 150 instead of 3000 to see it faster
        if (currentPhase != oldPhase && currentPhase >= 2 && turnsSinceLastLotus == 0)
        {
            // Spawn lotus immediately on phase transition
            SpawnLotusSeed();
        }
    }

    private void GameOver()
    {
        gameOverScreen.SetActive(true);
        if (finalScoreText != null)
            finalScoreText.text = score.ToString();
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}