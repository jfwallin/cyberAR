using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScriptSerializer))]
public class ScriptSerializerEditor : Editor
{
    public override void OnInspectorGUI()
    { 
        ScriptSerializer serializer = (ScriptSerializer)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Serialize Script"))
        {
            serializer.SerializeScript();
        }

        GUILayout.TextArea(serializer.serializedScript);
    }
}
