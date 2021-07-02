using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace demoRoutines
{
    public class demo : ActivityModule
    {
        private MediaPlayer mPlayer = null;         //Reference to the object that plays all media
        //[SerializeField]
        private demoData moduleData;        //Conatians answer choices, correct answers, and question text
        private Bridge bridge;
        private string jsonString;
        //public override void Initialize(ActivityModuleData dataIn)
        public override void Initialize(string jsonData)
        {
            moduleData = new demoData();

            jsonString = jsonData;
            JsonUtility.FromJsonOverwrite(jsonData, moduleData);
            mPlayer = MediaPlayer.Instance;

            bridge = new Bridge();
            bridge.ParseJson(jsonString);
            if (moduleData.timeToEnd > 0)
                StartCoroutine(EndByTime());
        }

        public override void EndOfModule()
        {
            bridge.CleanUp(jsonString);
//            StartCoroutine(DelayBeforeExit());
            FindObjectOfType<LabManager>().ModuleComplete();
        }


        //public override string SaveState()
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
