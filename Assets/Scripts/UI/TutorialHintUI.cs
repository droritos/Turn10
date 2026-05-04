using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZenGrid.UI
{
    /// <summary>
    /// Visual controller for the tap-to-rotate tutorial hint panel.
    /// Attach this to the hint panel GameObject inside the Gameplay canvas.
    ///
    /// Hierarchy suggestion:
    ///   GameplayUI
    ///     └─ TutorialHintPanel          ← this component lives here
    ///          ├─ Background (Image)
    ///          ├─ IconImage (Image)      ← the finger/tap icon
    ///          └─ HintText (TMP)        ← "Tap a shape to rotate it"
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class TutorialHintUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _iconTransform;
        [SerializeField] private TextMeshProUGUI _hintText;

        [Header("Content")]
        [SerializeField] private string _message = "Tap a shape\nto rotate it";

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration  = 0.4f;
        [SerializeField] private float _fadeOutDuration = 0.35f;
        [SerializeField] private float _pulseScale      = 1.18f;
        [SerializeField] private float _pulseDuration   = 0.65f;

        private Tween _pulseTween;

        // ── Lifecycle ─────────────────────────────────────────────

        private void Awake()
        {
            if (!_canvasGroup) _canvasGroup = GetComponent<CanvasGroup>();

            // Start invisible and non-interactive
            _canvasGroup.alpha          = 0f;
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = false;

            if (_hintText != null)
                _hintText.text = _message;
        }

        private void OnValidate()
        {
            if (!_canvasGroup) _canvasGroup = GetComponent<CanvasGroup>();
        }

        // ── Public API ────────────────────────────────────────────

        public void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(1f, _fadeInDuration)
                        .SetEase(Ease.OutCubic)
                        .OnComplete(StartPulse);
        }

        public void Dismiss()
        {
            StopPulse();
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(0f, _fadeOutDuration)
                        .SetEase(Ease.InCubic)
                        .OnComplete(() =>
                        {
                            _canvasGroup.interactable   = false;
                            _canvasGroup.blocksRaycasts = false;
                            gameObject.SetActive(false);
                        });
        }

        // ── Private ───────────────────────────────────────────────

        private void StartPulse()
        {
            if (_iconTransform == null) return;

            _pulseTween = _iconTransform
                .DOScale(_pulseScale, _pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo); // infinite ping-pong
        }

        private void StopPulse()
        {
            // Kill the looping pulse tween
            if (_pulseTween != null && _pulseTween.IsActive())
            {
                _pulseTween.Kill();
            }

            if (_iconTransform != null)
            {
                // Kill any other tweens on the transform just in case
                _iconTransform.DOKill();

                // Smoothly scale back to normal size (1, 1, 1) over the fade-out duration
                _iconTransform.DOScale(Vector3.one, _fadeOutDuration).SetEase(Ease.OutCubic);
            }
        }
    }
}
