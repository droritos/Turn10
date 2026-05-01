using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ZenGrid
{
    public class GridSystem : MonoBehaviour
    {
        public static GridSystem Instance;

        [Header("Grid Settings")]
        [SerializeField] private int _columns = 10;
        [SerializeField] private int _rows = 10;
        [SerializeField] private GridCell _cellPrefab;
        [SerializeField] private Transform _gridContainer;
        [SerializeField] Color backgroundColor = Color.white;

        private GridCell[,] _gridCells;

        public int Columns => _columns;
        public int Rows => _rows;

        private void Awake()
        {
            Instance = this;
        }

        [ContextMenu("Force Refresh Grid Visuals")]
        public void InitializeGrid()
        {
            // Make the container itself transparent so the gaps show the clean background
            Image containerImg = _gridContainer.GetComponent<Image>();
            if (containerImg != null) containerImg.color = new Color(1, 1, 1, 0);

            // Clear existing
            // Clear old grid if any (use DestroyImmediate to ensure they are gone before we create new ones)
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in _gridContainer) children.Add(child.gameObject);
            foreach (GameObject child in children) DestroyImmediate(child);
            
            _gridCells = new GridCell[_columns, _rows];

            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _columns; c++)
                {
                    GridCell cellObj = Instantiate(_cellPrefab, _gridContainer);
                    
                    // Zen Visuals: Make cells very subtle to let the shape colors pop
                    cellObj.SetBackground(backgroundColor);
                    _gridCells[c, r] = cellObj;
                }
            }
            Debug.Log("Grid Initialized");
        }

        public GridCell GetCell(int x, int y)
        {
            if (x < 0 || x >= _columns || y < 0 || y >= _rows) return null;
            return _gridCells[x, y];
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

                        if (checkX < 0 || checkX >= _columns || checkY < 0 || checkY >= _rows)
                            return false;

                        if (_gridCells[checkX, checkY].IsOccupied)
                            return false;
                    }
                }
            }
            return true;
        }

        public void PlaceShape(ShapeData shape, int gridX, int gridY)
        {
            for (int y = 0; y < shape.height; y++)
            {
                for (int x = 0; x < shape.width; x++)
                {
                    if (shape.GetCell(x, y) == 1)
                    {
                        int placeX = gridX + x;
                        int placeY = gridY + y;
                        _gridCells[placeX, placeY].SetState(shape.color, false);
                        
                        if (JuiceManager.Instance != null)
                        {
                            JuiceManager.Instance.PlayPetals(_gridCells[placeX, placeY].transform.position, shape.color);
                            JuiceManager.Instance.PopBlock(_gridCells[placeX, placeY].GetComponent<RectTransform>());
                        }
                    }
                }
            }
        }

        public Vector2Int WorldToGrid(Vector3 worldPos, float blockSize, int shapeWidth, int shapeHeight)
        {
            RectTransform gridRectTransform = _gridContainer.GetComponent<RectTransform>();
            Vector3 localPos = gridRectTransform.InverseTransformPoint(worldPos);
            Rect gridRect = gridRectTransform.rect;

            float shapeTopLeftX = localPos.x - (shapeWidth / 2f * blockSize);
            float shapeTopLeftY = localPos.y + (shapeHeight / 2f * blockSize);

            float gridRelativeX = shapeTopLeftX - gridRect.xMin;
            float gridRelativeY = gridRect.yMax - shapeTopLeftY;

            int gridX = Mathf.RoundToInt(gridRelativeX / blockSize);
            int gridY = Mathf.RoundToInt(gridRelativeY / blockSize);

            return new Vector2Int(gridX, gridY);
        }

        public void GetFullLines(out List<int> rowsToClear, out List<int> colsToClear)
        {
            rowsToClear = new List<int>();
            colsToClear = new List<int>();

            for (int y = 0; y < _rows; y++)
            {
                bool full = true;
                for (int x = 0; x < _columns; x++)
                {
                    if (!_gridCells[x, y].IsOccupied)
                    {
                        full = false;
                        break;
                    }
                }
                if (full) rowsToClear.Add(y);
            }

            for (int x = 0; x < _columns; x++)
            {
                bool full = true;
                for (int y = 0; y < _rows; y++)
                {
                    if (!_gridCells[x, y].IsOccupied)
                    {
                        full = false;
                        break;
                    }
                }
                if (full) colsToClear.Add(x);
            }
        }

        public void ClearCell(int x, int y, bool animate = true)
        {
            _gridCells[x, y].SetState(null, false, animate);
        }

        public void ShowGhost(ShapeData shape, int gridX, int gridY)
        {
            if (!CanPlaceShape(shape, gridX, gridY)) return;

            for (int y = 0; y < shape.height; y++)
            {
                for (int x = 0; x < shape.width; x++)
                {
                    if (shape.GetCell(x, y) == 1)
                    {
                        int placeX = gridX + x;
                        int placeY = gridY + y;
                        _gridCells[placeX, placeY].SetGhost(shape.color);
                    }
                }
            }
        }

        public void ClearAllGhosts()
        {
            for (int x = 0; x < _columns; x++)
            {
                for (int y = 0; y < _rows; y++)
                {
                    _gridCells[x, y].ClearGhost();
                }
            }
        }
    }
}
