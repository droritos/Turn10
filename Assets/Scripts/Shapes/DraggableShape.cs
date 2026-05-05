using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using ZenGrid;
using ZenGrid.UI;

public class DraggableShape : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")] [SerializeField]
    private GridCell cellPrefab;

    [Header("Touch Settings")]
    [Tooltip("How big the invisible touch area should be around the shape.")]
    [SerializeField]
    private float _touchAreaSize = 550f;

    private ShapeData _shapeData;
    private Transform _startParent;
    private Vector3 _startPosition;
    private Transform _dragContainer;

    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] Image _hitboxImage;
    
    
    [SerializeField, Tooltip("Pixels between the bottom cell of the held shape and the finger.")]
    private float _gapAboveFinger = 60f;

    private float _timeDown;
    private readonly float _blockSize = 100f;
    private Vector3 _dragOffset;
    private float _trayScale;

    private Vector2Int _lastValidGridPos;
    private bool _hasValidGhost;
    private bool _isSettling; // Guard to prevent re-picking while snapping back

    // --- UPDATED TRACKING VARIABLES ---
    private Vector2 _grabOffset;
    private Tween _offsetTween;

    public ShapeData ShapeData => _shapeData;

    private Material _currentShader;

    private void OnValidate()
    {
        if (!_canvasGroup)
            _canvasGroup = GetComponent<CanvasGroup>();
        if (!_rectTransform)
            _rectTransform = GetComponent<RectTransform>();
        if (!_hitboxImage)
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
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(_touchAreaSize, _touchAreaSize, 0f));
    }
#endif

    public void Initialize(ShapeData data, Transform parent, Material cellShader)
    {
        transform.DOKill();
        _offsetTween?.Kill(); // Make sure to kill the offset tween on re-init

        _shapeData = data.Clone();
        _startParent = parent;
        _currentShader = cellShader;

        transform.SetParent(parent, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        BuildVisuals();

        float maxDim = Mathf.Max(_shapeData.width, _shapeData.height);
        _trayScale = Mathf.Min(0.55f, 3.0f / maxDim * 0.55f);
        transform.localScale = Vector3.one * _trayScale;

        _isSettling = false;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
        _canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
    }

    private void BuildVisuals()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject childObj = transform.GetChild(i).gameObject;

            if (_hitboxImage != null && childObj == _hitboxImage.gameObject)
            {
                continue;
            }

            Destroy(childObj);
        }

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
                    cell.SetShaders(_currentShader);
                }
            }
        }
    }

    private Vector2 _lastLocalPointerPos;

    public void OnDrag(PointerEventData eventData)
    {
        if (_isSettling || !ZenGridManager.Instance.isGameActive) return;
        UpdatePosition(eventData);
        UpdateGhostHint();
    }

    // --- UPDATED POINTER DOWN ---
    public void OnPointerDown(PointerEventData eventData) 
{
    if (!ZenGridManager.Instance.isGameActive) return;

    // If the shape is currently shrinking into the grid to be placed, ignore the touch.
    // We know it is placing (not snapping back) because raycasts are disabled.
    if (_isSettling && !_canvasGroup.blocksRaycasts) return;

    // If we caught the shape while it was snapping back to the tray, interrupt it!
    _isSettling = false; 

    transform.DOKill();
    _offsetTween?.Kill(); 
    
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
        SoundManager.Instance.PlaySFX(SoundManager.SFXType.SelectShape);
    
    transform.DOScale(Vector3.one * 1.0f, 0.2f).SetEase(Ease.OutBack);
    
    if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_dragContainer, eventData.position, eventData.pressEventCamera, out var localPoint))
    {
        // 1. Capture exact touch offset and save the pointer position
        _lastLocalPointerPos = localPoint;
        _grabOffset = _rectTransform.anchoredPosition - localPoint;

        // 2. Calculate target Y offset
        float targetBottomEdge = GetShapeBottomEdgeY() * 1.0f; 
        float targetOffsetY = _gapAboveFinger - targetBottomEdge;

        // 3. Tween the offset AND force the position to update every frame
        Vector2 targetOffset = new Vector2(_grabOffset.x, targetOffsetY);
        
        _offsetTween = DOTween.To(() => _grabOffset, x => _grabOffset = x, targetOffset, 0.15f)
            .SetEase(Ease.OutQuad)
            .OnUpdate(() => 
            {
                _rectTransform.anchoredPosition = _lastLocalPointerPos + _grabOffset;
            });
    }
    
    UpdatePosition(eventData);
    UpdateGhostHint();
}

    private void UpdatePosition(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_dragContainer, eventData.position,
                eventData.pressEventCamera, out var localPoint))
        {
            // Keep our tracking variable updated during the drag
            _lastLocalPointerPos = localPoint;

            // Apply the pointer position plus our animating offset
            _rectTransform.anchoredPosition = _lastLocalPointerPos + _grabOffset;
        }
    }


    private float GetShapeBottomEdgeY()
    {
        int lowestRow = 0;
        for (int y = 0; y < _shapeData.height; y++)
        for (int x = 0; x < _shapeData.width; x++)
            if (_shapeData.GetCell(x, y) == 1)
                lowestRow = y;

        float cellCenterY = -(lowestRow - _shapeData.height / 2f + 0.5f) * _blockSize;
        return cellCenterY - _blockSize / 2f;
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
        if (_isSettling) return;

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
            _isSettling = true;
            _canvasGroup.blocksRaycasts = false;

            ZenGridManager.Instance.OnShapePlaced(this, _lastValidGridPos.x, _lastValidGridPos.y);

            if (!gameObject.activeSelf)
            {
                transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    _isSettling = false;
                    transform.SetParent(_startParent);
                    transform.localPosition = Vector3.zero;
                });
            }
            else
            {
                _isSettling = false;
            }
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
        _isSettling = true;
        transform.SetParent(_startParent);
        _offsetTween?.Kill(); // Good practice to kill this on snap back too

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SoundManager.SFXType.BackToTray);

        _rectTransform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutBack).OnComplete(() => { _isSettling = false; });

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

        TutorialManager.NotifyRotated();
    }
}