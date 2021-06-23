using System.Collections;
using System.Collections.Generic;
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
        
        public override void Initialize(string initData)
        {

            Debug.Log("xxx " + initData);
            moduleData = new demoData();
            bridge = new Bridge();
            JsonUtility.FromJsonOverwrite(initData, moduleData);
            Debug.Log(JsonUtility.ToJson(moduleData));
            mPlayer = MediaPlayer.Instance;

            Debug.Log("aobut to use the bridge");
            useTheBridge();
        }

        public override void EndOfModule()
        {
            Debug.Log("demo finished!");
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
            bool doURL = false;

            if (doURL)
            {
                string url = moduleData.urlJson;
                Debug.Log("the module name is " + moduleData.moduleName);
                StartCoroutine(GetRequest(url));
            }
            else
            {


                string jsonExample; ;
                string path;
                path = "C:/Users/jfwal/OneDrive/Documents/GitHub/cyberAR/_Code Device/AR Labs/Assets/Resources/scene-example.json";
                path = "C:/Users/jfwal/OneDrive/Documents/GitHub/cyberAR/_Code Device/AR Labs/Assets/Resources/basketball.json";
                //path = "C:/Users/jfwal/OneDrive/Documents/GitHub/cyberAR/_Code Device/AR Labs/Assets/Resources/earthMoon1.json";
                StreamReader reader = new StreamReader(path);
                jsonExample = reader.ReadToEnd();
                Debug.Log(jsonExample);
                bridge.ParseJson(jsonExample);
            }
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
                bridge.ParseJson(jstring);
            }
        }

    }
}
