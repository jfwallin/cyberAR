using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class clipData
{
    public string clipName = "";
    public string audioClipString = "";
    public AudioClip audioClip = null;
    public float timeToEnd = -1.0f;
    public bool autoAdvance = false;
    public string goCallback = "";  // on collision with this object, it executes a call back to advance
    public string goNext = "";
    public string goPrevious = "";
    public objectModifications[] objectChanges;
    //public string[] objectMods; 
    //public string jsonModifications;
    //public string[] goActivateWhenStarted;  // list of game objects to activate when the clip is played
    //public string[] goDeactivateWhenEnded;  // list of game objects to deactivate when when the clip ends

}
