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

        // set the callbacks
        // for clip zero, get the forward and back information
        GameObject go;
        go = GameObject.Find(theClips[0].goNext);
        if (go != null)
        { 
            go.GetComponent<MagicLeapTools.InputReceiver>().OnSelected.AddListener(actionNext);
            //Debug.Log("Seeting next callback  "+ theClip.goNext);
        }
        go = GameObject.Find(theClips[0].goPrevious);
        if (go != null)
        {
            Debug.Log("Seeting previous callback ");
            
            go.GetComponent<MagicLeapTools.InputReceiver>().OnSelected.AddListener(actionPrevious);
        }

        newClip();
    }
    private void OnDisable()
    {

        GameObject go;
        go = GameObject.Find(theClips[0].goNext);
        if (go != null)
        {
            //go.GetComponent<MagicLeapTools.InputReceiver>().OnSelected.RemoveAllListeners();
            go.GetComponent<MagicLeapTools.InputReceiver>().OnSelected.RemoveAllListeners();
        }

        if (go != null)
        {
            go = GameObject.Find(theClips[0].goPrevious);
            go.GetComponent<MagicLeapTools.InputReceiver>().OnSelected.RemoveAllListeners();
        }


    }


    public void newClip()
    {
        Debug.Log("new clips!!!! " + currentState.ToString());
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

        // modify gameobject
        int conditionFlag = 0;
        modifyObjects(conditionFlag, theClip);

        // set up a time delay  if that is appropriate
        float timeDelay;
        timeDelay = theClip.timeToEnd;
        //if (theClip.timeToEnd > 0)
        //    timeDelay = theClip.timeToEnd;
        //else
        //    timeDelay = theClip.audioClip.length;

        if (timeDelay > 0)
        {
            StartCoroutine(WaitForClip(timeDelay));
        }
        
    }

    public void modifyObjects(int conditionFlag, clipData theClip)
    {
        int activationConditions;
        objectModifications objectMods = new objectModifications();
        for (int i = 0; i < theClip.objectChanges.Length; i++)
        {
            activationConditions = theClip.objectChanges[i].activationConditions;

            Debug.Log("PROCESSING CLIP " + theClip.clipName + "  activation " + activationConditions.ToString());
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
                        Debug.Log("parent object = NULL!!!!" + objectMods.name + "  " + theClip.clipName);
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
                    Debug.Log("here we go!!!!");
                    bridge.makeObject(objectMods as ObjectInfo);
            }

        }

    }
    public void clipFinished()
    {
        // modify gameobjects
        int conditionFlag = 1;
        modifyObjects(conditionFlag, theClip);

        if (theClip.autoAdvance)
        {
            currentState = currentState + theClip.incrementClip;
            newClip();
        }

    }

    public void actionCallBack(GameObject sendingObject)
    {
        currentState = 0;
        clipFinished();
    }
    public void actionNext(GameObject sendingObject)
    {
        //GameObject go;   
        //go = GameObject.Find("brbNext");
        //go.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        Debug.Log("action next!!!!!");
        currentState = currentState + theClip.incrementClip; // deltaI;
        // modify gameobjects
        int conditionFlag = 1;
        modifyObjects(conditionFlag, theClip);
        newClip();
    }

    public void actionPrevious(GameObject sendingObject)
    {
        //GameObject go;   
        //go = GameObject.Find("brbPrevious");
        //go.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        Debug.Log("action previous!!!!");
        // modify gameobjects
        int conditionFlag = 1;
        modifyObjects(conditionFlag, theClip);
        currentState = currentState - theClip.incrementClip; // deltaI;
        if (currentState < 0)
            currentState = 0;
        newClip();
    }


/*
    IEnumerator DebounceSelect()
    {

        deltaI = 0;
        yield return new WaitForSeconds(1.0f);
        deltaI = 1;
    }
*/

    IEnumerator WaitForClip(float timeDelay)
    {
        aud.clip = Resources.Load<AudioClip>(theClip.audioClipString);

        if (aud.clip != null)
            aud.Play();

        yield return new WaitForSeconds(timeDelay);
        clipFinished();
    }




}

