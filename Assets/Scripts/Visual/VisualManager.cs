using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using ZenUI;

namespace ZenGrid
{
    public class VisualManager : MonoBehaviour
    {
        public static VisualManager Instance;
        
        [Tooltip("The fallback theme if the player hasn't picked one.")]
        [SerializeField] private GameVisualSettings _defaultTheme;
        
        [SerializeField] private List<ButtonUI> buttons;
        [SerializeField] private List<GridCell> cells;
        
        private GameVisualSettings _activeTheme;

        public event Action OnThemeChanged;

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>Access the currently active visual settings.</summary>
        public GameVisualSettings ActiveTheme => _activeTheme;

        // ── Unity API ────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _activeTheme = _defaultTheme;
                // Optional: Make this persist across scenes if your Main Menu and Game are different scenes
                // DontDestroyOnLoad(gameObject); 
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void OnValidate()
        {
            // Check if null FIRST, then check the count. 
            if (buttons == null || buttons.Count == 0)
            {
                // Exclude inactive objects (acts like the old FindObjectsOfType)
                buttons = FindObjectsByType<ButtonUI>(FindObjectsInactive.Exclude).ToList();
            }

            if (cells == null || cells.Count == 0)
            {
                cells = FindObjectsByType<GridCell>(FindObjectsInactive.Exclude).ToList();
            }
        }
// ── Public Methods ───────────────────────────────────────────────────

        /// <summary>
        /// Call this from your Settings Menu when the player picks a new theme.
        /// </summary>
        public void SetTheme(GameVisualSettings newTheme)
        {
            if (newTheme == null)
            {
                Debug.LogWarning("[VisualManager] SetTheme called with null theme.");
                return;
            }

            _activeTheme = newTheme;
            Debug.Log($"[VisualManager] Theme set to: {newTheme.ThemeName}");
            
            // Tell all listening scripts to update their materials!
            ApplyTheme();
            OnThemeChanged?.Invoke(); 
        }


        private void ApplyTheme()
        {
            foreach (var cell in cells)
            {
                if(cell)
                    cell.SetShaders(_activeTheme.CellShader);
            }

            foreach (var button in buttons)
            {
                if(button)
                    button.SetShader(_activeTheme.UiButtonShader);
            }
        }
    }
}

/*
public class VisualManager : MonoBehaviour
{
  
}
*/