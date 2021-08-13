using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectModifications
{
    public string objectName;
    public int activationConditions;  
    // 0 = initial
    // 1 = final

    public string jsonModifications;
}


[System.Serializable]
public class clipData
{
    public string clipName = "";
    public AudioClip audioClip;
    public float timeToEnd = -1.0f;
    public bool autoAdvance = false;
    public string goCallback = "";  // on collision with this object, it executes a call back to advance
    public objectModifications[] objectChanges;

    //public string[] goActivateWhenStarted;  // list of game objects to activate when the clip is played
    //public string[] goDeactivateWhenEnded;  // list of game objects to deactivate when when the clip ends

}
