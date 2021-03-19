using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Video;
using UnityEngine;
using UnityEngine.UI;
//Audio Player 
// Recieve inputs as a string fro audio wave that reside in 
// Audio file in Assets/Resources file 
public enum MediaType { Audio, Video, Image }
public class AudioPlayer : MonoBehaviour
{
    //Componenet References
    [SerializeField]
    private VideoPlayer vPlayer;
    [SerializeField]
    private AudioSource aSource;
    [SerializeField]
    private Image imageDisplay;
    public MediaManager media = null;

   
    public string Num { get; set; }
    public int PAUSE { get; set; }
    public int STOP { get; set; }
    public int NUM { set; get; }
    public System.Action localCallBack;
    // private float Beg { set; get; }
    //  public AudioSource[] allAudios;
    private Sprite img1;
    public VideoPlayer myVideoPlayer { set; get; }
    
    private AudioClip myAudioClip;
    public AudioPlayer(string playMe)
    {
        Num = (playMe);
    }
    // This will get a item should change to get the file name
    public void MediaManager (string[] item, System.Action CallBack)
    {
        //AudioSource audio = GetComponent<AudioSource>();
        //myVideoPlayer = GetComponent<VideoPlayer>();
        //Use the assigned values.
        AudioSource audio = aSource;
        myVideoPlayer = vPlayer;

        localCallBack = CallBack;
       // audio.Stop();
        string MediaName = item[0];
        NUM = Convert.ToInt32(item[1]);
       // print(item[1]);
        MediaType TYPE =(MediaType) NUM ;//  int.Parse(item[1]);
     //   print(NUM);
        switch (TYPE)
        {
            case MediaType.Audio:
                if (myVideoPlayer.isPlaying == false && audio.isPlaying == false)
                {
                    vPlayer.gameObject.SetActive(false);
                    vPlayer.enabled = false;
                    aSource.gameObject.SetActive(true);
                    aSource.enabled = true;

                    //myAudioClip = ((AudioClip)Resources.Load("Audio/" + MediaName));
                    myAudioClip = media.GetAudioClip(MediaName);
                    audio.clip = myAudioClip;
                   
                    audio.Play();
                    print($"audio length {audio.clip.length}");
                    StartCoroutine(waitAudio(audio.clip.length));
                    // new WaitForSeconds(audio.clip.length);


                }
                break;
            case MediaType.Video:
                vPlayer.gameObject.SetActive(true);
                vPlayer.enabled = true;
                aSource.gameObject.SetActive(false);
                aSource.enabled = false;

                audio.Stop();
                myVideoPlayer.Stop();
                //myVideoPlayer.clip = Resources.Load<VideoClip>("Video/" + MediaName);
                myVideoPlayer.clip = media.GetVideoClip(MediaName);
               // Beg = Time.time;
                myVideoPlayer.Play();
                myVideoPlayer.loopPointReached += EndReached;
              

                break;
            case MediaType.Image:
                img1 = Resources.Load<Sprite>("Image/" + MediaName);
                //display image to the component "UI image"  that connect to this script
                //GetComponent<Image>().sprite = img1;

                //Get the texture, construct the sprite.
                Texture2D tex = media.GetImage(MediaName);
                img1 = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
                imageDisplay.sprite = img1;
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
        localCallBack.Invoke();
        // gameObject.SendMessage("MethodNameToRecieveMessage", "TypeofMessage" );
    }

    
    //This will send meesage after the Video playe stop
    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        vp.loopPointReached -= EndReached;
        //vp.playbackSpeed = vp.playbackSpeed / 10.0F;
        //myVideoPlayer.loopPointReached
        myVideoPlayer.Stop();
       // myVideoPlayer.Prepare();
        myVideoPlayer.skipOnDrop = false;
        print($"I reached the end {myVideoPlayer.length}");
        print($"video frame is {myVideoPlayer.frame}");
        localCallBack.Invoke();
        //gameObject.SendMessage("MethodNameToRecieveMessage", "TypeofMessage" );
    }

// This method used to puse and unpause the audio 
public void Pause(int number)
    {
        // -1 for pause and -2 for un pause
        //PAUSE = number;
        //AudioSource[] allAudios = Camera.main.gameObject.GetComponents<AudioSource>();
        //if (PAUSE == -1)
        //{
        //    foreach (AudioSource audioS in allAudios)
        //    {
        //        audioS.Pause();
        //    }
        //}
        //else
        //{

        //    foreach (AudioSource audioS in allAudios)
        //    {
        //        audioS.UnPause();
        //    }
        //}

        if(aSource.enabled)
        {
            aSource.Pause();
        }
        else if(vPlayer.enabled)
        {
            vPlayer.Pause();
        }

        //if(aSource.isPlaying)
        //{
        //    aSource.Pause();
        //}
        //else //Is paused or stopped
        //{
        //    aSource.Play();
        //}
    }

    public void Play()
    {
        if (aSource.enabled)
        {
            aSource.Play();
        }
        else if (vPlayer.enabled)
        {
            vPlayer.Play();
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

}
