using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video; //Must include this

public class MediaPlayer : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer vPlayer = null; //Reference to the video player

    System.Action mediaCompleteCallback; //A function to be called when the media is done playing

    MediaManager mediaManager = null;

    public void Initialize(MediaManager initManager)
    {
        mediaManager = initManager;
    }

    public void PlayMedia(string mediaName, MCQ.MediaType mediaType, System.Action callback)
    {
        mediaCompleteCallback = callback; //Hold on to the function to call once the media is done
        Debug.Log($"Play Media called with media name: {mediaName}\nExecuting callback\n\n\n");

        //First, play the media. If its a video or audio, wait until it is complete and then call the callback
        switch(mediaType)
        {
            case MCQ.MediaType.Audio:
                break;
            case MCQ.MediaType.AudioAndImage:
                break;
            case MCQ.MediaType.Image:
                break;
            case MCQ.MediaType.Video:
                //Get clip from resources
                VideoClip video = (VideoClip)Resources.Load("Video/" + mediaName);
                //or get the clip from the mediaManager
                VideoClip video1 = mediaManager.GetVideoClip(mediaName);


                //Assign the new video clip
                vPlayer.clip = video;

                //Hook into the video player so we are notified when the video is done
                vPlayer.loopPointReached += handleVideoCompleted;

                //Play the video
                vPlayer.Play();
                break;
            case MCQ.MediaType.None:
                break;
        }
    }

    //Handles when the video player reaches the end of the video
    public void handleVideoCompleted(VideoPlayer source)
    {
        //Unsubscribe from the event (very imoprtant to balance subscriptions and unsubscriptions)
        vPlayer.loopPointReached -= handleVideoCompleted;

        //Call the function to tell the MCQ that the media playback is complete.
        mediaCompleteCallback.Invoke();
    }
}
