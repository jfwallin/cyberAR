using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

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

    [Header("Scene References")]
    [SerializeField]
    private Transform rootUITransform = null;

    private Camera mainCam = null;

    [SerializeField]
    private string initDataString = null;
    private MCQ.MCExerciseData initData = null;

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
        Debug.Log("Spawning Media Player, setting it to the welcome image");
        //Place the media player in front of the camera, hopefully this is a good position for now.
        //also get a reference to the media player
        aPlayer = Instantiate(mediaPlayerPrefab, rootUITransform.InverseTransformPoint(mainCam.transform.position + Vector3.forward), Quaternion.identity, rootUITransform).GetComponentInChildren<AudioPlayer>();

        //Show beginning of lab splash screen
        string[] mediaCallInfo = new string[] { "_welcome", 2.ToString() /*MediaType.Image.ToString()*/ };
        //Call the media player, do nothing when the image "finishes" displaying
        aPlayer.MediaManager(mediaCallInfo, () => { });

        Debug.Log("Spawning StartButton, waiting for user press");
        //Add start button
        GameObject button = GameObject.Instantiate(labStartButtonPrefab, rootUITransform.InverseTransformPoint(aPlayer.transform.position + Vector3.back * 0.2f), Quaternion.identity, rootUITransform);
        labStartButton = button.GetComponentInChildren<Button>();
        labStartButton.onClick.AddListener(()=> StartLab(labStartButton));
    }

    public void StartLab(Button labStartButton)
    {
        Debug.Log("Start Lab Button pressed, playing intro video and removing the start button");
        labStartButton.onClick.RemoveAllListeners();

        //Play intro video
        string[] mediaCallInfo = new string[] { "moonphase-intro", 1.ToString() /*MediaType.Video.ToString()*/ };
        aPlayer.MediaManager(mediaCallInfo, IntroMediaComplete);

        //Remove the start button
        GameObject.Destroy(labStartButton.transform.parent.gameObject);
    }

    public void IntroMediaComplete()
    {
        Debug.Log("Intro media done playing, spawning and initializing the MCQ manager");
        mcqManager = Instantiate(mcqPrefab, rootUITransform.InverseTransformPoint(aPlayer.transform.position + Vector3.right*2.5f), Quaternion.identity, rootUITransform).GetComponent<MCQ.MCQManager>();
        mcqManager.Initialize(initData, aPlayer);
    }
}
