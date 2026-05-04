using System;
using UnityEngine;

namespace ZenGrid.UI
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        private const string PREF_ROTATE_SEEN = "Tutorial_RotateSeen";
        private bool _shouldShowRotateHint;

        [Header("References")]
        [SerializeField] private TutorialHintUI _rotateHintUI;

        [Header("Settings")]
        [Tooltip("Seconds after gameplay starts before the hint appears.")]
        [SerializeField] private float _showDelay = 1.5f;

        private bool _hintActive;
        private float _showTimer;
        private bool _timerRunning;

        public static event Action ShapeRotated;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _shouldShowRotateHint = PlayerPrefs.GetInt(PREF_ROTATE_SEEN, 0) == 0;
        }

        // Use Start() instead of OnEnable() to ensure ZenGridManager.Awake() has finished
        private void Start()      
        {
            ShapeRotated += OnShapeRotated;
            
            if (ZenGridManager.Instance != null)
            {
                ZenGridManager.Instance.ShapePlacedEvent += TryShowHint;
            }
        }

        private void OnDestroy() // Changed from OnDisable to match Start()
        {
            ShapeRotated -= OnShapeRotated;
            
            if (ZenGridManager.Instance != null)
            {
                ZenGridManager.Instance.ShapePlacedEvent -= TryShowHint;
            }
        }

        private void Update()
        {
            if (!_timerRunning) return;

            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f)
            {
                _timerRunning = false;
                ShowHint(); // UNCOMMENTED THIS!
            }
        }

        public void TryShowHint()
        {
            if (!_shouldShowRotateHint || _hintActive || _rotateHintUI == null) return;

            _showTimer    = _showDelay;
            _timerRunning = true;
        }

        public static void NotifyRotated()
        {
            ShapeRotated?.Invoke();
        }

        private void ShowHint()
        {
            if (!_shouldShowRotateHint || _rotateHintUI == null) return;

            _hintActive = true;
            _rotateHintUI.Show();
        }

        private void OnShapeRotated()
        {
            if (!_hintActive) return;

            PlayerPrefs.SetInt(PREF_ROTATE_SEEN, 1); // Uncomment This Later
            PlayerPrefs.Save();
            
            _shouldShowRotateHint = false;
            _hintActive           = false;
            _timerRunning         = false;

            _rotateHintUI.Dismiss();
        }
        
#if UNITY_EDITOR
        [ContextMenu("Reset Tutorial (Debug)")]
        private void ResetTutorial()
        {
            PlayerPrefs.DeleteKey(PREF_ROTATE_SEEN);
            Debug.Log("[TutorialManager] Tutorial reset. Will show on next play.");
        }
#endif
    }
}