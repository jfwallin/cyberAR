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
    private MediaPlayer aPlayer = null;
    [SerializeField]
    private GameObject mcqPrefab = null;
    private MCQ.MCQManager mcqManager = null;
    [SerializeField]
    private GameObject sortPrefab = null;
    private GameObject sortManager = null;
    [SerializeField]
    private GameObject demoPrefab = null;
    private GameObject demoObject = null;
    [SerializeField]
    private GameObject finalScreenPrefab = null;
    private GameObject finalScreen = null;

    [Header("Scene References")]
    [SerializeField]
    private Transform rootUITransform = null;
    private Camera mainCam = null;

    [SerializeField]
    private string initDataString = null;
    private MCQ.MCExerciseData initData = null;

    private enum labsStage { intro, mc, demo, sorter};

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
        //spawnStartLab();
        spawnSorting();
    }

    public void spawnStartLab()
    {
        Debug.Log("Spawning Media Player, setting it to the welcome image");
        //Place the media player centered in the root canvas. The root is following the headpose curently.
        //also get a reference to the media player
        aPlayer = Instantiate(mediaPlayerPrefab, Vector3.zero, Quaternion.identity, rootUITransform).GetComponentInChildren<MediaPlayer>();
        aPlayer.name = "MP"; // added a name so it can be located more easily

        //Add start button
        Debug.Log("Spawning StartButton, waiting for user press");
        GameObject button = GameObject.Instantiate(labStartButtonPrefab, rootUITransform.InverseTransformPoint(aPlayer.transform.position + Vector3.back * 0.2f + Vector3.down * 0.2f), Quaternion.identity, rootUITransform);

        labStartButton = button.GetComponentInChildren<Button>();
        labStartButton.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
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
        MediaInfo startLabMedia = new MediaInfo();
        startLabMedia.resource_type = MediaType.Video;
        //startLabMedia.id = ????
        //aPlayer.PlayMedia(startLabMedia, spawnMC);
    }

    public void spawnMC()
    {
        Debug.Log("Intro media done playing, spawning and initializing the MCQ manager");
        mcqManager = Instantiate(mcqPrefab, aPlayer.transform.position + aPlayer.transform.right * 1.5f, aPlayer.transform.rotation, rootUITransform).GetComponent<MCQ.MCQManager>();
        string tmpJson;
        tmpJson = JsonUtility.ToJson(initData);
        mcqManager.Initialize(tmpJson);
        //mcqManager.Initialize(initData);
    }

    public void MCCompleted()
    {
        GameObject.Find("MP").SetActive(false);
        GameObject.Find("Root Main Canvas").SetActive(false);
        spawnDemo();
    }

    public void spawnDemo()
    {
        demoObject = Instantiate(demoPrefab, Vector3.zero, Quaternion.identity);  //, rootUITransform);
    }

    public void demoCompleted()
    {
        Destroy(demoObject);
        spawnSorting();
    }

    public void spawnSorting()
    {
        GameObject theLight = GameObject.Find("Directional Light");
        Light sceneLight = theLight.GetComponent<Light>();
        sceneLight.color = Color.white;
        sortManager = GameObject.Instantiate(sortPrefab, Vector3.zero, Quaternion.identity);
    }

    public void sortingDone()
    {
        Destroy(sortManager);
        spawnFinalie();
    }

    public void spawnFinalie()
    {
        float oscale = 0.08f;
        finalScreen = GameObject.Instantiate(finalScreenPrefab, Vector3.zero + Vector3.forward * 2.0f, Quaternion.identity);
        finalScreen.transform.eulerAngles = new Vector3(90.0f, 180.0f, 0.0f);
        finalScreen.transform.localScale = new Vector3(2.0f * oscale, 1.0f * oscale, 1.0f * oscale);
    }
    public void finalieDone()
    {
        Debug.Log("@34532453");
        Application.Quit();
    }
}