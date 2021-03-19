using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class LabControl : MonoBehaviour
{
    private Camera mainCam = null;
    [SerializeField]
    private GameObject labStartButtonPrefab = null;
    private Button labStartButton = null;
    [SerializeField]
    private GameObject mediaPlayerPrefab = null;
    private MediaPlayer mPlayer = null;
    [SerializeField]
    private GameObject mcqPrefab = null;
    private MCQ.MCQManager mcqManager = null;

    //private LabData labData = null;

    public void Awake()
    {
        Assert.IsNotNull(labStartButton);
        Assert.IsNotNull(mediaPlayerPrefab);
        Assert.IsNotNull(mcqPrefab);
        mainCam = Camera.main;
    }

    public void Start()
    {
        //Place the media player in front of the camera, hopefully this is a good position for now.
        //also get a reference to the media player
        mPlayer = Instantiate(mediaPlayerPrefab, mainCam.transform.position + Vector3.forward, Quaternion.identity).GetComponent<MediaPlayer>();

        //Show beginning of lab splash screen
        mPlayer.PlayMedia("_welcome", MCQ.MediaType.Image, IntroMediaComplete);

        //Add start button
        Button labStartButton = Instantiate(labStartButtonPrefab, mPlayer.transform.position + Vector3.down * 0.5f, Quaternion.identity).GetComponent<Button>();
        labStartButton.onClick.AddListener(StartLab);
    }

    public void StartLab()
    {
        labStartButton.onClick.RemoveAllListeners();
    }

    public void IntroMediaComplete()
    {
        mcqManager = Instantiate(mcqPrefab, mPlayer.transform.position + Vector3.right, Quaternion.identity).GetComponent<MCQ.MCQManager>();
        MCQ.MCExerciseData initData = new MCQ.MCExerciseData(); //This will be a JSON deserialization.
        mcqManager.Initialize(initData, mPlayer);
    }
}
