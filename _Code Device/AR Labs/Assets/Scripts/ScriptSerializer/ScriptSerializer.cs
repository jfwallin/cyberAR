using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Serializer", menuName = "ScriptableObjects/ScriptSerializer", order = 1)]
public class ScriptSerializer : ScriptableObject
{
    [System.Serializable]
    public enum DataObject { LabData, MCExcerciseData, MCQData}

    public DataObject ObjectToSerialize = DataObject.LabData;
    public LabDataObject labData = null;
    public MCQ.MCExerciseData mCEData = null;
    public MCQ.MCQData mCQData = null;
    //public MCQ.MCQData script = null;
    [HideInInspector]
    public string serializedScript = "";

    public void SerializeScript()
    {
        switch (ObjectToSerialize)
        {
            case DataObject.LabData:
                serializedScript = JsonUtility.ToJson(labData, true);
                break;
            case DataObject.MCExcerciseData:
                serializedScript = JsonUtility.ToJson(mCEData, true);
                break;
            case DataObject.MCQData:
                serializedScript = JsonUtility.ToJson(mCQData, true);
                break;
        }
    }
}
