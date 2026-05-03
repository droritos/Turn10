using UnityEngine;

namespace ZenGrid.UI
{
    /// <summary>
    /// Base class for all menus. Uses CanvasGroup to show/hide menus without
    /// toggling SetActive, which avoids Awake() ordering issues with child components.
    /// The CanvasGroup blocks raycasts and interaction when hidden, so no clicks leak through.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseMenu : MonoBehaviour
    {
        public abstract MenuType MenuType { get; }

        [SerializeField]protected CanvasGroup CanvasGroup;

        protected virtual void Awake()
        {
            if(!CanvasGroup)
                CanvasGroup = GetComponent<CanvasGroup>();
            // Start fully hidden via CanvasGroup. Do NOT call Hide() here —
            // that would recurse before MenuManager registers us. MenuManager.Awake calls Hide().
        }
        protected virtual void OnValidate()
        {
            // Auto Grabbing Components
            if(!CanvasGroup)
                CanvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Show()
        {
            CanvasGroup.alpha = 1f;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
        }

        public virtual void Hide()
        {
            // Use CanvasGroup only — never SetActive(false).
            // This keeps Awake/Start running normally and button listeners stay wired.
            // Alpha=0 + blocksRaycasts=false means the menu is invisible AND untouchable.
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
        }

        public virtual void ToggleSpectate()
        {
            if (Mathf.Approximately(CanvasGroup.alpha, 0.5f))
            {
                CanvasGroup.alpha = 0f;
            }
            else 
                CanvasGroup.alpha = 0.5f;
            
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
        }
    }
}