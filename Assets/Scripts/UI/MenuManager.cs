using System.Collections.Generic;
using UnityEngine;

namespace ZenGrid.UI
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance { get; private set; }

        public MenuType startingMenu = MenuType.MainMenu;

        private Dictionary<MenuType, BaseMenu> _menus = new Dictionary<MenuType, BaseMenu>();
        private BaseMenu _currentMenu;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Register all child menus automatically
            BaseMenu[] childMenus = GetComponentsInChildren<BaseMenu>(true);
            foreach (var menu in childMenus)
            {
                _menus[menu.MenuType] = menu;
                menu.Hide(); // Hide all initially
            }
        }

        private void Start()
        {
            if (startingMenu != MenuType.None)
            {
                OpenMenu(startingMenu);
            }
        }

        public void OpenMenu(MenuType menuType)
        {
            if (_currentMenu != null)
            {
                if (_currentMenu.MenuType == menuType)
                    return; // Already open
                
                _currentMenu.Hide();
            }

            if (_menus.TryGetValue(menuType, out BaseMenu menuToOpen))
            {
                _currentMenu = menuToOpen;
                _currentMenu.Show();
            }
            else
            {
                Debug.LogWarning($"MenuManager: Could not find menu of type {menuType}");
            }
        }

        public void CloseCurrentMenu()
        {
            if (_currentMenu != null)
            {
                _currentMenu.Hide();
                _currentMenu = null;
            }
        }

        public BaseMenu GetMenu(MenuType menuType)
        {
            _menus.TryGetValue(menuType, out BaseMenu menu);
            return menu;
        }

#if UNITY_EDITOR
        public void PreviewMenu(MenuType menuType)
        {
            // In editor mode, we might not have initialized the dictionary via Awake
            BaseMenu[] childMenus = GetComponentsInChildren<BaseMenu>(true);
            foreach (var menu in childMenus)
            {
                if (menu.MenuType == menuType)
                {
                    menu.Show();
                    UnityEditor.EditorUtility.SetDirty(menu.gameObject);
                }
                else
                {
                    menu.Hide();
                    UnityEditor.EditorUtility.SetDirty(menu.gameObject);
                }
            }
        }
#endif
    }
}