using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScriptSerializer))]
public class ScriptSerializerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        DrawDefaultInspector();
        ScriptSerializer myTarget = (ScriptSerializer)target;
        if (GUILayout.Button("Serialize Script"))
        {
            myTarget.SerializeScript();
        }
    }
}
