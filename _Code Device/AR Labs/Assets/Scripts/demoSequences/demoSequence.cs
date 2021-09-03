using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class demoSequence : MonoBehaviour
{
    // Start is called before the first frame update
    //private demoSequenceData sequenceData;
    private int currentState = 0;

    private clipData theClip;
    private clipData[] theClips;
    private AudioSource aud;
    private Bridge bridge;
    private GameObject parentObj;

    private string actionNextClipName = "NextClip";
    private string actionPreviousClipName = "PreviousClip";
    private string actionNextModuleName = "NextModule";
    private string actionPreviousModuleName = "PreviousModule";
    private string actionEndApplicationName = "EndApplication";


    public void ParseJson(string data)
    {
        demoSequenceData sdata = (JsonUtility.FromJson<demoSequenceData>(data));
        makeEvents(sdata.clipList);
    }
    public void makeEvents( clipData[] sequenceData)
    {
        parentObj = GameObject.Find("[_DYNAMIC]");
        aud = GetComponent<AudioSource>();
        bridge = new Bridge();
        theClips = sequenceData;
        currentState = 0;

        newClip();
    }

    public void newClip()
    {
       if (currentState < theClips.Length)
        {
            Debug.Log("current clip " + currentState.ToString());
            theClip = theClips[currentState];
            processClip(theClip);
        } else
        {
            Debug.Log(" no more clips!!!");
        }
    }


    public void processClip(clipData theClip)
    {

        Debug.Log("process clip  " + currentState.ToString() + 
            "  timetoend=" + theClip.timeToEnd.ToString() + " *****************************");

        // modify gameobject
        int conditionFlag = 0;
        modifyObjects(conditionFlag, theClip);

        // set up a time delay  if that is appropriate
        float timeDelay;
        timeDelay = theClip.timeToEnd;

        if (timeDelay > 0)
        {
            StartCoroutine(WaitForClip(timeDelay));
        }
    }

    public void clipFinished()
    {
        // modify gameobjects
        int conditionFlag = 1;
        modifyObjects(conditionFlag, theClip);


        Debug.Log("clip finished ....");
        if (theClip.autoAdvance)
        {
            incrementClip();
            newClip();
        }
    }


    public void incrementClip()
    {
        currentState = currentState + 1;
        if (currentState > theClips.Length - 1)
            currentState = theClips.Length - 1;
    }


    public void modifyObjects(int conditionFlag, clipData theClip)
    {
        int activationConditions;
        objectModifications objectMods = new objectModifications();
        for (int i = 0; i < theClip.objectChanges.Length; i++)
        {
            activationConditions = theClip.objectChanges[i].activationConditions;

            Debug.Log("PROCESSING CLIP " + theClip.clipName + "   "+ theClip.objectChanges[i].name + "  " +   
                " object # " + i.ToString() + "  activation " + activationConditions.ToString() + 
                " conditionFlag=" + conditionFlag.ToString());
          
            if (activationConditions == conditionFlag)
            {
                //JsonUtility.FromJsonOverwrite(theClip.objectChanges[i].jsonModifications, objectMods);
                Debug.Log(JsonUtility.ToJson(theClip.objectChanges[i], true));
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
                    bridge.makeObject(objectMods as ObjectInfo);
            }
        }
    }


    IEnumerator WaitForClip(float timeDelay)
    {
        aud.clip = Resources.Load<AudioClip>(theClip.audioClipString);

        if (aud.clip != null)
            aud.Play();

        Debug.Log("TTIME DELAY " + timeDelay.ToString() + "*********************************************************************************************");

        yield return new WaitForSeconds(timeDelay);
        clipFinished();
    }

   
    public void actionCallBack(GameObject sendingObject)
    {
        Debug.Log("actioncallback from " + sendingObject.name + "++++++++++++++++++++++++++++");
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
        Debug.Log("action next!!!!!-----------------------------------------------------------");

        // modify gameobjects
        int conditionFlag = 1;
        modifyObjects(conditionFlag, theClip);
       
        Debug.Log("new clips!!!! " + currentState.ToString());

        incrementClip();
        StopAllCoroutines();
        newClip();
    }

    public void actionPreviousClip ()
    {
        Debug.Log("action previous!!!!------------------------------------------------");

        // modify gameobjects
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

        Debug.Log("previous module");
        //GameObject.Find("Manager").GetComponent<LabManager>().nextModuleCallback();
        GameObject go1 = GameObject.Find("demoModule");
        go1.GetComponent<demoRoutines.demo>().nextModuleCallback();
             
    }
    public void actionPreviousModule()
    {
        Debug.Log("action next module");
        //GameObject.Find("Manager").GetComponent<LabManager>().previousModuleCallback();
        GameObject go1 = GameObject.Find("demoModule");
        go1.GetComponent<demoRoutines.demo>().previousModuleCallback();

    }


    public void actionEndApplication()
    {

    }
}

