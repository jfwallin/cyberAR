using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestVideo : MonoBehaviour
{
    public string Vname { set; get; }
    public int Flag { set; get; }
    public UnityEngine.Video.VideoPlayer videoPlayer;
    // Start is called before the first frame update
    // VideoPlayer videoPlayer;
    // VideoClip videoClip;
    public void StartV(string Vname)
    {
        this.Vname = Vname;
        if (Vname != null)

            print($"I recieve a signal {Vname} and Flag is {Flag}");
        if (Flag == 0)
        {
            Flag = 1;
            Start();
        }
    }
    public void StopV(int VStop)
    {

        Flag = VStop;
        print($"I recieve a signal {Vname} and Flag is {Flag}");
        Start();
    }

    void Start()
    {

        //Stop all audio sources before start the video
        AudioSource[] allAudios = Camera.main.gameObject.GetComponents<AudioSource>();
        foreach (AudioSource audioS in allAudios)
        {
            audioS.Stop();
        }
        print($"I enter the start and falg is {Flag}");
        // Will attach a VideoPlayer to the main camera.
        GameObject camera = GameObject.Find("Main Camera");

        // VideoPlayer automatically targets the camera backplane when it is added
        // to a camera object, no need to change videoPlayer.targetCamera.
        videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();
        if (Flag == 1)
        {
            // print($"I enter the start and falg is {Flag}");
            // Will attach a VideoPlayer to the main camera.
            // GameObject camera = GameObject.Find("Main Camera");

            // VideoPlayer automatically targets the camera backplane when it is added
            // to a camera object, no need to change videoPlayer.targetCamera.
            // var videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();

            // Play on awake defaults to true. Set it to false to avoid the url set
            // below to auto-start playback since we're in Start().
            videoPlayer.playOnAwake = false;

            // By default, VideoPlayers added to a camera will use the far plane.
            // Let's target the near plane instead.
            videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;

            // This will cause our Scene to be visible through the video being played.
            videoPlayer.targetCameraAlpha = 0.5F;

            // Set the video to play. URL supports local absolute or relative paths.
            // Here, using absolute.
            // VideoClip myclip = Resources.Load<VideoClip>("Video/moonphase-final.mp4") as VideoClip;
            //VideoPlayer.Referen = myclip;
            //myclip.Play();


            //videoPlayer.url =  "Assets/Resources/Video/moonphase-final.mp4";
            //videoPlayer.url = "Assets/Resources/Video/moonphase-intro.mp4";
            videoPlayer.url = "Assets/Resources/Video/" + Vname;

            // Skip the first 100 frames.
            videoPlayer.frame = 100;

            // Restart from beginning when done.
            videoPlayer.isLooping = false;

            // Each time we reach the end, we slow down the playback by a factor of 10.
            //  videoPlayer.loopPointReached += EndReached;

            // Start playback. This means the VideoPlayer may have to prepare (reserve
            // resources, pre-load a few frames, etc.). To better control the delays
            // associated with this preparation one can use videoPlayer.Prepare() along with
            // its prepareCompleted event.
            // if (Flag == 1)
            // {
            videoPlayer.Play();
            // }
            // else
            // {
            //   videoPlayer.Stop();
            // Time.timeScale = 1f;
            // }
        }
        else
        {

            if (Flag != 1 && videoPlayer.isPlaying == true)
            {
                //     videoPlayer.Pause();
            }
        }


    }


    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        vp.playbackSpeed = vp.playbackSpeed / 10.0F;
    }


    // Update is called once per frame
    void Update()
    {
        //GameObject camera = GameObject.Find("Main Camera");
        //var videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
        //var videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();
        if (Flag != 1)
        {
            print($"I enter the video stop else with {Flag}");
             videoPlayer.Pause();
        }

    }
}
