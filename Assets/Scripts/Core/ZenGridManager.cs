using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using ZenGrid.UI;

namespace ZenGrid
{
    public class ZenGridManager : MonoBehaviour
    {
        public UnityAction ShapePlacedEvent;
        public static ZenGridManager Instance;

        public bool isGameActive = false;

        private void Awake()
        {
            Instance = this;
            
            // Mobile Optimizations
            Application.targetFrameRate = 120;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void Start()
        {
            GridSystem.Instance.InitializeGrid();
            ScoreManager.Instance.UpdateScore(0);
        }

        public void StartGame()
        {
            isGameActive = true;
            
            // Ensure grid is clean and has correct Zen visuals
            GridSystem.Instance.InitializeGrid();

            if (ShapeManager.Instance.ShapesInTray == 0)
            {
                ShapeManager.Instance.SpawnTrayShapes(ScoreManager.Instance.CurrentPhase);
            }
        }

        [ContextMenu("Game Over")]
        public void GameOver()
        {
            isGameActive = false;

            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.SFXType.GameOver);

            if (GameOverUI.Instance != null)
                GameOverUI.Instance.Populate(ScoreManager.Instance.Score, ScoreManager.Instance.BestScore);

            if (MenuManager.Instance != null)
                MenuManager.Instance.OpenMenu(MenuType.GameOver);
        }

        public void ToggleSpectateInGameOver()
        {
            MenuManager.Instance.GetMenu(MenuType.Gameplay).ToggleSpectate();
        }

        public void OnShapePlaced(DraggableShape shape, int gridX, int gridY)
        {
            try
            {
                // 1. Logic Placement (GridSystem now automatically handles skipping the PopBlock if a clear happens)
                GridSystem.Instance.PlaceShape(shape.ShapeData, gridX, gridY);
                
                // 2. Score for placing
                ScoreManager.Instance.UpdateScore(10);

                // 3. Audio/Visuals
                if (JuiceManager.Instance != null) JuiceManager.Instance.ScreenShake(0.1f, 5f);
                if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.SFXType.PlaceShape);

                // 4. Line Checking (Executes instantly for maximum snappiness)
                CheckLines();

                // 5. Tray Management
                ShapeManager.Instance.RemoveShapeFromTray(shape);

                if (ShapeManager.Instance.ShapesInTray == 0)
                {
                    ShapeManager.Instance.SpawnTrayShapes(ScoreManager.Instance.CurrentPhase);
                }

                // 6. Game Over Check
                if (!ShapeManager.Instance.CheckAnyShapeCanFit())
                {
                    GameOver();
                }
                
                ShapePlacedEvent?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ZenGridManager] Error during shape placement: {e.Message}\n{e.StackTrace}");
            }
        }

        private void CheckLines()
        {
            GridSystem.Instance.GetFullLines(out var rowsToClear, out var colsToClear);

            if (rowsToClear.Count == 0 && colsToClear.Count == 0) return;

            HashSet<Vector2Int> cellsToClear = new HashSet<Vector2Int>();
            List<Vector2Int> explosionCenters = new List<Vector2Int>();

            // Identify initial line cells
            foreach (int y in rowsToClear)
            {
                for (int x = 0; x < GridSystem.Instance.Columns; x++)
                    cellsToClear.Add(new Vector2Int(x, y));
            }
            foreach (int x in colsToClear)
            {
                for (int y = 0; y < GridSystem.Instance.Rows; y++)
                    cellsToClear.Add(new Vector2Int(x, y));
            }

            // Check for lotuses in cleared lines
            foreach (var cellPos in cellsToClear)
            {
                if (GridSystem.Instance.GetCell(cellPos.x, cellPos.y).IsLotus)
                {
                    explosionCenters.Add(cellPos);
                }
            }

            // Handle explosions
            int emptySpacesCaught = 0;
            foreach (var center in explosionCenters)
            {
                LotusManager.Instance.HandleLotusExplosion(center, cellsToClear, ref emptySpacesCaught);
            }

            // Perform Clear
            int clearedCells = 0;
            Vector3 lastClearPos = Vector3.zero;

            foreach (var pos in cellsToClear)
            {
                GridCell cell = GridSystem.Instance.GetCell(pos.x, pos.y);
                if (cell.IsOccupied)
                {
                    bool wasLotus = cell.IsLotus;
                    lastClearPos = cell.transform.position;
              
                    GridSystem.Instance.ClearCell(pos.x, pos.y);
                    clearedCells++;
                }
            }

            // Scoring and Feedback
            if (clearedCells > 0)
            {
                int linesCleared = rowsToClear.Count + colsToClear.Count;
                int points = clearedCells * 100;

                if (SoundManager.Instance != null)
                {
                    var sfx = linesCleared > 1 ? SoundManager.SFXType.MultiLineClear : SoundManager.SFXType.ClearLine;
                    SoundManager.Instance.PlaySFX(sfx);
                }

                if (explosionCenters.Count > 0)
                {
                    int bonus = emptySpacesCaught * 500;
                    points += bonus;
                    if (bonus > 0 && SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.SFXType.TranquilityBonus);
                    if (JuiceManager.Instance != null) JuiceManager.Instance.SpawnFloatingText(lastClearPos, "+" + points, new Color(1f, 0.5f, 0.8f), 1.3f);
                }
                else
                {
                    if (JuiceManager.Instance != null)
                    {
                        JuiceManager.Instance.ScreenShake(0.3f, 15f);
                        JuiceManager.Instance.SpawnFloatingText(lastClearPos, "+" + points, Color.white, 1.1f);
                    }
                }

                ScoreManager.Instance.UpdateScore(points);
            }
        }
        
        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}