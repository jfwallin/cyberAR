using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using MagicLeapTools;

using UnityEngine.UI;


namespace demoRoutines
{
    public class demo : ActivityModule
    {
        // Variables
        private demoData moduleData;      
        private string jsonString;

        private lightingControl lightControl;
        private AudioSource aud;
        private Bridge bridge;
        private demoSequence sequencer;

        private bool callBackActive = false;

        #region Public Methods
        public override void Initialize(string jsonData)
        {
            // Save the json string into a private variable
            jsonString = jsonData;
            // Get the moduleData from the json string
            moduleData = new demoData();
            JsonUtility.FromJsonOverwrite(jsonData, moduleData);
           
            // Setup the lightControl, and audio player
            lightControl = lightingControl.Instance;
            aud = gameObject.GetComponent<AudioSource>();

            // Set name of this object
            gameObject.name = "demoModule";

            // Play the introAudio
            if (moduleData.introAudio != "")
            {
                AudioHandler.Instance.PlayAudio(moduleData.introAudio);
            }

            // Set the light if needed
            if (moduleData.useSunlight)
            {
                Sunlight();
                if (TransmissionActivity && TransmissionHost)
                {
                    Transmission.Send(new RPCMessage("Sunlight"));
                }
            }

            // Instantiate the bridge and create the demo objects
            // Only if we are the transmission host, or we are alone.
            if (!TransmissionActivity || (TransmissionActivity && TransmissionHost))
            {
                bridge = Bridge.Instance;
                if (moduleData.createObjects)
                    bridge.MakeObjects(moduleData.objects);
                sequencer = gameObject.GetComponent<demoSequence>();
                if (moduleData.clips != null)
                {
                    sequencer.makeEvents(moduleData.clips);
                }

                // Set the end criteria
                if (moduleData.timeToEnd > 0)
                    StartCoroutine(EndByTime());
            }
        }

        public override void EndOfModule()
        {
            // Undo Lighting changes
            if (moduleData.restoreLights)
            {
                if (TransmissionActivity)
                {
                    if (TransmissionHost)
                        Transmission.Send(new RPCMessage("RestoreLights"));
                }
                else
                    lightControl.restoreLights();
            }

            // ***SHOULD CHECK IF RECEIVING FROM TRANSMISSION OR NOT***
            // Remove bridge spawned objects
            if (moduleData.destroyObjects && TransmissionHost)
                bridge.CleanUp(jsonString);

            // ***SHOULD CHECK IF RECEIVING FROM TRANSMISSION OR NOT***
            // More cleanup? Does the bridge not get it all?
            if (TransmissionHost)
            {
                GameObject currentLabObject = GameObject.Find("[CURRENT_LAB]");
                foreach (Transform child in currentLabObject.transform)
                {
                    if (child.gameObject.name != "instructionCanvas"
                        && child.gameObject.name != "MainInstruction"
                        && child.gameObject.name != "Directional Light"
                        && child.gameObject.name != "Point Light")
                    {
                        var to = child.GetComponent<TransmissionObject>();
                        if (to != null)
                            to.Despawn();
                        else
                            GameObject.Destroy(child.gameObject);
                    }
                }
            }

            // Does this script use Main Instructions?
            GameObject mi = GameObject.Find("MainInstructions");
            if (mi != null && mi.GetComponent<Text>() != null)
                mi.GetComponent<Text>().text = "";

            // Protecting against multiple module change events?
            if (callBackActive == false)
                FindObjectOfType<LabManager>().ModuleComplete();

            // Stop any audio playing 
            AudioHandler.Instance.StopAudio();
        }

        // Transmission RPC call methods
        public void Sunlight()
        {
            lightControl.sunlight();
        }

        public void RestoreLights()
        {
            lightControl.restoreLights();
        }

        // Called by sequencer when the prev. module button is clicked?
        public void nextModuleCallback()
        {
            callBackActive = true;
            EndOfModule();
            FindObjectOfType<LabManager>().nextModuleCallback();
        }

        // Called by sequencer when the next module button is clicked?
        public void previousModuleCallback()
        {
            callBackActive = true;
            EndOfModule();
            FindObjectOfType<LabManager>().previousModuleCallback();
        }

        // Currently unused
        public override string SaveState()
        {
            return jsonString;
        }
        #endregion Public Methods

        IEnumerator EndByTime()
        {
            yield return new WaitForSeconds(moduleData.timeToEnd);
            Debug.Log("ending module");
            EndOfModule();
        }
    }
}
