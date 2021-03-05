using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LabControl : MonoBehaviour
{
    private Camera mainCam = null;
    [SerializeField]
    private GameObject mediaPlayerPrefab = null;
    private MediaPlayer mPlayer = null;
    [SerializeField]
    private GameObject mcqPrefab = null;
    private MCQ.MCQManager mcqManager = null;

    public void Awake()
    {
        Assert.IsNotNull(mediaPlayerPrefab);
        Assert.IsNotNull(mcqPrefab);
        mainCam = Camera.main;
    }

    public void StartLab()
    {
        mPlayer = Instantiate(mediaPlayerPrefab, mainCam.transform.position + Vector3.forward, Quaternion.identity).GetComponent<MediaPlayer>();
        mPlayer.PlayMedia("introVid", MCQ.MediaType.Video, IntroMediaComplete);
    }

    public void IntroMediaComplete()
    {
        mcqManager = Instantiate(mcqPrefab, mPlayer.transform.position + Vector3.right, Quaternion.identity).GetComponent<MCQ.MCQManager>();
        MCQ.MCExerciseData initData = new MCQ.MCExerciseData(); //This will be a JSON deserialization.
        mcqManager.Initialize(initData, mPlayer);
    }
}
