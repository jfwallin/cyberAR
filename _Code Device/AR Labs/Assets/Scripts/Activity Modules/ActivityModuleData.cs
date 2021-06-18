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
    public string moduleName;
    public string specificName;
    public string[] prerequisiteActivities;
    public string[] educationalObjectives;
    public string[] instructions;
    public MediaInfo[] introMediaIDs;
    public MediaInfo[] outroMediaIDs;
    public int numRepeatsAllowed;
    public int numGradableRepeatsAllowed;
    public string gradingCriteria;
    public float currentScore;
    public float bestScore;
    public bool completed;
    public int currentSubphase;
    public string[] subphaseNames;
}