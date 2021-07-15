using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class videoplayer2 : MonoBehaviour
{
    public string path = "Assets/Resources/test.txt";
    //Write some text to the test.txt file
    //Create instance of WrtieToFile
    public WrtieToFile writeToFile ;

    public string VideoName { set; get; }
    public int Flag { set; get; }
    //public UnityEngine.Video.VideoPlayer videoPlayer;
   public VideoPlayer myVideoPlayer { set; get; }
    public VideoClip myclip { set; get; }
    // get video name from control switch
    public void StartV(string VideoName)
    {
       // StreamWriter writer = new StreamWriter(path, true);
        this.VideoName = VideoName;
        myVideoPlayer = GetComponent<VideoPlayer>();
        if (VideoName != null)

            print($"I recieve a signal {VideoName} and Flag is {Flag}");
     //  writeToFile.WriteString(VideoName, System.DateTime.Now.ToString());
       // writer.WriteLine($"I recieve a signal {VideoName} and time is {System.DateTime.Now}");
       // writer.Close();
        if (Flag != 1)
        {
            Flag = 1;
            StartVideo();
        }
        else
            StartVideo();
    }
    public void StopV(int VStop)
    {

        // This will cause the video to pause
        // if I use Stop the video will not start again
        print($"I recieve a signal {VideoName} and Flag is {Flag}");
        if (myVideoPlayer.isPlaying)
        {
            myVideoPlayer.Pause();
        }
        else
        {
            myVideoPlayer.Play();
        }
        
    }

    void StartVideo()
    {

        //Stop all audio sources before start the video

        AudioSource[] allAudios = Camera.main.gameObject.GetComponents<AudioSource>();
        foreach (AudioSource audioS in allAudios)
        {
            audioS.Stop();
        }
        print($"I enter the start and falg is {Flag}");


       // Play video useing the name from a UI control
        myVideoPlayer = GetComponent<VideoPlayer>();
        myVideoPlayer.clip = Resources.Load<VideoClip>("Video/"+VideoName);

        //When first video stop if you hit play vidoe it will start the new video
        if ( (myVideoPlayer.frame > 0 && myVideoPlayer.isPlaying == false))
        {
            nextV();
        }

        // nextV();

    }


   

    public void nextV()
    {
      //  myVideoPlayer = GetComponent<VideoPlayer>();
        myVideoPlayer.clip = Resources.Load<VideoClip>("Video/moonphase-intro");

    }

}


