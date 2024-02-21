using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using MagicLeapTools;

public class demoSequence : MonoBehaviour
{
    // Variables
    private int currentState = 0;

    private clipData theClip;
    private clipData[] theClips;
    private AudioSource aud;
    private Bridge bridge;
    private GameObject parentObj;

    // These strings are compared to the names of objects invoking a callback
    // to determine what action is taken
    private string actionNextClipName = "NextClip";
    private string actionPreviousClipName = "PreviousClip";
    private string actionNextModuleName = "NextModule";
    private string actionPreviousModuleName = "PreviousModule";
    private string actionEndApplicationName = "EndApplication";

    // Could probably delete this, it never gets used, better to just have one entry point
    public void ParseJson(string data)
    {
        demoSequenceData sdata = (JsonUtility.FromJson<demoSequenceData>(data));
        makeEvents(sdata.clipList);
    }

    // Takes a clip list, and starts iterating through it
    public void makeEvents( clipData[] sequenceData)
    {
        parentObj = GameObject.Find("[_DYNAMIC]");
        aud = GetComponent<AudioSource>();
        bridge = Bridge.Instance;
        theClips = sequenceData;
        currentState = 0;

        newClip();
    }

    // Checks if we have a next clip before trying to build it
    public void newClip()
    {
        LabLogger.Instance.InfoLog(
            this.GetType().ToString(),
            LabLogger.LogTag.STATE_START,
            theClips[currentState].clipName);

       if (currentState < theClips.Length)
       {
            theClip = theClips[currentState];
            processClip(theClip);
       }
       else
       {
            //Debug.Log(" no more clips!!!");
       }
    }

    // Builds/modifies the scene from clip data
    public void processClip(clipData theClip)
    {
        // Modify gameobject
        int conditionFlag = 0;
        modifyObjects(conditionFlag, theClip);

        // *** This doesn't do anything right now
        // Set up a time delay  if that is appropriate
        float timeDelay;
        timeDelay = theClip.timeToEnd;

        if (timeDelay > 0)
        {
            StartCoroutine(WaitForClip(timeDelay));
        }
    }

    // Modifies objects using the current clip and then starts a new clip
    // Not sure why it does both
    // This is specifically for an AUDIO clip being finished, not a module clip
    public void clipFinished()
    {
        // modify gameobjects
        int conditionFlag = 1;
        modifyObjects(conditionFlag, theClip);


        // Start the next clip
        if (theClip.autoAdvance)
        {
            incrementClip();
            newClip();
        }
    }

    // Increments state index if possible
    public void incrementClip()
    {
        currentState = currentState + 1;
        if (currentState > theClips.Length - 1)
            currentState = theClips.Length - 1;
    }

    // 
    public void modifyObjects(int conditionFlag, clipData theClip)
    {
        int activationConditions;
        objectModifications objectMods = new objectModifications();
        for (int i = 0; i < theClip.objectChanges.Length; i++)
        {
            activationConditions = theClip.objectChanges[i].activationConditions;

            //Debug.Log("PROCESSING CLIP " + theClip.clipName + "   "+ theClip.objectChanges[i].name + "  " +   
            //    " object # " + i.ToString() + "  activation " + activationConditions.ToString() + 
            //    " conditionFlag=" + conditionFlag.ToString());
          
            if (activationConditions == conditionFlag)
            {
                //JsonUtility.FromJsonOverwrite(theClip.objectChanges[i].jsonModifications, objectMods);
                //Debug.Log(JsonUtility.ToJson(theClip.objectChanges[i], true));
                objectMods = theClip.objectChanges[i];

                // if we are reactivating an inactive object, we need to find it from the 
                // objects parent name
                if (objectMods.reactivateObject)
                {
                    parentObj = GameObject.Find(objectMods.parentName);
                    if (parentObj == null)
                    {
                        parentObj = GameObject.Find("[_DYNAMIC]");
                    }
                    Transform[] trs = parentObj.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in trs)
                    {
                        if (t.name == objectMods.name)
                            t.gameObject.SetActive(true);
                    }

                }
                // if we are deactivating an object, we don't want to apply other modifications
                // to it
                else if (objectMods.enabled == false)
                {
                    GameObject.Find(objectMods.name).SetActive(false);
                }
                else
                    bridge.MakeObject(objectMods as ObjectInfo);
            }
        }
    }

    // Waits for an audio clip to play
    IEnumerator WaitForClip(float timeDelay)
    {
        // Play through the audiohandler, it manages transmission or not
        AudioHandler.Instance.PlayAudio(theClip.audioClipString);

        yield return new WaitForSeconds(timeDelay);
        clipFinished();
    }

    // Called by all the buttons, passed object that called
    // Does different actions depending on the object name
    public void actionCallBack(GameObject sendingObject)
    {
        if (sendingObject.name.IndexOf(actionNextClipName) >= 0)
            actionNextClip();
        else if (sendingObject.name.IndexOf(actionPreviousClipName) >= 0)
            actionPreviousClip();
        else if (sendingObject.name.IndexOf(actionNextModuleName) >= 0)
            actionNextModule();
        else if (sendingObject.name.IndexOf(actionPreviousModuleName) >= 0)
            actionPreviousModule();
        else if (sendingObject.name.IndexOf(actionEndApplicationName) >= 0)
            actionEndApplication();
        else if (sendingObject.name.IndexOf("basketball") >= 0)
            actionNextClip();
        else
            Debug.Log("unknown callback!");
    }

    public void actionNextClip()
    {
        // Modify gameobjects
        int conditionFlag = 1;
        modifyObjects(conditionFlag, theClip);
       
        incrementClip();
        StopAllCoroutines();
        newClip();
    }

    public void actionPreviousClip ()
    {
        // Modify gameobjects
        int conditionFlag = 1;
        modifyObjects(conditionFlag, theClip);

        currentState = currentState - 1; 
        if (currentState < 0)
            currentState = 0;
        StopAllCoroutines();
        newClip();
    }

    public void actionNextModule()
    {
        GameObject go1 = GameObject.Find("demoModule");
        go1.GetComponent<demoRoutines.demo>().nextModuleCallback();
             
    }

    public void actionPreviousModule()
    {
        GameObject go1 = GameObject.Find("demoModule");
        go1.GetComponent<demoRoutines.demo>().previousModuleCallback();
    }

    public void actionEndApplication()
    { }
}

