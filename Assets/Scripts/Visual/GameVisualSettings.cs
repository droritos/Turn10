using UnityEngine;

namespace ZenGrid
{
    [CreateAssetMenu(fileName = "NewVisualTheme", menuName = "ZenGrid/Visual Theme")]
    public class GameVisualSettings : ScriptableObject
    {
        [Tooltip("Name of the theme, e.g., Dark Mode, Neon, Classic")]
        public string ThemeName = "Default";

        [Header("Materials")]
        public Material CellShader;
        public Material UiButtonShader;
        
        // Add anything else visual here later! 
        // public Color BackgroundColor;
    }
}