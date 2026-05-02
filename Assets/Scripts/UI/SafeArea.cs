using System;
using UnityEngine;

namespace ZenGrid.UI
{
    /// <summary>
    /// Resizes a UI RectTransform to fit the 'Safe Area' of the device (avoiding notches and home bars).
    /// Attach this to a 'Container' object directly under the Main Canvas.
    /// </summary>
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        private Rect _lastSafeArea = new Rect(0, 0, 0, 0);

        private void OnValidate()
        {
            if(!rectTransform)
                rectTransform = GetComponent<RectTransform>();
        }

        void Awake()
        {
            Refresh();
        }

        void Update()
        {
            if (_lastSafeArea != Screen.safeArea)
            {
                Refresh();
            }
        }

        void Refresh()
        {
            Rect safeArea = Screen.safeArea;
            _lastSafeArea = safeArea;

            // Convert safe area rectangle from screen space to normalized anchor space
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}
