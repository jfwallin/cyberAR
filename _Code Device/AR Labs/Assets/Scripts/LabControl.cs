using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

public class LabControl : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject labStartButtonPrefab = null;
    private Button labStartButton = null;
    [SerializeField]
    private GameObject mediaPlayerPrefab = null;
    private AudioPlayer aPlayer = null;
    [SerializeField]
    private GameObject mcqPrefab = null;
    private MCQ.MCQManager mcqManager = null;

    [SerializeField]
    private GameObject sortPrefab = null;
    private GameObject sortManager = null;

    [SerializeField]
    private GameObject demoPrefab = null;
    private GameObject demoObject = null;

    [Header("Scene References")]
    [SerializeField]
    private Transform rootUITransform = null;

    private Camera mainCam = null;

    [SerializeField]
    private string initDataString = null;
    private MCQ.MCExerciseData initData = null;

    private enum labsStage { intro, mc, demo, sorter};
    //private LabData labData = null;

    public void Awake()
    {
        Debug.Log("LabControl Awake, checking dependencies and parsing JSON");
        Assert.IsNotNull(labStartButtonPrefab);
        Assert.IsNotNull(mediaPlayerPrefab);
        Assert.IsNotNull(mcqPrefab);
        mainCam = Camera.main;
        initData = JsonUtility.FromJson<MCQ.MCExerciseData>(initDataString);
    }

    public void Start()
    {
        
        spawnStartLab();
        //spawnMedia();
        //spawnSorting();
        //spawnDemo();
    }



    public void spawnStartLab()
    {
        //spawnMedia();

        Debug.Log("Spawning Media Player, setting it to the welcome image");
        //Place the media player centered in the root canvas. The root is following the headpose curently.
        //also get a reference to the media player
        aPlayer = Instantiate(mediaPlayerPrefab, Vector3.zero, Quaternion.identity, rootUITransform).GetComponentInChildren<AudioPlayer>();
        aPlayer.name = "MP"; // added a name so it can be located more easily

        //Add start button
        Debug.Log("Spawning StartButton, waiting for user press");
        GameObject button = GameObject.Instantiate(labStartButtonPrefab, rootUITransform.InverseTransformPoint(aPlayer.transform.position + Vector3.back * 0.2f + Vector3.down *0.2f), Quaternion.identity, rootUITransform);
        
        labStartButton = button.GetComponentInChildren<Button>();
        labStartButton.transform.localScale= new Vector3(0.5f, 0.5f, 0.5f);
        labStartButton.onClick.AddListener(() => startLabDone(labStartButton));
        labStartButton.onClick.AddListener(() => rootUITransform.GetComponent<HeadposeCanvas>().enabled = false);
    }

  

    public void startLabDone(Button labStartButton)
    {
        Debug.Log("Start Lab Button pressed, playing intro video and removing the start button");
        labStartButton.onClick.RemoveAllListeners();
        //Remove the start button
        GameObject.Destroy(labStartButton.transform.parent.gameObject);

        //Play intro video
        string[] mediaCallInfo = new string[] { "moonphase-intro", 1.ToString() /*MediaType.Video.ToString()*/ };
        aPlayer.MediaManager(mediaCallInfo, spawnMC);

    }


    public void spawnMC()
    {
        Debug.Log("Intro media done playing, spawning and initializing the MCQ manager");
        mcqManager = Instantiate(mcqPrefab, aPlayer.transform.position + aPlayer.transform.right * 1.5f, aPlayer.transform.rotation, rootUITransform).GetComponent<MCQ.MCQManager>();
        mcqManager.Initialize(initData, aPlayer);
    }

    public void MCCompleted()
    {
        GameObject.Find("MP").SetActive(false);
        GameObject.Find("Root Main Canvas").SetActive(false);
        Debug.Log("back to lab control");
        spawnDemo();
    }

        public void spawnDemo()
    {
        demoObject = Instantiate(demoPrefab, Vector3.zero, Quaternion.identity);  //, rootUITransform);
    }

    public void demoCompleted()
    {
        spawnSorting();
    }

    public void spawnSorting()
    {
        //aPlayer.SetActive(false);
        GameObject theLight = GameObject.Find("Directional Light");
        Light sceneLight = theLight.GetComponent<Light>();
        sceneLight.color = Color.white;


        //go.transform.position = new Vector3(1000f, 1000f, 1000f);
        Debug.Log("starting the sort");
        sortManager = GameObject.Instantiate(sortPrefab, Vector3.zero, Quaternion.identity);
        

    }

    public void sortingDone()
    {

        GameObject.Find("Root Main Canvas").SetActive(true);

        // SHOULD PUT A MEDIA CLIP HERE OF THE END OF THE LAB
        Application.Quit();
        
    }


}


