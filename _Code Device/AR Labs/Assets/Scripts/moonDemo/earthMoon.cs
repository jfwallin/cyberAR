﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;

/*
 Home = Recenter content
Trigger = Grab
Touchpad Left/Right = Scale
Touchpad Up/Down = Nudge
Touchpad Radial = Rotate
Touchpad Forcepress = Reset
Reach out to extend pointer.
 */







public class earthMoon : MonoBehaviour
{
        Bridge bridge = new Bridge();
     public Vector3 earthPosition;
    private Vector3 moonPosition;
    public float orbitalDistance = 2;
    public float orbitalPeriod = 10;
    public float timeRate = 1.0f;
    private float orbitalRate;
    private float orbitalAngle;

    public float rotationTime = 0.4f;
    private float rotationRate;


    public GameObject myPrefab;


    public float mscale = 0.1f;

    //public AudioClip grab;

    public GameObject theMoon;
    public GameObject theEarth;

    public Texture earthTexture;
    public Texture moonTexture;

    public float earthScale = 0.2f;
    public float moonScale = 0.05f;



    public bool earthMoonEnabled = false;
    public GameObject theBigRedButton;

    public float systemScale;
    /*
     1. Students will see the relative speeds between the rotating earth and the revolving moon.
     2. Students will view the Earth moon system from above to understand how shadows appear on spherical objects
     3. Students will view the moon from the location of the earth to see how phases are created.
     4. Students will understand the TRUE scale earth-moon system.
      
     */


    const int startTheBridge = 0;
    const int startSimulation = 1;
    const int startBasketball = 2;
    const int basketball = 3;
    const int startTinyEarthMoon = 4;
    const int tinyEarthMoon = 5;
    const int bigEarthMoon = 6;
    const int pauseEarthMoon = 7;
    const int endSimulation  = 8;

    public int modulePhase ;
    
    public utility.lightControl theLight;
    public Texture ballTexture;
    public GameObject ballPrefab;
    public float ballSize = 0.5f;
    public GameObject theBall;

    public GameObject instructionPrefab;
    public GameObject instructionCanvas;
    public GameObject instructionHolder;

    public AudioSource aud;
    public AudioClip startSimulationAudio;
    public AudioClip startBasketballAudio;
    public AudioClip basketballAudio;
    public AudioClip startTinyEarthMoonAudio;
    public AudioClip tinyEarthMoonAudio;
    public AudioClip bigEarthMoonAudio;
    public AudioClip pauseEarthMoonAudio;

    public void HandleOnClick(string source)
    {
        Debug.Log("YYYYYYYYYYYYYYYYYYYYY");
        Debug.Log(source);
        modulePhase++;
        moduleSequenceManager();

    }

    public void HandleDragEnd(string source)
    {
        Debug.Log("WOOF WOOF!");
        Debug.Log(source);
    }


      

    // Start is called before the first frame update
    void Start()
    {
       
        aud = GetComponent<AudioSource>();
        theLight = new utility.lightControl();
        theLight.sunlight();

        theBigRedButton = GameObject.Find("ArcadeButton Variant");
        theBigRedButton.AddComponent<earthCallback>();
        theBigRedButton.transform.localPosition = new Vector3(0.5f, 1.3f, 1.0f);
        theBigRedButton.transform.localRotation= Quaternion.Euler(-90f, 20f, 0.0f);


        theBigRedButton.GetComponent<earthCallback>().callbackObject = gameObject;
        theBigRedButton.GetComponent<Renderer>().material.color = Color.red;
        GameObject.Find("button").GetComponent<Renderer>().material.color = Color.red;
        

        createInstructions();

        modulePhase = 0; // startSimulation;
        moduleSequenceManager();

    }

    void simulationDone()
    {
        theLight.restoreLights();
        Destroy(instructionCanvas);
        Destroy(theEarth);
        Destroy(theMoon);
        Destroy(theBigRedButton);
        aud.Stop();
        GameObject.Find("Lab Control").GetComponent<LabControl>().demoCompleted();
    }

    void createInstructions()
    {
        instructionHolder = Instantiate(instructionPrefab, new Vector3(0.3f, -0.2f, 1.0f), Quaternion.Euler(0.0f, 180.0f, 0.0f));
        instructionCanvas = GameObject.Find("MainInstructions");
        instructionCanvas.GetComponent<Text>().text = "";
    }



    void setInstructions(int instructionNumber)
    {
        switch (instructionNumber)
        {

            case (startTheBridge):
                instructionCanvas.GetComponent<Text>().text =
                    "Using the Bridge.\n"
                    + "Click on the Red Button to begin";
                //aud.clip = startSimulationAudio;
                //aud.Play();
                Debug.Log("startting the bridge");
                break;

            case (startSimulation):
                instructionCanvas.GetComponent<Text>().text =
                    "Exploring how moon phases are created.\n"
                    + "Click on the Red Button to begin";
                aud.clip = startSimulationAudio;
                aud.Play();
                Debug.Log("stage1");
                break;

            case (startBasketball):
                instructionCanvas.GetComponent<Text>().text =
                 "Notice the how the lighted part of the moon \n"
                + "always faces the same direction.\n"
                + "\n"
                + "- Click on the red button when you are done exploring.";
                aud.clip = startBasketballAudio;
                aud.Play();
                break;

            case (basketball):
                instructionCanvas.GetComponent<Text>().text =
                      "Explore how changing the location \n"
                    + "of the basketball changes the shadows.\n"
                    + "\n" 
                    +  "- Reach out to extend pointer. \n"
                    + "- Use the trigger to grab objects\n" 
                    + "- Click on the red button when you are done exploring.";
    
                aud.clip = basketballAudio;
                aud.Play();

                break;


            case (startTinyEarthMoon):
                instructionCanvas.GetComponent<Text>().text =
                "This model is not to scale! \n"
                + "Notice how rapidly the earth rotates \n"
                + "compared to how long it takes for the moon \n"
                + "to revolve in its orbit.\n"
                + "\n"
                + "- Reach out to extend pointer. \n"
                + "- Use the trigger to grab objects\n"
                + "- Click on the red button when you are done exploring.";
                aud.clip = startTinyEarthMoonAudio;
                aud.Play();
                break;

            case (tinyEarthMoon):
                instructionCanvas.GetComponent<Text>().text =
                "Place the small Earth-Moon System \n"
                + "so you can observer the motion and shadows from \n" 
                + "above.  Note how the shadows change during the orbit \n" 
                + "When you observer from above the North Pole.\n"
                + "\n"
                + "- Reach out to extend pointer. \n"
                + "- Use the trigger to grab objects\n"
                + "- Click on the red button when you are done exploring.";
                aud.clip = tinyEarthMoonAudio;
                aud.Play();
                break;

            case (bigEarthMoon):
                instructionCanvas.GetComponent<Text>().text =
                "Place the big Earth-Moon System \n"
                + "so the Earth is close to your head. \n"
                + "Notice how the shadow on the moon changes during its orbit.\n"
                + "View the moon from different angles as it moves in its orbit \n"
                + "to see how changing your perspective changes the shadows.\n"
                + "\n"
                 + "- Reach out to extend pointer. \n"
                 + "- Use the trigger to grab objects\n"
                 + "- Click on the red button when you are done exploring.";
                aud.clip = bigEarthMoonAudio;
                aud.Play();
                break;

            case (pauseEarthMoon):
                instructionCanvas.GetComponent<Text>().text =
                "Move around the simulation to notice two\n"
                + "Important things: \n"
                + "1) At any given time, the moon is only visible from half\n"
                + "   of the Earth's surface. \n"
                + "2) Everyone on Earth who can see the moon sees the same\n"
                + "   Moon phase.\n\n"
                 + "- Click on the red button when you are done exploring.";
                aud.clip = pauseEarthMoonAudio;
                aud.Play();
                break;

            case (endSimulation):
                //instructionCanvas.GetComponent<Text>().text =
                //    "That's all folks!";

                break;
        }


    }


    void useTheBridge()
    {
        //string jsonExample = "";
        //jsonExample = "{\"objects\":[{ \"name\": \"Earth\"}]"; //, \"parentName\": \"[_DYNAMIC]\", \"type\": \"sphere\", \"position\": { \"x\": 0.0, \"y\": 1.0, \"z\": -2.0 }, \"scale\": { \"x\": 0.25, \"y\": 0.25, \"z\": 0.25 }, \"material\": \"Earth\", \"transmittable\": false, \"componentsToAdd\": [ ] }]}"; 
        //string path = "C:/Users/jfwal/OneDrive/Documents/GitHub/cyberAR/_Code Device/AR Labs/Assets/Resources/scene-example.json";

        //StreamReader reader = new StreamReader(path);
        //string line;
        //jsonExample = reader.ReadToEnd();

       string url = "http://cyberlearnar.cs.mtsu.edu/get_file/scene/scene-example.json";
       StartCoroutine(GetRequest(url));

        //Debug.Log(jsonExample);
        //bridge.ParseJson(jsonExample);
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


    void destroyUseTheBridge()
    {

    }

    void createBall()
    {
        theBall = Instantiate(ballPrefab, new Vector3(0.0f, -0.2f, 1.5f), Quaternion.Euler(0.0f, 0.0f, 0.0f));
        theBall.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        theBall.GetComponent<Renderer>().material.mainTexture = ballTexture;
        theBall.transform.localScale = new Vector3(ballSize, ballSize, ballSize);

        theBall.AddComponent<simpleRotation>();
        theBall.name = "Basketball";

        Debug.Log(JsonUtility.ToJson(theBall,true)) ;

    }

    void destroyBall()
    {
        Destroy(theBall);
    }

    void createEarthMoon()
    {

        moonPosition = new Vector3(orbitalDistance, 0.0f, 0.0f);
        myPrefab.transform.localScale = new Vector3(earthScale, earthScale, earthScale);
        myPrefab.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
        myPrefab.GetComponent<Rigidbody>().useGravity = false;


        theEarth = Instantiate(myPrefab, earthPosition, Quaternion.identity) as GameObject;
        theEarth.SetActive(false);
        theEarth.name = "Earth";

        theEarth.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        theEarth.GetComponent<Renderer>().material.mainTexture = earthTexture;
        //Earth.GetComponent<Rigidbody>().useGravity = false;

        theEarth.AddComponent<earthCallback>();
        theEarth.GetComponent<earthCallback>().callBackMessage = "the earth is calling";

        myPrefab.transform.localScale = new Vector3(moonScale, moonScale, moonScale);
        myPrefab.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);

        theMoon = Instantiate(myPrefab, earthPosition + moonPosition, Quaternion.identity) as GameObject;
        theMoon.SetActive(true);
        theMoon.name = "Moon";
        theMoon.GetComponent<Renderer>().material.mainTexture = moonTexture;
        //Moon.GetComponent<Rigidbody>().useGravity = false;

    }

    void enableEarthMoon()
    {
        moonPosition = theMoon.transform.position;
        orbitalDistance = Mathf.Sqrt(Vector3.Dot(moonPosition, moonPosition));
        orbitalRate = Mathf.PI * 2.0f / orbitalPeriod;
        orbitalAngle = Mathf.Atan2(moonPosition[1], moonPosition[0]);
        rotationRate = Mathf.PI * 2.0f / rotationTime;

        theEarth.SetActive(true);
        theMoon.SetActive(true);
        earthMoonEnabled = true;

        Debug.Log("enabling!");
    }

    void disableEarthMoon()
    {
      
        theEarth.SetActive(false);
        theMoon.SetActive(false);
        earthMoonEnabled = false;
        Debug.Log("disabling");
    }
    void moduleSequenceManager()
    {
        switch (modulePhase)
        {
            case startTheBridge:
                useTheBridge();
                setInstructions(startTheBridge);
                break;

            case startSimulation:
                earthMoonEnabled = false;
                setInstructions(startSimulation);
                break;

            case startBasketball:
                ballSize = 0.25f;
                createBall();
                setInstructions(startBasketball);
                break;

            case basketball:
                setInstructions(basketball);
                break;

            case startTinyEarthMoon:
                destroyBall();
                orbitalDistance = 3.0f;
                createEarthMoon();
                systemScale = 0.1f;
                enableEarthMoon();
                theEarth.transform.position = new Vector3(-0.50f, -0.3f, 1.5f);
                setInstructions(startTinyEarthMoon);
                break;

            case tinyEarthMoon:
                setInstructions(tinyEarthMoon);
                break;

            case bigEarthMoon:
                systemScale = 0.8f;
                theEarth.transform.position = new Vector3(-0.50f, -0.3f, 2.0f);
                timeRate = 0.25f;
                setInstructions(bigEarthMoon);

                break;

            case pauseEarthMoon:
                setInstructions(pauseEarthMoon);
                timeRate = 0.0f;
                break;

            case endSimulation:
                disableEarthMoon();
                setInstructions(endSimulation);
                simulationDone();

                break;
        }
    }


 



    void animateEarthMoon()
    {
        float orbitTheta;
        float xpos, ypos, zpos;
        float rotationTheta;

        float currentDistance;
        float theScale;

        //Vector3 currentEarthScale = theEarth.transform.localScale;
         //theScale = currentEarthScale[0] / earthScale;
        currentDistance = orbitalDistance * systemScale;

        orbitTheta = orbitalAngle + orbitalRate * Time.time * timeRate;
        xpos = currentDistance * Mathf.Cos(orbitTheta);
        ypos = 0.0f;
        zpos = currentDistance * Mathf.Sin(orbitTheta);


        theScale = systemScale / earthScale;

        theMoon.transform.localPosition = new Vector3(xpos, ypos, zpos) + theEarth.transform.position; //+ transform.position;
        theMoon.transform.eulerAngles = new Vector3(0.0f, -orbitTheta * 180.0f / Mathf.PI + 180.0f, 0.0f);
        theMoon.transform.localScale = new Vector3(moonScale * theScale, moonScale * theScale, moonScale * theScale);

        rotationTheta = -rotationRate * Time.time * timeRate;
        theEarth.transform.eulerAngles = new Vector3(0.0f, rotationTheta * 180.0f / Mathf.PI, 0.0f);
        theEarth.transform.localScale = new Vector3(earthScale * theScale, earthScale * theScale, earthScale * theScale);
    }


    // Update is called once per frame
    void Update()
    {


        if (earthMoonEnabled)
        {
            animateEarthMoon();
        }


        
    }
}
