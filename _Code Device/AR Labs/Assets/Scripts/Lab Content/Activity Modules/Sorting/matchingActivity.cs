using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



/*
 Home = Recenter content
Trigger = Grab
Touchpad Left/Right = Scale
Touchpad Up/Down = Nudge
Touchpad Radial = Rotate
Touchpad Forcepress = Reset
Reach out to extend pointer.
 */

namespace sortingRoutines
{
    public class matchingActivity : ActivityModule
    {

        public class matchInfo
        {
            public GameObject theObject;
            public string matchName;
            public bool isMatcheded;

            public float fractionalDistance;

            // implement IComparable interface
            public bool Match(object obj)
            {
                if (obj is matchInfo)
                {
                    return matchName.Equals((obj as matchInfo).matchName);
                }
                else
                {
                    return false;
                }
                //throw new ArgumentException("Object is not a matchInfo");
            }
        }

        // declare the sort info array
        private matchInfo[] matchData;
        private Dictionary<string, string> matchNames;


        private static int nObjects;
        private bool isMatcheded;

        // sortable Objects
        private Vector3[] sortPts;


        // markers
        private GameObject markerPrefab;
        private string markerPrefabName = "Prefab/tinysphere";
        private GameObject[] markers;
        private float mscale = 0.1f;
        private float voffset = 0.2f;

        //public AudioClip grab;
        public const int maxWrongAnswers = 5;
        public AudioClip[] wrongOrder = new AudioClip[maxWrongAnswers];
        public AudioClip correctOrder;
        private int wrongAnswerCount = 0;
        private int totalWrongAnswer = 0;

        //public GameObject theButton;
        private string buttonPrefabString;
        private GameObject buttonPrefab;
        public GameObject myButton;

        private bool feedbackEnabled = true;

        // default values for the delay, move time, and flourish of the movements
        private float tdelay = 2.0f;
        private float tmove = 5.0f;
        private int pretty = 1;

        //---------------------------------------------------------
        private sortingActivityData moduleData;
        private string jsonString;

        private MediaPlayer mPlayer = null;
        private GameObject mainCamera;
        private lightingControl lightControl;
        private AudioSource aud;
        private Bridge bridge;

        #region overrides
        //public override void Initialize(ActivityModuleData dataIn)
        public override void Initialize(string jsonData)
        {
            // save the json string into a private variable
            moduleData = new sortingActivityData();
            jsonString = jsonData;
            JsonUtility.FromJsonOverwrite(jsonData, moduleData);


            string jdata = JsonUtility.ToJson(moduleData, true);
            Debug.Log(jdata);


            ObjectInfoCollection oi = JsonUtility.FromJson<ObjectInfoCollection>(jsonString);
            foreach (ObjectInfo obj in oi.objects)
            {
                string jdat = JsonUtility.ToJson(obj, true);
                Debug.Log(jdat);
            }

            // setup the media player, lightControl, and audio player
            mPlayer = MediaPlayer.Instance;
            lightControl = lightingControl.Instance;
            aud = gameObject.GetComponent<AudioSource>();
            mainCamera = GameObject.Find("Main Camera");

            // play the introAudio
            aud.clip = Resources.Load<AudioClip>(moduleData.introAudio);
            aud.Play();

            // set the light if needed
            if (moduleData.useSunlight)
                lightControl.sunlight();

            // instantiate the bridge and create the demo objects
            bridge = Bridge.Instance;
            if (moduleData.createObjects)
                bridge.MakeObjects(moduleData.objects);

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

            destroyMarkers();
            Destroy(myButton, 0.1f);
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
        #endregion


        // Start is called before the first frame update
        void Start()
        {
            ObjectInfoCollection objList = JsonUtility.FromJson<ObjectInfoCollection>(jsonString);
            nObjects = objList.objects.Length;
            sortPts = new Vector3[nObjects]; // 
            //matchPts = new Vector3[nObjects];
            for (int i= 0; i < nObjects; i++)
            {
                Debug.Log(i.ToString() + " : " + objList.objects[i].name);
                sortPts[i] = objList.objects[i].position;
            }


            // set the array to be unsorted
            isMatcheded = false;

            // create an array to help with the sorting
            matchData = new matchInfo[nObjects];


            // populate the sorting array with needed information
            try
            {
                for (int i = 0; i < nObjects; i+=2)
                {
                    matchData[i] = new matchInfo();
                    matchData[i + 1] = new matchInfo();

                    matchData[i].theObject = GameObject.Find(objList.objects[i].name);
                    matchData[i + 1].theObject = GameObject.Find(objList.objects[i + 1].name);

                    matchData[i].isMatcheded = false;
                    matchData[i + 1].isMatcheded = false;

                    // add the moveObject script to the sortable objects
                    matchData[i].theObject.AddComponent<moveObjects>();
                    matchData[i + 1].theObject.AddComponent<moveObjects>();

                    matchData[i].matchName = matchData[i + 1].theObject.name;
                    matchData[i + 1].matchName = matchData[i].theObject.name;

                    matchNames.Add(matchData[i].theObject.name, matchData[i + 1].theObject.name);
                }
            }
            catch { print("There is an uneven amount of matching pairs."); }

    
            // this sets the intial position to the current position of the objects
            // and sets the time limits so nothing actually moves
            initializePath();

            // set up the big red button
            gameObject.name = "sortingManager";
            buttonPrefabString = "Prefabs/BigRedButton";
            buttonPrefab = Resources.Load(buttonPrefabString) as GameObject;
            myButton = Instantiate(buttonPrefab, new Vector3(0.0f, -0.3f, 2.0f),
                Quaternion.Euler(-90f, 0f, 0.0f));
            myButton.AddComponent<buttonCallback>();
            GameObject.Find("button").GetComponent<Renderer>().material.color = Color.red;

            // this moves the objects to a scrambled location
            scrambleProjectedPosition();
            //resetPositions(); //, orderList);

        }
        

        // Update is called once per frame
        void Update()
        {
            float myTime;
            myTime = Time.time;

        }


        public void destroyMarkers()
        {
            for (int i = 0; i < nObjects; i++)
            {
                Destroy(markers[i]);
            }
        }


        void initializePath()
        {

            for (int i = 0; i < nObjects; i++)
            {
                matchData[i].theObject.GetComponent<moveObjects>().StartPos = matchData[i].theObject.transform.position;
                matchData[i].theObject.GetComponent<moveObjects>().MidPos = new Vector3(0.001f, 0.001f, 0.001f);
                matchData[i].theObject.GetComponent<moveObjects>().FinalPos = new Vector3(0.001f, 0.001f, 0.001f);

                matchData[i].theObject.GetComponent<moveObjects>().StartSize = matchData[i].theObject.transform.localScale;
                matchData[i].theObject.GetComponent<moveObjects>().FinalSize = matchData[i].theObject.transform.localScale;

                matchData[i].theObject.GetComponent<moveObjects>().StartAngle = matchData[i].theObject.transform.eulerAngles;
                matchData[i].theObject.GetComponent<moveObjects>().FinalAngle = matchData[i].theObject.transform.eulerAngles;

                // this disables the move
                matchData[i].theObject.GetComponent<moveObjects>().TimeRange = new Vector2(-100.0f, -90.0f);

            }
        }

        void scrambleProjectedPosition()
        {
            for (int i = 0; i < nObjects; i++)
                matchData[i].fractionalDistance = UnityEngine.Random.value;
        }
    }

}
