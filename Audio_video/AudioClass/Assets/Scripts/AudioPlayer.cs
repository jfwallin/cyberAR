using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.Video;
using UnityEngine;
using UnityEngine.UI;
//Audio Player 
// Recieve inputs as a string fro audio wave that reside in 
// Audio file in Assets/Resources file 
public enum MCQ { Audio, Video, Image }
public class AudioPlayer : MonoBehaviour
{

    public string Num { get; set; }
    public int PAUSE { get; set; }
    public int STOP { get; set; }
    public int NUM { set; get; }
    public System.Action localCallBack;
    public Sprite img1;  // This will hold the image 
    public VideoPlayer myVideoPlayer { set; get; }  // videplayer comp.
    public AudioClip myAudioClip;
    public InputType dataType { set; get; } = InputType.Media;
    //public string Path { set; get; } = "Assets/Resources/test2.txt";

    //Create instance of WrtieToFile
   // [SerializeField]
   // WriteAtEnd writeatEnd;
    [SerializeField]
    TestWrite testWrite;
    // This will create an instant for Write file
    public void Start()
    {
        testWrite = FindObjectOfType(typeof(TestWrite)) as TestWrite;
    }

    public void MediaManager(string[] item)//, System.Action CallBack)
    {
        //string path = "Assets/Resources/test2.txt";
       // StreamWriter writer = new StreamWriter(Path, true);
        AudioSource audio = GetComponent<AudioSource>();
        myVideoPlayer = GetComponent<VideoPlayer>();
        // localCallBack = CallBack;
        // audio.Stop();
        string MediaName = item[0];
        NUM = Convert.ToInt32(item[1]);
        // print(item[1]);
        MCQ TYPE = (MCQ)NUM;
        
        
        switch (TYPE)
        {
            case MCQ.Audio:
                if (myVideoPlayer.isPlaying == false && audio.isPlaying == false)
                {
                    myAudioClip = ((AudioClip)Resources.Load("Audio/" + MediaName));
                    audio.clip = myAudioClip;

                    audio.Play();
                    print($"audio length {audio.clip.length}");
                    StartCoroutine(waitAudio(audio.clip.length));
                    // new WaitForSeconds(audio.clip.length);
                   //writer.WriteLine($"Audio file {MediaName} played at {System.DateTime.Now} ");
                    testWrite.WriteToString(dataType, System.DateTime.Now.ToString(), MediaName, "Audio", "Other inormation");
                   //writer.Close();

                }
                break;
            case MCQ.Video:
                if (myVideoPlayer.isPlaying == false && audio.isPlaying == false)
                {
                    audio.Stop();
                  //  print($"thsi is a test for write script{JustWrite.number}");
                    myVideoPlayer.Stop();
                    myVideoPlayer.clip = Resources.Load<VideoClip>("Video/" + MediaName);
                    myVideoPlayer.Play();
                    myVideoPlayer.loopPointReached += EndReached;
                    testWrite.WriteToString(dataType, System.DateTime.Now.ToString(), MediaName, "Video");
                }


                break;
            case MCQ.Image:
                img1 = Resources.Load<Sprite>("Image/" + MediaName);
                //display image to the component "UI image"  that connect to this script
                GetComponent<Image>().sprite = img1;
             //   writer.WriteLine($"Image {MediaName} has been showed ");
                break;


        }



    }
    public void StopAudio(int STOPme)
    {
        // -1 for stop
        STOP = STOPme;
        AudioSource[] allAudios = Camera.main.gameObject.GetComponents<AudioSource>();
        if (STOP == -1)
        {
            foreach (AudioSource audioS in allAudios)
            {
                audioS.Stop();
            }
        }


    }

    //This function will be used to send a message after audio clip is finished
    private IEnumerator waitAudio(float T)
    {
        yield return new WaitForSeconds(T);
        print("end of sound");

        // img1 = Resources.Load<Sprite>("Image/card_waxing_gibbous_image");
        //display image to the component "UI image"  that connect to this script
        //GetComponent<Image>().overrideSprite = img1;
        //localCallBack.Invoke();
        // gameObject.SendMessage("MethodNameToRecieveMessage", "TypeofMessage" );
    }


    //This will send meesage after the Video playe stop
    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        //StreamWriter writer = new StreamWriter(Path, true);
        //vp.playbackSpeed = vp.playbackSpeed / 10.0F;
        //myVideoPlayer.loopPointReached
        myVideoPlayer.Stop();
        // myVideoPlayer.Prepare();
        myVideoPlayer.skipOnDrop = false;
        print($"I reached the end {myVideoPlayer.length}");
        print($"video frame is {myVideoPlayer.frame}");
       // writer.WriteLine($"Video file  End  at {System.DateTime.Now}");

       // writer.Close();
        // img1 = Resources.Load<Sprite>("Image/card_waxing_gibbous_image");
        //display image to the component "UI image"  that connect to this script
        // GetComponent<Image>().overrideSprite= img1;
        // localCallBack.Invoke();
        //gameObject.SendMessage("MethodNameToRecieveMessage", "TypeofMessage" );
    }

    // This method used to puse and unpause the audio 
    public void Pause(int number)
    {
        // -1 for pause and -2 for un pause
        PAUSE = number;
        AudioSource[] allAudios = Camera.main.gameObject.GetComponents<AudioSource>();
        if (PAUSE == -1)
        {
            foreach (AudioSource audioS in allAudios)
            {
                audioS.Pause();
            }
        }
        else
        {

            foreach (AudioSource audioS in allAudios)
            {
                audioS.UnPause();
            }
        }


    }


    //This will stop all audio before starting other audio
    // AudioSource audio = GetComponent<AudioSource>();
    // audio.Stop();
    //AudioSource[] allAudios = Camera.main.gameObject.GetComponents<AudioSource>();
    //foreach (AudioSource audioS in allAudios)
    //{
    //    audioS.Stop();
    //}
    // cal the audio player method
    //myVideoPlayer = GetComponent<VideoPlayer>();
    //    //myVideoPlayer.clip = Resources.Load<VideoClip>("Video/" + VideoName);

    //    //When first video stop if you hit play vidoe it will start the new video
    //    if ((myVideoPlayer.isPlaying == false))
    //    {
    //        //FileAudioPlayer();
    //    }
    //  public AudioPlayer(string playMe)
    // {
    //     Num = (playMe);
    //  }
}
