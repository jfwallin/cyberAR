using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for the serializeable data classes describing 
/// activity modules
/// </summary>
[System.Serializable]
public class ActivityModuleData
{
    // module description data
    public string moduleName;
    public string jsonFileName;
    public string description;
    public string author;
    public string authorInstitution;
    public string dateCreated;
    public string[] prerequisiteActivities;

    // data used at runtime within the prefab
    public string prefabName;
    public string specificName;  // appears in the navigation menu
    public string[] educationalObjectives;
    public string[] instructions;
    public MediaInfo[] introMediaIDs;
    public MediaInfo[] outroMediaIDs;

    // grading and access information
    public int numRepeatsAllowed;
    public int numGradableRepeatsAllowed;
    public string gradingCriteria;
    public float currentScore;
    public float bestScore;
    public bool completed;
    public int currentSubphase;
    public string[] subphaseNames;
}