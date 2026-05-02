using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
using ZenGrid.UI;

namespace ZenGrid.Editor
{
    [Overlay(typeof(SceneView), "Menu Switcher", true)]
    public class MenuManagerOverlay : Overlay
    {
        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement() { name = "Menu Switcher Root" };
            root.style.flexDirection = FlexDirection.Row;
            root.style.paddingTop = 5;
            root.style.paddingBottom = 5;
            root.style.paddingLeft = 5;
            root.style.paddingRight = 5;

            var label = new Label("Menu: ");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginRight = 5;
            label.style.alignSelf = Align.Center;
            root.Add(label);

            var dropdown = new EnumField(MenuType.None);
            dropdown.style.minWidth = 120;
            
            dropdown.RegisterValueChangedCallback(evt =>
            {
                MenuType selectedType = (MenuType)evt.newValue;
                SwitchMenu(selectedType);
            });

            root.Add(dropdown);

            return root;
        }

        private void SwitchMenu(MenuType type)
        {
            MenuManager manager = Object.FindAnyObjectByType<MenuManager>();
            if (manager == null)
            {
                Debug.LogWarning("[MenuSwitcher] No MenuManager found in current scene.");
                return;
            }

            Undo.RecordObject(manager, "Switch Menu Preview");
            manager.PreviewMenu(type);
            
            // Force repaint to see changes in Game/Scene view
            SceneView.RepaintAll();
        }

        // Standard MenuItem fallback
        [MenuItem("Tools/ZenGrid/Menu Switcher Window")]
        public static void OpenAsWindow()
        {
            EditorWindow.GetWindow<MenuSwitcherWindowStandalone>("Menu Switcher");
        }
    }

    // Small standalone window just in case they prefer it
    public class MenuSwitcherWindowStandalone : EditorWindow
    {
        private MenuType _selectedMenu = MenuType.None;
        private void OnGUI()
        {
            _selectedMenu = (MenuType)EditorGUILayout.EnumPopup("Menu", _selectedMenu);
            if (GUILayout.Button("Switch"))
            {
                MenuManager manager = Object.FindAnyObjectByType<MenuManager>();
                if (manager != null) manager.PreviewMenu(_selectedMenu);
            }
        }
    }
}
