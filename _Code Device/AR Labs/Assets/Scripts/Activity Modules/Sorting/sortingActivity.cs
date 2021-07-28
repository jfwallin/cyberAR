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
    public class sortingActivity : ActivityModule
    {

        public class sortInfo
        {
            public GameObject theObject;
            public int sortedOrder;
            public bool isSorted;

            public float fractionalDistance;

            // implement IComparable interface
            public int CompareTo(object obj)
            {
                if (obj is sortInfo)
                {
                    return this.fractionalDistance.CompareTo((obj as sortInfo).fractionalDistance);  // compare user names
                }
                else
                {
                    return 0;
                }
                //throw new ArgumentException("Object is not a sortInfo");
            }
        }

        // declare the sort info array
        private sortInfo[] sortData;


        private static int nObjects;
        private bool isSorted;

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
        private InstructionBox ibox;

        //public override void Initialize(ActivityModuleData dataIn)
        public override void Initialize(string jsonData)
        {
            // save the json string into a private variable
            moduleData = new sortingActivityData();
            jsonString = jsonData;
            JsonUtility.FromJsonOverwrite(jsonData, moduleData);

            ibox = InstructionBox.Instance;

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
            bridge = new Bridge();
            if (moduleData.createObjects)
                bridge.makeObjects(moduleData.objects);

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



        // Start is called before the first frame update
        void Start()
        {


            ObjectInfoCollection objList = JsonUtility.FromJson<ObjectInfoCollection>(jsonString);
            nObjects = objList.objects.Length;
            sortPts = new Vector3[nObjects];
            for (int i= 0; i < nObjects; i++)
            {
                Debug.Log(i.ToString() + " : " + objList.objects[i].name);
                sortPts[i] = objList.objects[i].position;
            }
            

            // find the actual number wrong Answers
            totalWrongAnswer = 0;
            for (int i = 0; i < nObjects; i++)
                if (wrongOrder[i] != null) totalWrongAnswer++;


            mscale = 0.07f;
            voffset = 0.2f;
            createMarkers(mscale, voffset);


            // set the array to be unsorted
            isSorted = false;

            // create an array to help with the sorting
            sortData = new sortInfo[nObjects];

            
            // populate the sorting array with needed information
            for (int i = 0; i < nObjects; i++)
            {
                sortData[i] = new sortInfo();
                sortData[i].theObject = GameObject.Find( objList.objects[i].name);
                //sortData[i].theObject = gameObjects[i];
                sortData[i].sortedOrder = i; // nObjects - i - 1;
                sortData[i].fractionalDistance = 0.0f;
                sortData[i].isSorted = false;

                // add the moveObject script to the sortable objects
                sortData[i].theObject.AddComponent<moveObjects>();

            }

    
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
            resetPositions(); //, orderList);

        }
        
        public void createMarkers(float mscale, float voffset)
        { 
            
            markers = new GameObject[nObjects];
            for (int i = 0; i < nObjects; i++)
            {
                mscale = 0.07f;
                markerPrefab = Resources.Load("Prefabs/tinysphere") as GameObject;
                markers[i] = Instantiate(markerPrefab, sortPts[i] - new Vector3(0.0f, voffset, 0.0f), Quaternion.identity) as GameObject;
                markers[i].transform.localScale = new Vector3(mscale, mscale, mscale);
                markers[i].GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f, 1.0f);
            }


        }
        public void destroyMarkers()
        {
            for (int i = 0; i < nObjects; i++)
            {
                Destroy(markers[i]);
            }
        }


        IEnumerator buttonWait(float dTime)
        {
            // this routine disables the click interaction so the move routine
            // doesn't get suck in the middle of a resort

            feedbackEnabled = false;
            yield return new WaitForSeconds(dTime);
            feedbackEnabled = true;
        }

        public void feedbackOnOrder()
        {
            IEnumerator coroutine;

            // we can disable the feedback loop so clicks don't affect the system 
            // during a sort
            if (feedbackEnabled)
            {
                checkOrder();
                AudioSource aud = GetComponent<AudioSource>();
                if (isSorted)
                {
                    aud.clip = correctOrder;
                    setOrderLights();
                    pretty = 1;

                    aud.Play();
                    coroutine = taskCompleted();
                    StartCoroutine(coroutine);
                    //taskCompleted();

                }
                else
                {
                    // this moves the objects to a scrambled location
                    setOrderLights();
                    scrambleProjectedPosition();
                    tdelay = 5.0f;
                    tmove = 3.0f;
                    pretty = 1;
                    resetPositions(); //, orderList);
                    coroutine = clearOrderLights();
                    StartCoroutine(coroutine);

                    //yield return new WaitForSeconds(audio.clip.length);
                    aud.clip = wrongOrder[wrongAnswerCount];

                    if (wrongAnswerCount + 1 < totalWrongAnswer)
                        wrongAnswerCount = wrongAnswerCount + 1;
                    aud.Play();
                }
            }
        }

        IEnumerator taskCompleted()
        {

            float dtime;

            dtime = 5.0f;
            yield return new WaitForSeconds(dtime);
            //Debug.Log("sorting is done!");
            for (int i = 0; i < nObjects; i++)
            {
                Destroy(sortData[i].theObject);
                Destroy(markers[i]);
            }
            Destroy(myButton);

            GameObject jj = GameObject.Find("Lab Control");
            jj.GetComponent<LabControl>().sortingDone();

        }




        public void resort()
        {

            Debug.Log("in resort.....");

            // find where the objects are along the projected path
            setProjectedLocation();

            pretty = 0;
            tdelay = 0.50f;
            tmove = 1.5f;
            resetPositions();
        }

        IEnumerator clearOrderLights()
        {
            float delaytime = 5.0f;

            yield return new WaitForSeconds(delaytime);

            for (int i = 0; i < nObjects; i++)
                markers[i].GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f, 1.0f);

        }

        void setOrderLights()
        {
            for (int i = 0; i < nObjects; i++)
            {
                if (sortData[i].isSorted)
                    markers[i].GetComponent<Renderer>().material.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
                else
                    markers[i].GetComponent<Renderer>().material.color = Color.red;
            }

        }

        void checkOrder()
        {
            isSorted = true;
            for (int i = 0; i < nObjects; i++)
            {
                if (i == sortData[i].sortedOrder)
                {
                    sortData[i].isSorted = true;
                }
                else
                {
                    sortData[i].isSorted = false;
                    isSorted = false;
                }
            }
        }

        void scrambleProjectedPosition()
        {
            for (int i = 0; i < nObjects; i++)
                sortData[i].fractionalDistance = UnityEngine.Random.value;
        }


        int[] findDisplacedObject()
        {
            float distance, minDistance;
            Vector3 dr;
            int[] closestMarker = new int[nObjects];
            float[] closestDistance = new float[nObjects];

            // find the closest points for all the objects
            for (int i = 0; i < nObjects; i++)
            {
                minDistance = 10000.0f;
                for (int j = 0; j < nObjects; j++)
                {
                    dr = sortData[i].theObject.transform.position - sortPts[j];
                    distance = Mathf.Sqrt(Vector3.Dot(dr, dr));
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestMarker[i] = j;
                        closestDistance[i] = minDistance;
                    }
                }
            }

            // find the closest points for all the objects
            for (int i = 0; i < nObjects; i++)
            {
                minDistance = 10000.0f;
                for (int j = 0; j < nObjects; j++)
                {
                    dr = sortData[i].theObject.transform.position - sortPts[j];
                    distance = Mathf.Sqrt(Vector3.Dot(dr, dr));
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestMarker[i] = j;
                        closestDistance[i] = minDistance;
                    }
                }
            }

            float d1;
            int i1;
            int dObject = 0;
            // find the object that is the furthest away from the target points
            d1 = 0.0f;
            i1 = 0;
            for (int i = 0; i < nObjects; i++)
            {
                if (closestDistance[i] > d1)
                {
                    dObject = i;
                    i1 = closestMarker[i];
                    d1 = closestDistance[i];
                }
            }


            // now find the second closest target point for the displaced object
            float d2;
            int i2;

            d2 = 100000.0f;
            i2 = 0;
            for (int j = 0; j < nObjects; j++)
            {
                if (j != i1)
                {
                    dr = sortData[dObject].theObject.transform.position - sortPts[j];
                    distance = Mathf.Sqrt(Vector3.Dot(dr, dr));
                    if (distance < d2)
                    {
                        d2 = distance;
                        i2 = j;
                    }
                }
            }
            
            return new int[3] { dObject, i1, i2 };

        }




        void setProjectedLocation()
        {


            int[] pdata;
            pdata = findDisplacedObject();

            int displacedObject = pdata[0];
            int firstClosestPt = pdata[1];
            int secondClosestPt = pdata[2];

            //print("new position " + displacedObject.ToString() + "   " + firstClosestPt.ToString() + "  -  " + secondClosestPt.ToString());

            // reset the fractional distances 
            for (int i = 0; i < nObjects; i++)
            {
                sortData[i].fractionalDistance = (float) i;
            }

            float eps = 0.01f;  // this is a small number to shift and object for resorting
            // case 1 -  the first closest point located to the right of the second closet point 
            if (firstClosestPt > secondClosestPt)
            {
                // move the object currently at the firstClosest point to the right
                if (firstClosestPt < nObjects - 1)
                {
                    sortData[displacedObject].fractionalDistance = (float)firstClosestPt - eps;
                }
                else
                {
                    sortData[displacedObject].fractionalDistance = (float)firstClosestPt + eps;
                }
            }
            else
            // case 2 - first closest point located to the left of the second closest point
            {
                // move the object currently at the firstClosest point to the left
                if (firstClosestPt > 0)
                {
                    sortData[displacedObject].fractionalDistance = (float)firstClosestPt + eps;
                }
                else
                { 
                    sortData[displacedObject].fractionalDistance = (float)firstClosestPt - eps;
                }
            }

        }



        /*
        void setProjectedLocation()
        {
            // find the project position of an object along the direction of the sorted locations line

            // projected distance along a line r for a vector p is given by
            // distance = r dot p / r
            // the fractional distance is r dot p / r**2

            for (int i = 0; i < nObjects; i++)
            {
                Vector3 targetPosition = sortData[i].theObject.transform.position;
                Vector3 projectedPath = new Vector3(xend - xstart, yend - ystart, zend - zstart);
                float fractionalDistance = Vector3.Dot(targetPosition, projectedPath) /
                Vector3.Dot(projectedPath, projectedPath);
                sortData[i].fractionalDistance = fractionalDistance;
            }
        }*/


        void resetPositions()
        {
            float myTime;
            myTime = Time.time;
            float a1, a2, a3;



            // sort by the projected fractional order
            Array.Sort(sortData, delegate (sortInfo s1, sortInfo s2)
            {
                return s1.fractionalDistance.CompareTo(s2.fractionalDistance);
            });

            // initialize the moveObject variables to safe default values
            initializePath();

            // set up the moveObject scripts to move the objects
            for (int i = 0; i < nObjects; i++)
            {

                // set the final positions of the particles to be the target locations
                sortData[i].theObject.GetComponent<moveObjects>().FinalPos = sortPts[i];

                // move them in an indirect path or a direct path
                if (pretty == 1)
                {

                    // pick a midpoint scattered around the middle of the path
                    float rrange = 0.85f;
                    sortData[i].theObject.GetComponent<moveObjects>().MidPos = (sortData[i].theObject.transform.position + sortPts[i]) * 0.5f +
                    new Vector3(UnityEngine.Random.Range(-rrange, rrange), UnityEngine.Random.Range(0, rrange), UnityEngine.Random.Range(-rrange, rrange));

                    // this adds a nice spin during the sort - a1 should be 90 or 90 + 360*n
                    a1 = 90;
                    a2 = 180;
                    a3 = 0;
                    sortData[i].theObject.GetComponent<moveObjects>().FinalAngle = new Vector3(a1, a2, a3);

                }
                else
                {

                    // pick a midpoint between the points
                    sortData[i].theObject.GetComponent<moveObjects>().MidPos = (sortData[i].theObject.transform.position +
                        sortPts[i]) * 0.5f;

                    a1 = 90;
                    a2 = 180;
                    a3 = 0;
                    sortData[i].theObject.GetComponent<moveObjects>().FinalAngle = new Vector3(a1, a2, a3);
                }

                // start the move after a delay and finishing in a specified time
                sortData[i].theObject.GetComponent<moveObjects>().TimeRange = new Vector2(myTime + tdelay, myTime + tdelay + tmove);

                // this is the last thing to do to make it all work
                sortData[i].theObject.GetComponent<moveObjects>().initializePath();

            }
            IEnumerator pausePointer = buttonWait(tdelay + tmove);
            StartCoroutine(pausePointer);
        }



        void initializePath()
        {

            for (int i = 0; i < nObjects; i++)
            {
                sortData[i].theObject.GetComponent<moveObjects>().StartPos = sortData[i].theObject.transform.position;
                sortData[i].theObject.GetComponent<moveObjects>().MidPos = new Vector3(0.001f, 0.001f, 0.001f);
                sortData[i].theObject.GetComponent<moveObjects>().FinalPos = new Vector3(0.001f, 0.001f, 0.001f);

                sortData[i].theObject.GetComponent<moveObjects>().StartSize = sortData[i].theObject.transform.localScale;
                sortData[i].theObject.GetComponent<moveObjects>().FinalSize = sortData[i].theObject.transform.localScale;

                sortData[i].theObject.GetComponent<moveObjects>().StartAngle = sortData[i].theObject.transform.eulerAngles;
                sortData[i].theObject.GetComponent<moveObjects>().FinalAngle = sortData[i].theObject.transform.eulerAngles;

                // this disables the move
                sortData[i].theObject.GetComponent<moveObjects>().TimeRange = new Vector2(-100.0f, -90.0f);

            }
        }

        void testMove()
        {

            //setProjectedLocation();
            //sortData[3].fractionalDistance = -10f;
            sortData[0].fractionalDistance = 100f;

            pretty = 0;
            tdelay = 3.0f;
            tmove = 3.0f;
            resetPositions();
        }


        // Update is called once per frame
        void Update()
        {
            float myTime;
            myTime = Time.time;

        }
    }

}
