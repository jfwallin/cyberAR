using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LabLogger))]
public class LabLoggerEditor : Editor
{
    public override void OnInspectorGUI()
    { 
        LabLogger logger = (LabLogger)target;
        DrawDefaultInspector();

        if(logger.uploadLogs)
        {
            if (GUILayout.Button("Upload Logs and Stop"))
            {
                logger.UploadAndStop();
            }
        }
        if (GUILayout.Button("Open Log Folder"))
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath + "/Logs");
        }
    }
}
