using System;
using UnityEngine;
using UnityEngine.UI; // <-- ADDED THIS for the Image component
using UnityEngine.EventSystems;
using DG.Tweening;
using ZenGrid;

public class DraggableShape : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private GridCell cellPrefab; 
    
    [Header("Touch Settings")]
    [Tooltip("How big the invisible touch area should be around the shape.")]
    [SerializeField] private float _touchAreaSize = 450f;
    
    private ShapeData _shapeData;
    private Transform _startParent;
    private Vector3 _startPosition;
    private Transform _dragContainer;
    
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] Image _hitboxImage; 

    private float _timeDown;
    private readonly float _blockSize = 100f; 
    private Vector3 _dragOffset;
    private float _trayScale;
    
    private Vector2Int _lastValidGridPos;
    private bool _hasValidGhost;

    public ShapeData ShapeData => _shapeData;

    private void OnValidate()
    {
        if(!_canvasGroup)
            _canvasGroup = GetComponent<CanvasGroup>();
        if (!_rectTransform)
            _rectTransform = GetComponent<RectTransform>();
        if(!_hitboxImage)
            _hitboxImage = GetComponentInChildren<Image>();
    }

    private void Awake()
    {
        _hitboxImage.color = new Color(0, 0, 0, 0); 
        _hitboxImage.raycastTarget = true;          

        _rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Set the color to a highly visible green
        Gizmos.color = Color.green;
        
        // This tells the Gizmo to rotate and scale along with the DraggableShape's transform
        Gizmos.matrix = transform.localToWorldMatrix;

        // Draw a wireframe box representing the _touchAreaSize.
        // Because our pivot is (0.5, 0.5), the center is Vector3.zero.
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(_touchAreaSize, _touchAreaSize, 0f));
    }
#endif
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
        _trayScale = Mathf.Min(0.55f, 3.0f / maxDim * 0.55f); 
        transform.localScale = Vector3.one * _trayScale;
        
        gameObject.SetActive(true);
    }

    private void BuildVisuals()
    {
        // Clear old visual blocks safely, BUT ignore our hitbox image!
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject childObj = transform.GetChild(i).gameObject;
            
            // If this child is our hitbox object, skip it and don't destroy it!
            if (_hitboxImage != null && childObj == _hitboxImage.gameObject)
            {
                continue; 
            }
            
            Destroy(childObj);
        }

        // Make the main rect big enough to hold everything
        _rectTransform.sizeDelta = new Vector2(_touchAreaSize, _touchAreaSize);
        
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

                    cell.SetState(_shapeData.color, false, false);
                    cell.SetBackground(new Color(1, 1, 1, 0.4f)); 
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
        _canvasGroup.blocksRaycasts = false; // This automatically disables our giant hitbox while dragging so it doesn't block the grid!
        
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
        if (timeDelta < 0.12f)
        {
            Rotate();
            SnapBack();
            return;
        }

        if (_hasValidGhost)
        {
            Debug.Log($"[DraggableShape] Placing at cached ghost position: {_lastValidGridPos}");
            
            transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
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