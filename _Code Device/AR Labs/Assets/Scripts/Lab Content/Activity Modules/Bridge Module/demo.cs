using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

using UnityEngine.UI;


namespace demoRoutines
{
    public class demo : ActivityModule
    {
        private demoData moduleData;      
        private string jsonString;

        private MediaPlayer mPlayer = null;    
        private lightingControl lightControl;
        private AudioSource aud;
        private Bridge bridge;
        private demoSequence sequencer;

        private bool callBackActive = false;
        //private InstructionBox ibox;

        //public override void Initialize(ActivityModuleData dataIn)
        public override void Initialize(string jsonData)
        {
            //Debug.Log("starting the f module");
            // save the json string into a private variable
            moduleData = new demoData();
            jsonString = jsonData;
            JsonUtility.FromJsonOverwrite(jsonData, moduleData);
           
            // setup the media player, lightControl, and audio player
            mPlayer = MediaPlayer.Instance;
            lightControl = lightingControl.Instance;
            
            aud = gameObject.GetComponent<AudioSource>();

            gameObject.name = "demoModule";
            //ibox = InstructionBox.Instance;
            //ibox.AddPage("test", "this is a big test", true);

            // play the introAudio
            if (moduleData.introAudio != "")
            {
                aud.clip = MediaCatalogue.Instance.GetAudioClip(moduleData.introAudio);
                aud.Play();
            }

            // set the light if needed
            if (moduleData.useSunlight)
                lightControl.sunlight();

            // instantiate the bridge and create the demo objects
            bridge = new Bridge();
            if (moduleData.createObjects)
                bridge.makeObjects(moduleData.objects);


            sequencer = gameObject.GetComponent<demoSequence>();
            if (moduleData.clips != null)
            {

                /*
                Debug.Log("json data " + jsonData);
                Debug.Log(JsonUtility.ToJson(moduleData, true));
                Debug.Log("number of clips = " + moduleData.clips.Length);

                for (int i = 0; i < moduleData.clips.Length; i++)
                {
                    Debug.Log("------------------------");
                    Debug.Log("clip name " + moduleData.clips[i].clipName);
                    Debug.Log("objects to modify = " + moduleData.clips[i].objectChanges.Length);
                    Debug.Log("time to end =" + moduleData.clips[i].timeToEnd.ToString());
                    //Debug.Log("jsonmods = " + moduleData.clips[i].jsonModifications);
                    for (int j = 0; j < moduleData.clips[i].objectChanges.Length; j++)
                    {
                        Debug.Log("object " + j.ToString());
                        Debug.Log("name = " + moduleData.clips[i].objectChanges[j].name);
                        Debug.Log(" material = " + moduleData.clips[i].objectChanges[j].material);
                        Debug.Log(" activation = " + moduleData.clips[i].objectChanges[j].activationConditions.ToString());
                       // Debug.Log("json = " + JsonUtility.ToJson(moduleData.clips[i].objectChanges[j]));
                    }
                }

                if (sequencer == null)
                {
                    Debug.Log("no sequencer");
                }
                */
                sequencer.makeEvents(moduleData.clips);
                //Debug.Log("sequence done");
            }

            // set the end criteria
            if (moduleData.timeToEnd > 0)
                StartCoroutine(EndByTime());
        }


        public override void EndOfModule()
        {
            string jdata = JsonUtility.ToJson(moduleData, true);
            //Debug.Log(jdata);
            string odata = JsonUtility.ToJson(moduleData.objects, true);
            //Debug.Log(odata);

            if (moduleData.restoreLights)
                lightControl.restoreLights();   
            
            if (moduleData.destroyObjects)
                bridge.CleanUp(jsonString);

            GameObject currentLabObject = GameObject.Find("[CURRENT_LAB]");
            foreach (Transform child in currentLabObject.transform)
            {
                if (child.gameObject.name != "instructionCanvas"
                    && child.gameObject.name != "MainInstruction"
                    && child.gameObject.name != "Directional Light" 
                    && child.gameObject.name != "Point Light")
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            GameObject mi = GameObject.Find("MainInstructions");
            if (mi != null && mi.GetComponent<Text>() != null)
                mi.GetComponent<Text>().text = "";
            if (callBackActive == false)
                FindObjectOfType<LabManager>().ModuleComplete();


        }


        public void nextModuleCallback()
        {
            callBackActive = true;
            EndOfModule();
            FindObjectOfType<LabManager>().nextModuleCallback();
        }

        public void previousModuleCallback()

        {
            callBackActive = true;
            EndOfModule();
            FindObjectOfType<LabManager>().previousModuleCallback();
            
        }

        public override string SaveState()
        {
            return jsonString;
        }


        IEnumerator EndByTime()
        {
            yield return new WaitForSeconds(moduleData.timeToEnd);
            Debug.Log("ending module");
            EndOfModule();
        }



    }
}
