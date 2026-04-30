using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DraggableShape : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public ShapeData shapeData;
    private Transform startParent;
    private Vector3 startPosition;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    
    private float timeDown;
    private float blockSize = 96f; // Matched to grid cell size in Canvas

    private Vector3 dragOffset;
    private float trayScale;

    public void Initialize(ShapeData data, Transform parent)
    {
        shapeData = data.Clone();
        startParent = parent;
        rectTransform = GetComponent<RectTransform>();
        
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        BuildVisuals();
        
        // Dynamically scale shapes so wide/tall shapes don't clip out of the tray container
        float maxDim = Mathf.Max(shapeData.width, shapeData.height);
        trayScale = Mathf.Min(0.65f, 3.0f / maxDim * 0.65f); 
        transform.localScale = Vector3.one * trayScale;
    }

    private void BuildVisuals()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        rectTransform.sizeDelta = new Vector2(shapeData.width * blockSize, shapeData.height * blockSize);
        
        for (int y = 0; y < shapeData.height; y++)
        {
            for (int x = 0; x < shapeData.width; x++)
            {
                if (shapeData.GetCell(x, y) == 1)
                {
                    GameObject blockObj = new GameObject("Block");
                    blockObj.transform.SetParent(transform, false);
                    Image img = blockObj.AddComponent<Image>();
                    img.color = shapeData.color;
                    
                    RectTransform rt = img.rectTransform;
                    rt.sizeDelta = new Vector2(blockSize, blockSize);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    
                    float posX = (x - shapeData.width / 2f + 0.5f) * blockSize;
                    float posY = -(y - shapeData.height / 2f + 0.5f) * blockSize;
                    rt.anchoredPosition = new Vector2(posX, posY);
                    
                    GameObject fillObj = new GameObject("Fill");
                    fillObj.transform.SetParent(rt, false);
                    Image fillImg = fillObj.AddComponent<Image>();
                    fillImg.color = new Color(1, 1, 1, 0.15f); // Subtler highlight so color pops!
                    RectTransform fillRt = fillImg.rectTransform;
                    fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
                    fillRt.offsetMin = new Vector2(4, 4); fillRt.offsetMax = new Vector2(-4, -4);
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Kill existing tweens to prevent snapping back while grabbing
        transform.DOKill();
        
        startPosition = rectTransform.position;
        timeDown = Time.time;
        
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        
        // Tween scale up!
        transform.DOScale(Vector3.one * 1.0f, 0.2f).SetEase(Ease.OutBack);
        
        dragOffset = new Vector3(0, 150f, 0); 
        rectTransform.position = new Vector3(eventData.position.x, eventData.position.y, 0) + dragOffset;
        
        UpdateGhostHint();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = new Vector3(eventData.position.x, eventData.position.y, 0) + dragOffset;
        UpdateGhostHint();
    }

    private void UpdateGhostHint()
    {
        if (ZenGridManager.Instance == null) return;
        
        RectTransform gridRectTransform = ZenGridManager.Instance.gridContainer.GetComponent<RectTransform>();
        Vector3 localPos = gridRectTransform.InverseTransformPoint(transform.position);
        Rect gridRect = gridRectTransform.rect;
        
        float shapeTopLeftX = localPos.x - (shapeData.width / 2f * blockSize);
        float shapeTopLeftY = localPos.y + (shapeData.height / 2f * blockSize);

        float gridRelativeX = shapeTopLeftX - gridRect.xMin;
        float gridRelativeY = gridRect.yMax - shapeTopLeftY;
        
        int gridX = Mathf.RoundToInt(gridRelativeX / blockSize);
        int gridY = Mathf.RoundToInt(gridRelativeY / blockSize);

        ZenGridManager.Instance.ShowGhost(shapeData, gridX, gridY);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;

        if (ZenGridManager.Instance != null)
        {
            ZenGridManager.Instance.ClearGhost();
        }

        float timeDelta = Time.time - timeDown;
        if (timeDelta < 0.25f)
        {
            Rotate();
            SnapBack();
            return;
        }

        if (TryPlaceOnGrid())
        {
            // DoTween scale out
            transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
                ZenGridManager.Instance.OnShapePlaced(this);
            });
        }
        else
        {
            SnapBack();
        }
    }
    
    private void SnapBack()
    {
        transform.SetParent(startParent);
        // DoTween smooth snap back!
        rectTransform.DOMove(startPosition, 0.3f).SetEase(Ease.OutBack);
        transform.DOScale(Vector3.one * trayScale, 0.3f).SetEase(Ease.OutBack);
    }

    private void Rotate()
    {
        shapeData.RotateClockwise();
        BuildVisuals();
        
        // Re-evaluate scale in case width/height swapped and it's a long shape
        float maxDim = Mathf.Max(shapeData.width, shapeData.height);
        trayScale = Mathf.Min(0.65f, 3.0f / maxDim * 0.65f); 
        
        // Pop effect on rotate
        transform.DOKill();
        transform.localScale = Vector3.one * (trayScale + 0.15f);
        transform.DOScale(Vector3.one * trayScale, 0.2f).SetEase(Ease.OutBounce);
    }

    private bool TryPlaceOnGrid()
    {
        if (ZenGridManager.Instance == null) return false;
        
        RectTransform gridRectTransform = ZenGridManager.Instance.gridContainer.GetComponent<RectTransform>();
        Vector3 localPos = gridRectTransform.InverseTransformPoint(transform.position);
        Rect gridRect = gridRectTransform.rect;
        
        float shapeTopLeftX = localPos.x - (shapeData.width / 2f * blockSize);
        float shapeTopLeftY = localPos.y + (shapeData.height / 2f * blockSize);

        float gridRelativeX = shapeTopLeftX - gridRect.xMin;
        float gridRelativeY = gridRect.yMax - shapeTopLeftY;
        
        int gridX = Mathf.RoundToInt(gridRelativeX / blockSize);
        int gridY = Mathf.RoundToInt(gridRelativeY / blockSize);

        if (ZenGridManager.Instance.CanPlaceShape(shapeData, gridX, gridY))
        {
            ZenGridManager.Instance.PlaceShape(shapeData, gridX, gridY);
            return true;
        }

        return false;
    }
}