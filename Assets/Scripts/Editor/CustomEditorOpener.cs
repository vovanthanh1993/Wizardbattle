using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;

public class CustomEditorOpener
{
    [MenuItem("Assets/Open with Cursor")]
    public static void OpenWithCursor()
    {
        string selectedPath = GetSelectedPath();
        if (!string.IsNullOrEmpty(selectedPath))
        {
            OpenFileWithCursor(selectedPath);
        }
    }

    [MenuItem("Assets/Open with Cursor", true)]
    public static bool ValidateOpenWithCursor()
    {
        string selectedPath = GetSelectedPath();
        return !string.IsNullOrEmpty(selectedPath) && 
               (selectedPath.EndsWith(".cs") || selectedPath.EndsWith(".js") || selectedPath.EndsWith(".shader"));
    }

    private static string GetSelectedPath()
    {
        if (Selection.activeObject == null) return null;
        
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path)) return null;
        
        // Convert to absolute path
        string projectPath = Application.dataPath.Replace("/Assets", "");
        return Path.Combine(projectPath, path);
    }

    private static void OpenFileWithCursor(string filePath)
    {
        string cursorPath = GetCursorPath();
        if (string.IsNullOrEmpty(cursorPath))
        {
            UnityEngine.Debug.LogError("Cursor not found! Please set the correct path to Cursor.exe");
            return;
        }

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = cursorPath;
            startInfo.Arguments = $"\"{filePath}\"";
            startInfo.UseShellExecute = false;
            
            Process.Start(startInfo);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to open file with Cursor: {e.Message}");
        }
    }

    public static string GetCursorPath()
    {
        // Windows paths
        string[] possiblePaths = {
            @"C:\Users\" + System.Environment.UserName + @"\AppData\Local\Programs\Cursor\Cursor.exe",
            @"C:\Program Files\Cursor\Cursor.exe",
            @"C:\Program Files (x86)\Cursor\Cursor.exe"
        };

        foreach (string path in possiblePaths)
        {
            if (File.Exists(path))
                return path;
        }

        // Mac paths
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            string macPath = "/Applications/Cursor.app/Contents/MacOS/Cursor";
            if (File.Exists(macPath))
                return macPath;
        }

        // Linux paths
        if (Application.platform == RuntimePlatform.LinuxEditor)
        {
            string[] linuxPaths = {
                "/usr/bin/cursor",
                "/snap/bin/cursor",
                "/opt/cursor/cursor"
            };

            foreach (string path in linuxPaths)
            {
                if (File.Exists(path))
                    return path;
            }
        }

        return null;
    }
} 