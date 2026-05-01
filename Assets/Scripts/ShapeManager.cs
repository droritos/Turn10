using UnityEngine;
using System.Collections.Generic;

namespace ZenGrid
{
    public class ShapeManager : MonoBehaviour
    {
        public static ShapeManager Instance;

        [Header("Shapes Settings")]
        [SerializeField] private ShapeDatabase _shapeDatabase;
        [SerializeField] private DraggableShape _draggableShapePrefab;
        [SerializeField] private Transform[] _trayPositions;

        private List<DraggableShape> _shapePool = new List<DraggableShape>();

        public int ShapesInTray
        {
            get
            {
                int count = 0;
                foreach (var shape in _shapePool)
                {
                    if (shape.gameObject.activeSelf) count++;
                }
                return count;
            }
        }

        private void Awake()
        {
            Instance = this;
            InitializePool();
        }

        private void InitializePool()
        {
            foreach (var pos in _trayPositions)
            {
                DraggableShape shape = Instantiate(_draggableShapePrefab, pos);
                shape.gameObject.SetActive(false);
                _shapePool.Add(shape);
            }
        }

        public void SpawnTrayShapes(int currentPhase)
        {
            List<ShapeData> availableShapes = new List<ShapeData>();
            foreach (var shape in _shapeDatabase.shapes)
            {
                if (shape.minPhase <= currentPhase)
                    availableShapes.Add(shape);
            }

            if (availableShapes.Count == 0) return;

            for (int i = 0; i < _shapePool.Count; i++)
            {
                // Only spawn if the slot is currently empty/inactive
                if (!_shapePool[i].gameObject.activeSelf)
                {
                    ShapeData randomShape = availableShapes[Random.Range(0, availableShapes.Count)];
                    _shapePool[i].gameObject.SetActive(true);
                    _shapePool[i].Initialize(randomShape, _trayPositions[i]);
                }
            }
        }

        public void RemoveShapeFromTray(DraggableShape shape)
        {
            // Instead of destroying, we just disable it for the pool
            shape.gameObject.SetActive(false);
        }

        public bool CheckAnyShapeCanFit()
        {
            foreach (var shape in _shapePool)
            {
                if (!shape.gameObject.activeSelf) continue;

                for (int y = 0; y < GridSystem.Instance.Rows; y++)
                {
                    for (int x = 0; x < GridSystem.Instance.Columns; x++)
                    {
                        if (GridSystem.Instance.CanPlaceShape(shape.shapeData, x, y))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
