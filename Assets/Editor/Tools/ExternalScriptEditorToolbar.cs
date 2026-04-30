#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;
using UnityEditor.Toolbars;

public static class ExternalScriptEditorToolbar
{
    const string k_ToolbarElementName = "ExternalScriptEditor";

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
        
        // Refresh the toolbar element to update the tooltip or text
        MainToolbar.Refresh(k_ToolbarElementName);
        
        Debug.Log($"External Script Editor set to: {(string.IsNullOrEmpty(path) ? "Open by file extension" : path)}");
    }
}
#endif