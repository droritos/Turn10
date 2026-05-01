using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace ZenGrid
{
    public class LotusManager : MonoBehaviour
    {
        public static LotusManager Instance;

        [SerializeField] private int _turnsBetweenLotus = 5;
        private int _turnsSinceLastLotus = 0;

        private void Awake()
        {
            Instance = this;
        }

        public void OnTurnPassed(int currentPhase)
        {
            _turnsSinceLastLotus++;
            if (currentPhase >= 2 && _turnsSinceLastLotus >= _turnsBetweenLotus)
            {
                SpawnLotusSeed();
            }
        }

        public void SpreadLotus()
        {
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
            }
        }

        public void SpawnLotusSeed()
        {
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

                if (JuiceManager.Instance != null)
                {
                    JuiceManager.Instance.PopBlock(cell.GetComponent<RectTransform>());
                    JuiceManager.Instance.PlayPetals(cell.transform.position, Color.magenta);
                }
            }
        }

        public void HandleLotusExplosion(Vector2Int center, HashSet<Vector2Int> cellsToClear, ref int emptySpacesCaught)
        {
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
                                JuiceManager.Instance.SpawnFloatingText(cell.transform.position, "TRANQUIL!", new Color(0.4f, 1f, 0.8f));
                                cell.FlashBackground(Color.white, new Color(0.9f, 0.9f, 0.9f, 0.8f));
                            }
                        }
                    }
                }
            }
        }

        public void ResetLotusProtocol()
        {
            _turnsSinceLastLotus = 0;
        }
    }
}
