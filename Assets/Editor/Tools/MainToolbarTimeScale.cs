using UnityEditor.Toolbars;
using UnityEngine;

namespace Editor.Tools
{
    public class MainToolbarTimeScale
    {
        const string k_ToolbarElementName = "Tools/Time Scale Slider";

        // Docks the slider to the right side of the main toolbar
        [MainToolbarElement(k_ToolbarElementName, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement TimeSlider()
        {
            // 1. Set up the text and tooltip
            var content = new MainToolbarContent("Time Scale", "Adjust Time.timeScale from 0 to 1");

            // 2. Create the built-in toolbar slider
            // Parameters: (content, initial value, min value, max value, callback method)
            var slider = new MainToolbarSlider(
                content, 
                Time.timeScale, 
                0f, 
                1f, 
                OnSliderValueChanged
            );

            return slider;
        }

        // 3. Update the actual game time when the slider is moved
        private static void OnSliderValueChanged(float newValue)
        {
            Time.timeScale = newValue;
        }
    }
}