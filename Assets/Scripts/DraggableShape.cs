using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using ZenGrid;

public class DraggableShape : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private GameObject _cellPrefab; // Use the same GridCell prefab
    
    private ShapeData _shapeData;
    private Transform _startParent;
    private Vector3 _startPosition;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Transform _dragContainer;

    private float _timeDown;
    private float _blockSize = 96f; 
    private Vector3 _dragOffset;
    private float _trayScale;

    public ShapeData shapeData => _shapeData;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        _rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        // Find a suitable drag container (JuiceManager usually has the main canvas ref)
        if (JuiceManager.Instance != null)
            _dragContainer = JuiceManager.Instance.canvasTransform;
    }

    public void Initialize(ShapeData data, Transform parent)
    {
        _shapeData = data.Clone();
        _startParent = parent;
        transform.SetParent(parent, false);
        transform.localPosition = Vector3.zero;
        
        BuildVisuals();
        
        float maxDim = Mathf.Max(_shapeData.width, _shapeData.height);
        _trayScale = Mathf.Min(0.65f, 3.0f / maxDim * 0.65f); 
        transform.localScale = Vector3.one * _trayScale;
    }

    private void BuildVisuals()
    {
        // Clear old blocks
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        _rectTransform.sizeDelta = new Vector2(_shapeData.width * _blockSize, _shapeData.height * _blockSize);
        
        for (int y = 0; y < _shapeData.height; y++)
        {
            for (int x = 0; x < _shapeData.width; x++)
            {
                if (_shapeData.GetCell(x, y) == 1)
                {
                    GameObject cellObj = Instantiate(_cellPrefab, transform);
                    GridCell cell = cellObj.GetComponent<GridCell>();
                    
                    RectTransform rt = cellObj.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(_blockSize, _blockSize);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    
                    float posX = (x - _shapeData.width / 2f + 0.5f) * _blockSize;
                    float posY = -(y - _shapeData.height / 2f + 0.5f) * _blockSize;
                    rt.anchoredPosition = new Vector2(posX, posY);

                    // Set visual state using the consolidated script
                    cell.SetState(_shapeData.color, false, false);
                    cell.SetBackground(new Color(1, 1, 1, 0)); // Transparent bg for tray shapes
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!ZenGridManager.Instance.isGameActive) return;

        transform.DOKill();
        
        _startPosition = _rectTransform.position;
        _timeDown = Time.time;
        
        _canvasGroup.alpha = 0.8f;
        _canvasGroup.blocksRaycasts = false;
        
        // Use the drag container to ensure it stays visible and on top of everything
        if (_dragContainer != null) transform.SetParent(_dragContainer);
        transform.SetAsLastSibling();
        
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SoundManager.SFX.SelectShape);
        
        transform.DOScale(Vector3.one * 1.0f, 0.2f).SetEase(Ease.OutBack);
        
        _dragOffset = new Vector3(0, 150f, 0); 
        _rectTransform.position = new Vector3(eventData.position.x, eventData.position.y, 0) + _dragOffset;
        
        UpdateGhostHint();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!ZenGridManager.Instance.isGameActive) return;
        _rectTransform.position = new Vector3(eventData.position.x, eventData.position.y, 0) + _dragOffset;
        UpdateGhostHint();
    }

    private void UpdateGhostHint()
    {
        if (GridSystem.Instance == null) return;
        
        Vector2Int gridPos = GetGridPosition();
        GridSystem.Instance.ClearAllGhosts();
        GridSystem.Instance.ShowGhost(_shapeData, gridPos.x, gridPos.y);
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
        if (timeDelta < 0.25f)
        {
            Rotate();
            SnapBack();
            return;
        }

        Vector2Int gridPos = GetGridPosition();
        if (GridSystem.Instance.CanPlaceShape(_shapeData, gridPos.x, gridPos.y))
        {
            transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
                ZenGridManager.Instance.OnShapePlaced(this, gridPos.x, gridPos.y);
            });
        }
        else
        {
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