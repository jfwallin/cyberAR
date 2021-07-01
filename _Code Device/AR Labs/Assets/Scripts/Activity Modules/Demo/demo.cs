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
        public override void Initialize(ActivityModuleData dataIn)
        {
            moduleData = new demoData();
            if (dataIn is demoData)
            {
                moduleData = (demoData) dataIn;
            }
            else
            {
                Debug.LogError("Unable to cast ActivityModuleData into correct data object, disabling script");
                enabled = false;
            }

            //jsonString = initData;
            //JsonUtility.FromJsonOverwrite(initData, moduleData);
            mPlayer = MediaPlayer.Instance;

            bridge = new Bridge();
            bridge.ParseJson(jsonString);
            if (moduleData.timeToEnd > 0)
                StartCoroutine(EndByTime());
        }

        public override void EndOfModule()
        {
            Debug.Log("demo finished!");
            //bridge.CleanUp(moduleData.json);
            bridge.CleanUp(jsonString);
            //FindObjectOfType<LabManager>().ModuleComplete();
        }

        //public override string SaveState()
        public override ActivityModuleData SaveState()
        {
            return moduleData;
        }


        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }


        IEnumerator EndByTime()
        {
            yield return new WaitForSeconds(moduleData.timeToEnd);

            SaveState();
            EndOfModule();
        }



    }
}
