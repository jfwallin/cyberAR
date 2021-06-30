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
        private enum jsonTestCase { useURL, useFile, useString};
        public override void Initialize(string initData)
        {

            Debug.Log("json string in the demo" + initData);
            moduleData = new demoData();
            bridge = new Bridge();
            JsonUtility.FromJsonOverwrite(initData, moduleData);
            Debug.Log(JsonUtility.ToJson(moduleData));
            mPlayer = MediaPlayer.Instance;

            Debug.Log("aobut to use the bridge");
            Debug.Log("json string = " + moduleData.json);
            Debug.Log("json string module name = " + moduleData.moduleName);
            Debug.Log("json string specific name = " + moduleData.specificName);
            Debug.Log("json string   json = " + moduleData.json);
            useTheBridge();
        }

        public override void EndOfModule()
        {
            Debug.Log("demo finished!");
            bridge.CleanUp(moduleData.json);
            FindObjectOfType<LabManager>().ModuleComplete();
        }

        public override string SaveState()
        {
            return JsonUtility.ToJson(moduleData);
        }


        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
        void useTheBridge()
        {
            int jsonTest = (int)jsonTestCase.useString;
            
            if (jsonTest == (int) jsonTestCase.useURL)
            {
                string url = moduleData.urlJson;
                Debug.Log("the module name is " + moduleData.moduleName);
                StartCoroutine(GetRequest(url));
            }
            else if (jsonTest == (int) jsonTestCase.useFile)

            {
                string jsonExample; ;
                string path;
                path = "C:/Users/jfwal/OneDrive/Documents/GitHub/cyberAR/_Code Device/AR Labs/Assets/Resources/scene-example.json";
                path = "C:/Users/jfwal/OneDrive/Documents/GitHub/cyberAR/_Code Device/AR Labs/Assets/Resources/basketball.json";
                StreamReader reader = new StreamReader(path);
                jsonExample = reader.ReadToEnd();
                Debug.Log(jsonExample);
                moduleData.json = jsonExample;
                bridge.ParseJson(moduleData.json);
            }
            else if (jsonTest == (int) jsonTestCase.useString)
            {
                Debug.Log("using string ");
                string s = "";
                s = moduleData.json;

               
                Debug.Log("modified string " + s);
                //bridge.ParseJson(moduleData.json);
                bridge.ParseJson(s);
            }
            else
            {

                Debug.Log("Invalid Json casei  " + jsonTest.ToString());
            }
        }


        IEnumerator EndByTime(string url)
        {
            yield return new WaitForSeconds(moduleData.timeToEnd);

            SaveState();
            EndOfModule();
        }

        // Loads json from URL, converts it to ObjectInfoCollection, and calls makeScene in the bridge class
        IEnumerator GetRequest(string url)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(url);

            Debug.Log("Loading json");
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Error loading json: " + webRequest.error);
            }
            else
            {
                string jstring = webRequest.downloadHandler.text; // json file read as string
                moduleData.json = jstring;
                bridge.ParseJson(moduleData.json);
            }
        }

    }
}
