using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using ZenGrid.UI;

namespace ZenGrid
{
    public class ZenGridManager : MonoBehaviour
    {
        public static ZenGridManager Instance;

        public bool isGameActive = false;

        private void Awake()
        {
            Instance = this;
            
            // Mobile Optimizations
            Application.targetFrameRate = -1;
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

        public void OnShapePlaced(DraggableShape shape, int gridX, int gridY)
        {
            try
            {
                // 1. Logic Placement
                GridSystem.Instance.PlaceShape(shape.ShapeData, gridX, gridY);
                
                // 2. Score for placing
                ScoreManager.Instance.UpdateScore(10);

                // 3. Audio/Visuals
                if (JuiceManager.Instance != null) JuiceManager.Instance.ScreenShake(0.1f, 5f);
                if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.SFX.PlaceShape);

                // 4. Lotus Protocol
                LotusManager.Instance.SpreadLotus();
                LotusManager.Instance.OnTurnPassed(ScoreManager.Instance.CurrentPhase);

                // 5. Line Checking
                CheckLines();

                // 6. Tray Management
                ShapeManager.Instance.RemoveShapeFromTray(shape);

                if (ShapeManager.Instance.ShapesInTray == 0)
                {
                    ShapeManager.Instance.SpawnTrayShapes(ScoreManager.Instance.CurrentPhase);
                }

                // 7. Game Over Check
                if (!ShapeManager.Instance.CheckAnyShapeCanFit())
                {
                    GameOver();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ZenGridManager] Error during shape placement: {e.Message}\n{e.StackTrace}");
            }
        }

        private void CheckLines()
        {
            List<int> rowsToClear, colsToClear;
            GridSystem.Instance.GetFullLines(out rowsToClear, out colsToClear);

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
                    
                    if (JuiceManager.Instance != null)
                    {
                        if (wasLotus) JuiceManager.Instance.PlayExplosion(lastClearPos, Color.magenta);
                        else JuiceManager.Instance.PlayPetals(lastClearPos, Color.white);
                    }

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
                    var sfx = linesCleared > 1 ? SoundManager.SFX.MultiLineClear : SoundManager.SFX.ClearLine;
                    SoundManager.Instance.PlaySFX(sfx);
                }

                if (explosionCenters.Count > 0)
                {
                    int bonus = emptySpacesCaught * 500;
                    points += bonus;
                    if (bonus > 0 && SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.SFX.TranquilityBonus);
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

        private void GameOver()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.SFX.GameOver);

            if (GameOverUI.Instance != null)
                GameOverUI.Instance.Populate(ScoreManager.Instance.Score, ScoreManager.Instance.BestScore);

            if (MenuManager.Instance != null)
                MenuManager.Instance.OpenMenu(MenuType.GameOver);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}