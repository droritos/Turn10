using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace ZenGrid
{
    public class LotusManager : MonoBehaviour
    {
        public static LotusManager Instance;

        [SerializeField]
        [Tooltip("Fallback turns-between-lotus used when no GameModeConfig is set.")]
        private int _turnsBetweenLotus = 5;

        private int _turnsSinceLastLotus = 0;
        private int _activeLotusCount = 0;

        // ── Mode guard ───────────────────────────────────────────────────────
        /// <summary>True when the active mode permits Lotus behaviour.</summary>
        private static bool IsLotusEnabled
        {
            get
            {
                if (GameModeManager.Instance == null)
                {
                    // GameModeManager is missing from the scene — Lotus is DISABLED as a safe default.
                    // Add a GameModeManager MonoBehaviour to the scene to fix this properly.
                    Debug.LogError("[LotusManager] GameModeManager.Instance is NULL. " +
                                   "Add GameModeManager to the scene. Defaulting to Lotus DISABLED.");
                    return false;
                }
                return GameModeManager.Instance.IsLotusEnabled;
            }
        }

        /// <summary>Turns between seeds — reads from config if available, else inspector fallback.</summary>
        private int TurnsBetweenLotus =>
            GameModeManager.Instance != null
                ? GameModeManager.Instance.TurnsBetweenLotus
                : _turnsBetweenLotus;

        /// <summary>Whether Lotus cells can spread to neighbours.</summary>
        private static bool LotusCanSpread =>
            GameModeManager.Instance != null && GameModeManager.Instance.LotusCanSpread;

        /// <summary>Max simultaneous lotus seeds (0 = unlimited).</summary>
        private static int MaxSimultaneousLotus =>
            GameModeManager.Instance?.MaxSimultaneousLotus ?? 0;

        private void Awake()
        {
            Instance = this;
        }

        public void OnTurnPassed(int currentPhase)
        {
            // Diagnostic: log mode state once per turn so it's easy to verify in the Console
            Debug.Log($"[LotusManager] OnTurnPassed — mode: {GameModeManager.Instance?.ModeName ?? "NULL"}, " +
                      $"IsLotusEnabled: {IsLotusEnabled}, phase: {currentPhase}, turnsSinceLast: {_turnsSinceLastLotus}");

            if (!IsLotusEnabled) return;   // ← Pure Zen: skip entirely

            _turnsSinceLastLotus++;
            if (currentPhase >= 2 && _turnsSinceLastLotus >= TurnsBetweenLotus)
            {
                SpawnLotusSeed();
            }
        }

        public void SpreadLotus()
        {
            if (!IsLotusEnabled) return;    // ← Pure Zen: skip entirely
            if (!LotusCanSpread) return;    // ← Config knob: spreading disabled

            List<Vector2Int> newLotusCells = new List<Vector2Int>();
            int cols = GridSystem.Instance.Columns;
            int rows = GridSystem.Instance.Rows;

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    GridCell cell = GridSystem.Instance.GetCell(x, y);
                    if (cell.IsLotus)
                    {
                        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                        List<Vector2Int> neighbors = new List<Vector2Int>();
                        foreach (var d in dirs)
                        {
                            int nx = x + d.x; int ny = y + d.y;
                            GridCell neighbor = GridSystem.Instance.GetCell(nx, ny);
                            if (neighbor != null && neighbor.IsOccupied && !neighbor.IsLotus)
                            {
                                neighbors.Add(new Vector2Int(nx, ny));
                            }
                        }

                        if (neighbors.Count > 0 && Random.value < 0.3f)
                        {
                            newLotusCells.Add(neighbors[Random.Range(0, neighbors.Count)]);
                        }
                    }
                }
            }

            foreach (var pos in newLotusCells)
            {
                GridSystem.Instance.GetCell(pos.x, pos.y).SetState(Color.magenta, true);
                _activeLotusCount++;
            }
        }

        public void SpawnLotusSeed()
        {
            if (!IsLotusEnabled) return;   // ← Pure Zen: skip entirely

            // Respect max-simultaneous-lotus cap (0 = unlimited)
            if (MaxSimultaneousLotus > 0 && _activeLotusCount >= MaxSimultaneousLotus) return;

            List<Vector2Int> emptyCells = new List<Vector2Int>();
            int cols = GridSystem.Instance.Columns;
            int rows = GridSystem.Instance.Rows;

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (!GridSystem.Instance.GetCell(x, y).IsOccupied)
                    {
                        emptyCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (emptyCells.Count > 0)
            {
                Vector2Int pos = emptyCells[Random.Range(0, emptyCells.Count)];
                GridCell cell = GridSystem.Instance.GetCell(pos.x, pos.y);
                cell.SetState(Color.magenta, true);
                _turnsSinceLastLotus = 0;
                _activeLotusCount++;

                if (JuiceManager.Instance != null)
                {
                    JuiceManager.Instance.PopBlock(cell.GetComponent<RectTransform>());
                    //JuiceManager.Instance.PlayPetals(cell.transform.position, Color.magenta);
                }
            }
        }

        public void HandleLotusExplosion(Vector2Int center, HashSet<Vector2Int> cellsToClear, ref int emptySpacesCaught)
        {
            if (!IsLotusEnabled) return;   // ← Pure Zen: no lotus cells exist, so this should never be called — guard anyway

            for (int ex = -1; ex <= 1; ex++)
            {
                for (int ey = -1; ey <= 1; ey++)
                {
                    int nx = center.x + ex;
                    int ny = center.y + ey;
                    GridCell cell = GridSystem.Instance.GetCell(nx, ny);

                    if (cell != null)
                    {
                        if (cell.IsOccupied)
                        {
                            cellsToClear.Add(new Vector2Int(nx, ny));
                        }
                        else
                        {
                            emptySpacesCaught++;
                            if (JuiceManager.Instance != null)
                            {
                                JuiceManager.Instance.SpawnFloatingText(cell.transform.position, "TRANQUIL!", new Color(0.4f, 1f, 0.8f), 0.6f);
                                cell.FlashBackground(Color.white);
                            }
                        }
                    }
                }
            }
        }

        public void ResetLotusProtocol()
        {
            _turnsSinceLastLotus = 0;
            _activeLotusCount = 0;
        }
    }
}
