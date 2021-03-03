using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Serializer", menuName = "ScriptableObjects/ScriptSerializer", order = 1)]
public class ScriptSerializer : ScriptableObject
{
    public MCQ.MCQData script = null;
    public string serializedScript = "";

    public void SerializeScript()
    {
        serializedScript = JsonUtility.ToJson(script, true);
    }
}
