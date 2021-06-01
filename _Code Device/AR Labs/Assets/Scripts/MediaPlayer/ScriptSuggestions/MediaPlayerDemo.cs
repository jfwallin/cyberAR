using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video; //Must include this

public class MediaPlayerDemo : MonoBehaviour
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
        Debug.Log($"Play Media called with media name: {mediaName}\n" +
                  $"MediaType is {mediaType}\n");

        //First, play the media. If its a video or audio, wait until it is complete and then call the callback
        switch(mediaType)
        {
            case MCQ.MediaType.Audio:
                AudioClip audio = mediaManager.GetAudioClip(mediaName);
                Debug.Log($"MediaManager returned {audio?.name}");
                break;
            case MCQ.MediaType.AudioAndImage:
                AudioClip audioI = mediaManager.GetAudioClip(mediaName.Split(',')[0].Trim());
                Texture2D imageA = mediaManager.GetImage(mediaName.Split(',')[1].Trim());
                Debug.Log($"MediaManager returned {audioI?.name} and {imageA?.name}");
                break;
            case MCQ.MediaType.Image:
                Texture2D image = mediaManager.GetImage(mediaName);
                Debug.Log($"MediaManager returned {image?.name}");
                break;
            case MCQ.MediaType.Video:
                //get clip from resources
                //videoclip video = (videoclip)resources.load("video/" + medianame);

                //or get the clip from the mediaManager
                VideoClip video = mediaManager.GetVideoClip(mediaName);
                Debug.Log($"MediaManager returned {video?.name}");

                //Assign the new video clip
                vPlayer.clip = video;

                //Hook into the video player so we are notified when the video is done
                vPlayer.loopPointReached += handleVideoCompleted;

                //Play the video
                vPlayer.Play();
                break;
            case MCQ.MediaType.None:
                Debug.Log("MediaType none encountered, no media retrieved");
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
