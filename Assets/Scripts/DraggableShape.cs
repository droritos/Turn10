using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using ZenGrid;

public class DraggableShape : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private GridCell cellPrefab; // Use the same GridCell prefab
    
    private ShapeData _shapeData;
    private Transform _startParent;
    private Vector3 _startPosition;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Transform _dragContainer;

    private float _timeDown;
    private readonly float _blockSize = 100f; // 96 cell + 4 spacing
    private Vector3 _dragOffset;
    private float _trayScale;
    
    private Vector2Int _lastValidGridPos;
    private bool _hasValidGhost;

    public ShapeData ShapeData => _shapeData;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        _rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    public void Initialize(ShapeData data, Transform parent)
    {
        _shapeData = data.Clone();
        _startParent = parent;
        
        transform.SetParent(parent, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        
        BuildVisuals();
        
        float maxDim = Mathf.Max(_shapeData.width, _shapeData.height);
        // Smaller tray scale to avoid overlapping neighbor slots
        _trayScale = Mathf.Min(0.55f, 3.0f / maxDim * 0.55f); 
        transform.localScale = Vector3.one * _trayScale;
        
        gameObject.SetActive(true);
    }

    private void BuildVisuals()
    {
        // Clear old visual blocks safely
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        _rectTransform.sizeDelta = new Vector2(_shapeData.width * _blockSize, _shapeData.height * _blockSize);
        
        for (int y = 0; y < _shapeData.height; y++)
        {
            for (int x = 0; x < _shapeData.width; x++)
            {
                if (_shapeData.GetCell(x, y) == 1)
                {
                    GridCell cell = Instantiate(cellPrefab, transform);

                    RectTransform rt = cell.MyRectTransform;
                    rt.sizeDelta = new Vector2(_blockSize, _blockSize);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    
                    float posX = (x - _shapeData.width / 2f + 0.5f) * _blockSize;
                    float posY = -(y - _shapeData.height / 2f + 0.5f) * _blockSize;
                    rt.anchoredPosition = new Vector2(posX, posY);

                    // Set visual state using the consolidated script
                    cell.SetState(_shapeData.color, false, false);
                    cell.SetBackground(new Color(1, 1, 1, 0.4f)); // Subtle bg for tray shapes instead of transparent
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData) // PickUp
    {
        if (!ZenGridManager.Instance.isGameActive) return;

        transform.DOKill();
        
        _startPosition = _rectTransform.position;
        _timeDown = Time.time;
        
        _canvasGroup.alpha = 0.8f;
        _canvasGroup.blocksRaycasts = false;
        
        if (JuiceManager.Instance != null && JuiceManager.Instance.canvasTransform != null)
        {
            _dragContainer = JuiceManager.Instance.canvasTransform;
            transform.SetParent(_dragContainer);
        }
        
        transform.SetAsLastSibling();
        
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SoundManager.SFX.SelectShape);
        
        transform.DOScale(Vector3.one * 1.0f, 0.2f).SetEase(Ease.OutBack);
        
        UpdatePosition(eventData);
        UpdateGhostHint();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!ZenGridManager.Instance.isGameActive) return;
        UpdatePosition(eventData);
        UpdateGhostHint();
    }

    private void UpdatePosition(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_dragContainer, eventData.position, eventData.pressEventCamera, out var localPoint))
        {
            // Use local coordinates for more predictable movement
            // Offset the shape slightly upwards so it's not hidden by the finger
            _rectTransform.anchoredPosition = localPoint + new Vector2(0, 120f); 
        }
    }

    private void UpdateGhostHint()
    {
        if (GridSystem.Instance == null) return;
        
        Vector2Int gridPos = GetGridPosition();
        GridSystem.Instance.ClearAllGhosts();
        
        _hasValidGhost = GridSystem.Instance.CanPlaceShape(_shapeData, gridPos.x, gridPos.y);
        if (_hasValidGhost)
        {
            _lastValidGridPos = gridPos;
            GridSystem.Instance.ShowGhost(_shapeData, gridPos.x, gridPos.y);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _canvasGroup.alpha = 1.0f;
        _canvasGroup.blocksRaycasts = true;

        if (GridSystem.Instance != null)
        {
            GridSystem.Instance.ClearAllGhosts();
        }

        float timeDelta = Time.time - _timeDown;
        // Reduced threshold for quick placement
        if (timeDelta < 0.12f)
        {
            Rotate();
            SnapBack();
            return;
        }

        // Use the last valid ghost position for perfect placement
        if (_hasValidGhost)
        {
            Debug.Log($"[DraggableShape] Placing at cached ghost position: {_lastValidGridPos}");
            
            transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
                // Return to tray slot parent before disabling to avoid flickers on reuse
                transform.SetParent(_startParent);
                transform.localPosition = Vector3.zero;
                ZenGridManager.Instance.OnShapePlaced(this, _lastValidGridPos.x, _lastValidGridPos.y);
            });
        }
        else
        {
            Debug.Log($"[DraggableShape] No valid ghost found at {GetGridPosition()}. Snapping back.");
            SnapBack();
        }
    }
    
    private Vector2Int GetGridPosition()
    {
        return GridSystem.Instance.WorldToGrid(transform.position, _blockSize, _shapeData.width, _shapeData.height);
    }

    private void SnapBack()
    {
        transform.SetParent(_startParent);
        _rectTransform.DOMove(_startPosition, 0.3f).SetEase(Ease.OutBack);
        transform.DOScale(Vector3.one * _trayScale, 0.3f).SetEase(Ease.OutBack);
    }

    private void Rotate()
    {
        _shapeData.RotateClockwise();
        BuildVisuals();
        
        float maxDim = Mathf.Max(_shapeData.width, _shapeData.height);
        _trayScale = Mathf.Min(0.65f, 3.0f / maxDim * 0.65f); 
        
        transform.DOKill();
        transform.localScale = Vector3.one * (_trayScale + 0.15f);
        transform.DOScale(Vector3.one * _trayScale, 0.2f).SetEase(Ease.OutBounce);
    }
}