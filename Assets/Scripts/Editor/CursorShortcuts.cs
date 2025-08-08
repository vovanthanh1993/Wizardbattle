using UnityEngine;
using UnityEditor;

public class CursorShortcuts
{
    [MenuItem("Assets/Open with Cursor %#o")] // Ctrl+Shift+O
    public static void OpenWithCursorShortcut()
    {
        CustomEditorOpener.OpenWithCursor();
    }

    [MenuItem("Assets/Open Project in Cursor")]
    public static void OpenProjectInCursor()
    {
        string projectPath = Application.dataPath.Replace("/Assets", "");
        string cursorPath = CustomEditorOpener.GetCursorPath();
        
        if (!string.IsNullOrEmpty(cursorPath))
        {
            try
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = cursorPath;
                startInfo.Arguments = $"\"{projectPath}\"";
                startInfo.UseShellExecute = false;
                
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to open project with Cursor: {e.Message}");
            }
        }
    }
} 