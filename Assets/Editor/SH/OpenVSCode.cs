using UnityEditor;
using UnityEngine;
using System.Diagnostics;

public class OpenVSCode
{
    [MenuItem("Assets/★ Open with VS Code", false, 0)]
    static void OpenCode()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string fullPath = System.IO.Path.GetFullPath(path);
        Process.Start("code", $"\"{fullPath}\"");

        //ProcessStartInfo psi = new ProcessStartInfo
        //{
        //    FileName = "code",
        //    Arguments = $"\"{fullPath}\"",
        //    UseShellExecute = false,
        //    CreateNoWindow = true,
        //};

        //Process.Start(psi);
    }

    [MenuItem("Assets/★ Open with VS Code", true)]
    static bool ValidateOpenCode()
    {
        if (Selection.activeObject == null) return false;

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string ext = System.IO.Path.GetExtension(path).ToLower();

        return ext == ".cs" || ext == ".shader" || ext == ".cginc" || ext == ".hlsl" || ext == ".txt";
    }
}
