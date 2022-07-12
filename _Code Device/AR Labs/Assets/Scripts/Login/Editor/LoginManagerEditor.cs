using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(LoginManager))]
public class LoginManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Open Debug Zip Folder"))
        {
            // Create reference to folder location
            DirectoryInfo zipFolder = new DirectoryInfo(
                Path.Combine(
                    Application.persistentDataPath,
                    "DebugZip"));
            // Create folder if it doesn't exist
            zipFolder.Create();
            EditorUtility.RevealInFinder(zipFolder.FullName);
        }
    }
}
