using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ZenGrid
{
    public class DebugHUD : MonoBehaviour
    {
        public static DebugHUD Instance;

        [Header("Settings")]
        [SerializeField] private bool _showOnStart = false;

        [Header("UI References")]
        [SerializeField] private GameObject _displayRoot;
        [SerializeField] private TextMeshProUGUI _fpsText;
        [SerializeField] private TextMeshProUGUI _infoText;

        private float _deltaTime = 0.0f;
        private bool _isVisible = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            _isVisible = _showOnStart;
            if (_displayRoot != null) _displayRoot.SetActive(_isVisible);
        }

        private void Update()
        {
            HandleToggleInput();

            if (!_isVisible) return;
            UpdateStats();
        }

        private void HandleToggleInput()
        {
#if ENABLE_INPUT_SYSTEM
            // New Input System
            if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
            {
                ToggleHUD();
            }

            if (Touchscreen.current != null && Touchscreen.current.touches.Count == 3)
            {
                if (Touchscreen.current.touches[0].press.wasPressedThisFrame)
                {
                    ToggleHUD();
                }
            }
#else
            // Legacy Input System
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                ToggleHUD();
            }

            if (Input.touchCount == 3 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                ToggleHUD();
            }
#endif
        }

        private void ToggleHUD()
        {
            _isVisible = !_isVisible;
            if (_displayRoot != null) _displayRoot.SetActive(_isVisible);
        }

        private void UpdateStats()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            float fps = 1.0f / _deltaTime;
            float ms = _deltaTime * 1000.0f;

            if (_fpsText != null)
            {
                _fpsText.text = string.Format("{0:0.} FPS ({1:0.0} ms)", fps, ms);
                if (fps < 30) _fpsText.color = Color.red;
                else if (fps < 60) _fpsText.color = Color.yellow;
                else _fpsText.color = Color.green;
            }

            if (_infoText != null && ZenGridManager.Instance != null && ScoreManager.Instance != null)
            {
                _infoText.text = $"Phase: {ScoreManager.Instance.CurrentPhase}\n" +
                                 $"Score: {ScoreManager.Instance.Score}\n" +
                                 $"Tray Count: {ShapeManager.Instance.ShapesInTray}\n" +
                                 $"Grid Size: {GridSystem.Instance.Columns}x{GridSystem.Instance.Rows}";
            }
        }
    }
}
