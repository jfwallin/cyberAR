using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;


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


        //public override void Initialize(ActivityModuleData dataIn)
        public override void Initialize(string jsonData)
        {
            // save the json string into a private variable
            moduleData = new demoData();
            jsonString = jsonData;
            JsonUtility.FromJsonOverwrite(jsonData, moduleData);
           
            // setup the media player, lightControl, and audio player
            mPlayer = MediaPlayer.Instance;
            lightControl = lightingControl.Instance;
            aud = gameObject.GetComponent<AudioSource>();

            // play the introAudio
            aud.clip = Resources.Load<AudioClip>(moduleData.introAudio);
            aud.Play();

            // set the light if needed
            if (moduleData.useSunlight)
                lightControl.sunlight();

            // instantiate the bridge and create the demo objects
            bridge = new Bridge();
            if (moduleData.createObjects)
                bridge.ParseJson(jsonString);

            // set the end criteria
            if (moduleData.timeToEnd > 0)
                StartCoroutine(EndByTime());
        }

        public override void EndOfModule()
        {
            if (moduleData.restoreLights)
                lightControl.restoreLights();   
            
            if (moduleData.destroyObjects)
                bridge.CleanUp(jsonString); 
            FindObjectOfType<LabManager>().ModuleComplete();
        }


        public override string SaveState()
        {
            return jsonString;
        }


        IEnumerator EndByTime()
        {
            yield return new WaitForSeconds(moduleData.timeToEnd);

            EndOfModule();
        }



    }
}
