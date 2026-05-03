using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ZenGrid;
using ZenGrid.UI;

namespace ZenUI
{
    public class SettingsMenuUI : BaseMenu
    {
        public override MenuType MenuType => MenuType.Settings;
        
        [SerializeField] ButtonUI _dropletShaderButton;
        [SerializeField] ButtonUI _glassShaderButton;
        [SerializeField] ButtonUI _returnToMainMenuButton;
        
        [SerializeField] GameVisualSettings dropletShader;
        [SerializeField] GameVisualSettings glassShader;

        private void Start()
        {
            // Click Events
            _dropletShaderButton.Button.onClick.AddListener(() => VisualManager.Instance.SetTheme(dropletShader));
    
            _glassShaderButton.Button.onClick.AddListener(() => VisualManager.Instance.SetTheme(glassShader));
            
            _returnToMainMenuButton.Button.onClick.AddListener(() => MenuManager.Instance.OpenMenu(MenuType.MainMenu));
            
            _dropletShaderButton.Text.SetText(dropletShader.ThemeName);
            _glassShaderButton.Text.SetText(glassShader.ThemeName);
        }
    }
}
