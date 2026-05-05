using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ZenGrid
{
    public class GridCell : MonoBehaviour
    {
        public bool IsLotus {get ; private set;} = false;
        public bool IsClearing {get ; private set;} = false;
        public bool IsOccupied => _currentColor.HasValue;

        [field:SerializeField] public RectTransform MyRectTransform { get; private set; }
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _fillImage;
        [SerializeField] private GameObject _lotusVisual;

        private Color? _currentColor = null;
        private Color _defaultBackgroundColor = new Color(1, 1, 1, 0.15f); // Fallback default

        private void OnValidate()
        {
            EnsureReferences();
            
            if(!MyRectTransform)
                MyRectTransform =  GetComponent<RectTransform>();
        }
        
        private void Awake() => EnsureReferences();

        private void EnsureReferences()
        {
            if (_backgroundImage == null) _backgroundImage = GetComponent<Image>();
            if (_fillImage == null)
            {
                Transform fill = transform.Find("Fill");
                if (fill != null) _fillImage = fill.GetComponent<Image>();
            }
            if (_lotusVisual == null)
            {
                Transform lotus = transform.Find("LotusVisual");
                if (lotus != null) _lotusVisual = lotus.gameObject;
            }
        }

        public void SetShaders(Material material)
        {
            _backgroundImage.material = material;
            _fillImage.material = material;
        }
        
        public void SetBackground(Color color)
        {
            EnsureReferences();
            _defaultBackgroundColor = color;
            if (_backgroundImage != null) _backgroundImage.color = color;
        }

        public void SetState(Color? color, bool isLotus, bool animate = true)
        {
            EnsureReferences();
            _currentColor = color;
            IsLotus = isLotus;

            if (color.HasValue)
            {
                if (_fillImage != null)
                {
                    _fillImage.gameObject.SetActive(true);
                    _fillImage.color = color.Value;
                    if (animate)
                    {
                        _fillImage.rectTransform.localScale = Vector3.zero;
                        _fillImage.rectTransform.DOKill();   
                        _fillImage.rectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                    }
                    else 
                    { 
                        _fillImage.rectTransform.localScale = Vector3.one; 
                    }
                }
            }
            else
            {
                // CLEARING LOGIC
                if (_fillImage != null)
                {
                    if (animate)
                    {
                        IsClearing = true; // 1. Lock the cell from ghosts/interactions
                        
                        _fillImage.rectTransform.DOKill();       
                        _fillImage.rectTransform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack)
                            .OnComplete(() => 
                            { 
                                if (_fillImage != null) _fillImage.gameObject.SetActive(false); 
                                IsClearing = false; // 2. Unlock it ONLY when the animation finishes
                            });
                    }
                    else
                    {
                        _fillImage.gameObject.SetActive(false);
                        IsClearing = false; // Ensure it's unlocked if we bypass animation
                    }
                }
            }

            if (_lotusVisual != null)
            {
                _lotusVisual.SetActive(isLotus);
            }
        }

        public void SetGhost(Color color)
        {
            // 3. SAFETY GUARD: Don't draw a ghost if the cell is currently exploding
            if (IsClearing) return; 

            EnsureReferences();
            if (_fillImage != null)
            {
                _fillImage.gameObject.SetActive(true);
                _fillImage.color = new Color(color.r, color.g, color.b, 0.4f);
                _fillImage.rectTransform.localScale = Vector3.one;
            }
        }

        public void ClearGhost()
        {
            // 3. SAFETY GUARD: Don't mess with visuals if the cell is currently exploding
            if (IsClearing) return; 

            EnsureReferences();
            if (_fillImage == null) return;

            if (!IsOccupied)
            {
                _fillImage.gameObject.SetActive(false);
            }
            else
            {
                _fillImage.color = _currentColor.Value;
            }
        }

        public void FlashBackground(Color flashColor)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.DOKill(); // Prevent overlapping tweens
                _backgroundImage.DOColor(flashColor, 0.2f).SetLoops(2, LoopType.Yoyo).OnComplete(() => {
                    _backgroundImage.color = _defaultBackgroundColor;
                });
            }
        }
    }
}