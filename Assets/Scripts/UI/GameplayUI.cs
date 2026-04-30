using UnityEngine;
using UnityEngine.UI;

namespace ZenGrid.UI
{
    public class GameplayUI : BaseMenu
    {
        public override MenuType MenuType => MenuType.Gameplay;

        [SerializeField] private Button _pauseButton;

        protected override void Awake()
        {
            base.Awake();
            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(OnPauseClicked);
            }
        }

        private void OnDestroy()
        {
            if (_pauseButton != null)
            {
                _pauseButton.onClick.RemoveListener(OnPauseClicked);
            }
        }

        private void OnPauseClicked()
        {
            // You can add PauseMenu later
            MenuManager.Instance.OpenMenu(MenuType.MainMenu);
        }
    }
}