using UnityEngine;
using TMPro;
using DG.Tweening;

namespace ZenGrid
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textMesh;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Setup(string text, Color color, Vector3 worldStartPos, float floatDistance = 1.0f, float duration = 1.5f, float scaleMultiplier = 1f)
        {
            if (_textMesh == null) _textMesh = GetComponentInChildren<TextMeshProUGUI>();
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

            if (_textMesh == null)
            {
                Debug.LogWarning("[FloatingText] TextMesh reference missing!");
                return;
            }

            _textMesh.text = text;
            _textMesh.color = color;
            
            // Convert world position to local position within the canvas
            if (transform.parent == null) return;
            RectTransform canvasRect = transform.parent.GetComponent<RectTransform>();
            if (canvasRect == null) return;

            Vector2 localPoint;
            Camera cam = Camera.main;
            if (cam == null) cam = GetComponentInParent<Canvas>().worldCamera;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, 
                cam != null ? cam.WorldToScreenPoint(worldStartPos) : (Vector2)worldStartPos, 
                null, 
                out localPoint);
            
            _rectTransform.anchoredPosition = localPoint;
            
            // Reset state
            transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            
            // Pop in (scaled by the multiplier for big/small messages)
            seq.Append(transform.DOScale(Vector3.one * (1.2f * scaleMultiplier), 0.2f).SetEase(Ease.OutQuad));
            seq.Append(transform.DOScale(Vector3.one * scaleMultiplier, 0.1f));
            
            // Smooth float up and fade
            // Note: floatDistance is now in UI units since we are using anchoredPosition
            float uiFloatDistance = floatDistance * 100f; 
            seq.Join(_rectTransform.DOAnchorPosY(localPoint.y + uiFloatDistance, duration).SetEase(Ease.OutSine));
            seq.Join(_textMesh.DOFade(0, duration).SetEase(Ease.InCubic));
            
            seq.OnComplete(() => {
                Destroy(gameObject);
            });
        }
    }
}
