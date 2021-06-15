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

public class sortingActivity: MonoBehaviour
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



    public static int nObjects;

    // declare the sort info array
    public sortInfo[] sortData;

    public string objectTag = "sortable";
    public float xstart, ystart, zstart;
    public float xend, yend, zend;
    public GameObject[] gameObjects;
    public Vector3[] sortPts;

    bool isSorted;

    // default values for the delay, move time, and flourish of the movements
    public float tdelay = 2.0f;
    public float tmove = 5.0f;
    public int pretty = 1;

    public const int maxObjects = 10;
    public GameObject myPrefab;
    public Texture[] myTexture = new Texture[maxObjects];
    public String[] tnames = new String[maxObjects];
    public GameObject[] markers = new GameObject[maxObjects];
    public float mscale = 0.1f;
    public GameObject markerPrefab;

    //public AudioClip grab;
    public const int maxWrongAnswers = 5;
    public AudioClip audioInstructions;
    public AudioClip[] wrongOrder = new AudioClip[maxWrongAnswers];
    public AudioClip correctOrder;
    public int wrongAnswerCount = 0;
    public int totalWrongAnswer = 0;

    //public GameObject theButton;
    public GameObject myButton;
    
    private bool feedbackEnabled = true;


    // Start is called before the first frame update
    void Start()
    {

        AudioSource aud = GetComponent<AudioSource>();
        //yield return new WaitForSeconds(audio.clip.length);
        aud.clip = audioInstructions;
        aud.Play();

        // create data
        // find all the game objects to be sorted
        //gameObjects = GameObject.FindGameObjectsWithTag("sortable");
        //nObjects = gameObjects.Length;

        // find the number of objects to be sorted
        nObjects = 0;
        for (int i = 0; i < maxObjects; i++)
            if (myTexture[i] != null) nObjects++;


        // find the actual number wrong Answers
        totalWrongAnswer = 0;
        for (int i = 0; i < maxWrongAnswers; i++)
            if (wrongOrder[i] != null) totalWrongAnswer++;

        //nObjects = 5;
        //totalWrongAnswers = 3;


        // this define the points where objects should be located
        setSortedLocations(distance: 2.2f, angle: 0.0f, height: 0.0f,
            width: 1.5f, xoffset: 0.0f, zoffset: 0.0f);

        for (int i = 0; i < nObjects; i++)
        {
            mscale = 0.07f;
            markers[i] = Instantiate(markerPrefab, sortPts[i] - new Vector3(0.0f, 0.2f, 0.0f), Quaternion.identity) as GameObject;
            markers[i].transform.localScale = new Vector3(mscale, mscale, mscale);
            markers[i].GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        }

        // set the array to be unsorted
        isSorted = false;

        // creates the sortable panels
        createSortables();

        // create an array to help with the sorting
        sortData = new sortInfo[nObjects];

        // populate the sorting array with needed information
        for (int i = 0; i < nObjects; i++)
        {
            sortData[i] = new sortInfo();
            sortData[i].theObject = gameObjects[i];
            sortData[i].sortedOrder = nObjects - i - 1;
            sortData[i].fractionalDistance = 0.0f;
            sortData[i].isSorted = false;

            // add the moveObject script to the sortable objects
            sortData[i].theObject.AddComponent<moveObjects>();

        }

        // this sets the intial position to the current position of the objects
        // and sets the time limits so nothing actually moves
        initializePath();
        //
        gameObject.name = "sortingManager";

        myButton =  GameObject.Find("sortbutton");
        myButton.AddComponent<buttonCallback>();
        myButton.transform.localPosition = new Vector3(0.0f, -0.3f, 2.0f);
        myButton.transform.localRotation = Quaternion.Euler(-90f, 0f, 0.0f);
        myButton.GetComponent<Renderer>().material.color = Color.red;
        GameObject.Find("sbutton").GetComponent<Renderer>().material.color = Color.red;


        // this moves the objects to a scrambled location
        scrambleProjectedPosition();
        resetPositions(); //, orderList);

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

    void createSortables()
    {
        float oscale = 0.02f;
        gameObjects = new GameObject[nObjects];
        for (int i = 0; i < nObjects; i++)
        {
            gameObjects[i] = Instantiate(myPrefab, sortPts[i], Quaternion.identity) as GameObject;
            gameObjects[i].transform.eulerAngles = new Vector3(90.0f, 180.0f, 0.0f);
            gameObjects[i].transform.localScale = new Vector3(2.0f * oscale, 1.0f * oscale, 1.0f * oscale);
            //gameObjects[i].tag = "sortable";

            gameObjects[i].GetComponent<Renderer>().material.mainTexture = myTexture[i]; // theTexture;
            gameObjects[i].name = tnames[i];
            gameObjects[i].GetComponent<Rigidbody>().useGravity = false;

            // gameObjects[i].AddComponent<InputFeedback>();


        }

    }

    void setSortedLocations(float distance = 3.0f, float angle = 0.0f, float height = 0.0f, float width = 5.0f, float xoffset = 0.0f, float zoffset = 0.0f)
    {

        float xcenter, ycenter, zcenter;
        float angleStart;
        float angleEnd;
        float angleOffset;
        float angleCenter;

        // convert the direction of the sorting line to be in radians
        angleCenter = angle * Mathf.PI / 180.0f;

        // find the center of the sorting line
        // height in the Unity environment is y, not z
        xcenter = distance * Mathf.Sin(angleCenter) + xoffset;
        zcenter = distance * Mathf.Cos(angleCenter) + zoffset;
        ycenter = height;

        // find the angular directions for the left and right side of the sorting line
        angleOffset = Mathf.Atan(width / 2.0f / distance);
        angleStart = angleCenter + angleOffset;
        angleEnd = angleCenter - angleOffset;

        // find the starting and ending positions of the sorting line
        xstart = distance * Mathf.Sin(angleStart) / Mathf.Cos(angleOffset) + xoffset;
        zstart = distance * Mathf.Cos(angleStart) / Mathf.Cos(angleOffset) + zoffset;
        ystart = height;

        xend = distance * Mathf.Sin(angleEnd) / Mathf.Cos(angleOffset) + xoffset;
        zend = distance * Mathf.Cos(angleEnd) / Mathf.Cos(angleOffset) + zoffset;
        yend = height;

        // initialize the array of sorting locaitons
        sortPts = new Vector3[nObjects];

        // determine the correct positions for the elements 
        float dx = (xend - xstart) / (float)(nObjects - 1);
        float dz = (zend - zstart) / (float)(nObjects - 1);
        for (int i = 0; i < nObjects; i++)
        {
            sortPts[i] = new Vector3(xstart + dx * i, height, zstart + dz * i);
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
    }


    void resetPositions()
    {
        float myTime;
        myTime = Time.time;
        float a1, a2, a3;



        // sort by the projected fractional order
        Array.Sort(sortData, delegate (sortInfo s1, sortInfo s2) {
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

        setProjectedLocation();
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


