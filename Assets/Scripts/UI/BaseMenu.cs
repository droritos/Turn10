using UnityEngine;

namespace ZenGrid.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseMenu : MonoBehaviour
    {
        public abstract MenuType MenuType { get; }

        protected CanvasGroup CanvasGroup;

        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            CanvasGroup.alpha = 1f;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
        }

        public virtual void Hide()
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false); // Can be disabled if we don't want it processing anything, but CanvasGroup settings already prevent clicks.
        }
    }
}