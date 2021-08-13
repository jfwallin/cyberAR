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


    public void ParseJson(string data)
    {
        demoSequenceData sdata = (JsonUtility.FromJson<demoSequenceData>(data));
        makeEvents(sdata.clipList);
    }
    public void makeEvents( clipData[] sequenceData)
    {
        aud = GetComponent<AudioSource>();
        theClips = sequenceData;
        currentState = 0;
    }

    public void newClip()
    {
       if (currentState < theClips.Length)
        {
            theClip = theClips[currentState];
            processClip(theClip);
        }
    }

    public void processClip( clipData theClip)
    {
        // set the callback
        GameObject go;
        go = GameObject.Find(theClip.goCallback);
        if (go != null)
            go.GetComponent<MagicLeapTools.InputReceiver>().OnClick.AddListener(actionCallBack);

        // modify gameobject
        int conditionFlag = 0;
        modifyObjects(conditionFlag, theClip);

        // play the audio clip
        aud.clip = theClip.audioClip;
        aud.Play();

        // set up a time delay  if that is appropriate
        float timeDelay;
        if (theClip.autoAdvance)
        {
            timeDelay = Mathf.Max(theClip.audioClip.length, theClip.timeToEnd);
            if (theClip.timeToEnd > 0)
                timeDelay = theClip.timeToEnd;
            else
                timeDelay = theClip.audioClip.length;

            StartCoroutine(WaitForClip(timeDelay));
        }
        
    }

    public void modifyObjects(int conditionFlag, clipData theClip)
    {
        int activationConditions;
        ObjectInfo objectMods = new ObjectInfo();
        for (int i = 0; i < theClip.objectChanges.Length; i++)
        {
            activationConditions = theClip.objectChanges[i].activationConditions;
            if (activationConditions == conditionFlag)
            {
                JsonUtility.FromJsonOverwrite(theClip.objectChanges[i].jsonModifications, objectMods);
                bridge.makeObject(objectMods);
            }

        }

    }
    public void clipFinished()
    {

        // modify gameobjects
        int conditionFlag = 1;
        modifyObjects(conditionFlag, theClip);

        currentState = currentState + 1;
       
    }

    public void actionCallBack(GameObject sendingObject)
    {
        clipFinished();
    }




    IEnumerator WaitForClip(float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        clipFinished();
    }




}

