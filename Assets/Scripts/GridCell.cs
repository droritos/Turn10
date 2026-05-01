using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ZenGrid
{
    public class GridCell : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _fillImage;
        [SerializeField] private GameObject _lotusVisual;

        private Color? _currentColor = null;
        private bool _isLotus = false;

        public bool IsOccupied => _currentColor.HasValue;
        public bool IsLotus => _isLotus;
        public Color? Color => _currentColor;

        private void Awake()
        {
            if (_fillImage != null) _fillImage.gameObject.SetActive(false);
            if (_lotusVisual != null) _lotusVisual.SetActive(false);
        }

        public void SetBackground(Color color)
        {
            if (_backgroundImage != null) _backgroundImage.color = color;
        }

        public void SetState(Color? color, bool isLotus, bool animate = true)
        {
            _currentColor = color;
            _isLotus = isLotus;

            if (color.HasValue)
            {
                _fillImage.gameObject.SetActive(true);
                _fillImage.color = color.Value;
                
                if (animate)
                {
                    _fillImage.rectTransform.localScale = Vector3.zero;
                    _fillImage.rectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                }
                else
                {
                    _fillImage.rectTransform.localScale = Vector3.one;
                }
            }
            else
            {
                if (animate)
                {
                    _fillImage.rectTransform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
                        _fillImage.gameObject.SetActive(false);
                    });
                }
                else
                {
                    _fillImage.gameObject.SetActive(false);
                }
            }

            if (_lotusVisual != null)
            {
                _lotusVisual.SetActive(isLotus);
            }
        }

        public void SetGhost(Color color)
        {
            _fillImage.gameObject.SetActive(true);
            _fillImage.color = new Color(color.r, color.g, color.b, 0.4f);
            _fillImage.rectTransform.localScale = Vector3.one;
        }

        public void ClearGhost()
        {
            if (!IsOccupied)
            {
                _fillImage.gameObject.SetActive(false);
            }
            else
            {
                _fillImage.color = _currentColor.Value;
            }
        }

        public void FlashBackground(Color flashColor, Color originalColor)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.DOColor(flashColor, 0.2f).SetLoops(2, LoopType.Yoyo).OnComplete(() => {
                    _backgroundImage.color = originalColor;
                });
            }
        }
    }
}
