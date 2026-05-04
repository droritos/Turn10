#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;
using UnityEditor.Toolbars;

/// <summary>
/// Adds a toolbar dropdown for switching the external script editor.
/// The [InitializeOnLoadMethod] guard here works around a Unity 6 bug where
/// [MainToolbarElement] fires during domain reload before the IMGUI skin is ready,
/// poisoning PropertyEditor+Styles' static constructor and crashing the Inspector.
/// We defer a forced EditorStyles warm-up so the static type is initialised safely.
/// </summary>
[InitializeOnLoad]
public static class ExternalScriptEditorToolbar
{
    const string k_ToolbarElementName = "ExternalScriptEditor";

    // Warm up EditorStyles after domain reload to prevent the Inspector crash loop.
    static ExternalScriptEditorToolbar()
    {
        EditorApplication.delayCall += WarmUpEditorStyles;
    }

    private static void WarmUpEditorStyles()
    {
        try
        {
            // Accessing any EditorStyles property forces the skin to initialize.
            // This breaks the "poisoned static ctor" cycle before the Inspector opens.
            var _ = EditorStyles.label;
        }
        catch
        {
            // If it fails here, queue another attempt next frame.
            EditorApplication.delayCall += WarmUpEditorStyles;
        }
    }

    [MainToolbarElement(k_ToolbarElementName, defaultDockPosition = MainToolbarDockPosition.Right)]
    static IEnumerable<MainToolbarElement> CreateToolbarElement()
    {
        var content = new MainToolbarContent(GetCurrentEditorName(), "External Script Editor");
        yield return new MainToolbarDropdown(content, ShowDropdownMenu);
    }

    private static string GetCurrentEditorName()
    {
        var currentInstall = CodeEditor.Editor.CurrentInstallation;
        return string.IsNullOrEmpty(currentInstall.Name) ? "Open by extension" : currentInstall.Name;
    }

    private static void ShowDropdownMenu(Rect dropDownRect)
    {
        var menu = new GenericMenu();

        bool isDefaultActive = string.IsNullOrEmpty(CodeEditor.CurrentEditorPath);
        menu.AddItem(new GUIContent("Open by file extension"), isDefaultActive, () => SetEditor(string.Empty));

        menu.AddSeparator(string.Empty);

        var installations = CodeEditor.Editor.GetFoundScriptEditorPaths();
        foreach (var installation in installations)
        {
            bool isActive = (installation.Key == CodeEditor.CurrentEditorPath);
            string capturedPath = installation.Key;
            menu.AddItem(new GUIContent(installation.Value), isActive, () => SetEditor(capturedPath));
        }

        menu.DropDown(dropDownRect);
    }

    private static void SetEditor(string path)
    {
        CodeEditor.Editor.SetCodeEditor(path);

        // Refresh the toolbar element to update the displayed editor name.
        MainToolbar.Refresh(k_ToolbarElementName);

        Debug.Log($"External Script Editor set to: {(string.IsNullOrEmpty(path) ? "Open by file extension" : path)}");
    }
}
#endif