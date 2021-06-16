using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Video;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles media playback of files retrieved from the media manager,
/// responds to function calls passing the name of the file to be 
/// played and its filetype. Also implements various media playback
/// controls
/// </summary>
public class MediaPlayer : MonoBehaviour
{
    #region Variables
    //Instance field
    private static MediaPlayer _instance;
    public static MediaPlayer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<MediaPlayer>();
            }

            return _instance;
        }
    }

    //Helper class
    public MediaManager media = null; //THIS WILL BE REPLACED WITH A NEW CLASS
    //public MediaCatalogue media = null;

    //Componenet References
    [SerializeField]
    private VideoPlayer videoPlayer = null;
    [SerializeField]
    private AudioSource audioSource = null;
    [SerializeField]
    private Image imageDisplay = null;
    [SerializeField]
    private Slider slider = null;

    public System.Action localCallBack;
    #endregion Variables

    #region Unity Functions
    private void Awake()
    {
        //Singleton Management, delete self if another media player exists.
        if(_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else
        {
            _instance = this;
        }

        //Use an audio source on the main camera to ensure the best chance  of users being able to hear
        audioSource = Camera.main.GetComponent<AudioSource>();
        if (!audioSource)
        {
            Debug.LogWarning("Could not find AudioSource for the mediaPlayer.");
        }

        //VideoPlayer setup
        videoPlayer.source = VideoSource.Url;

        //TO BE UPDATED WHEN LINKED TO NEW MEDIA MANAGER
        if (media == null)
        {
            //media = MediaCatalogue.Instance;
        }
    }

    void Update()
    {
        //Update the slider position if media is playing.
        if(videoPlayer.enabled && videoPlayer.isPlaying)
        {
            slider.SetValueWithoutNotify((float)videoPlayer.time / (float)videoPlayer.clip.length);
        }
        else if (audioSource.enabled && audioSource.isPlaying)
        {
            slider.SetValueWithoutNotify(audioSource.time / audioSource.clip.length);
        }

        //Respond to the end of an audio clip
        if(audioSource.enabled && !audioSource.isPlaying && audioSource.time ==0)
        {
            localCallBack.Invoke();
        }
    }
    #endregion Unity Functions

    #region Public Functions
    /// <summary>
    /// Handles displaying any type of media
    /// </summary>
    /// <param name="item">size 2 array, first item is the file name, second item is an integer corresponding to an ENUM filetype</param>
    /// <param name="CallBack">Function to call once the media finishes playing</param>
    public void PlayMedia (MediaInfo item, System.Action CallBack)
    {
        //Store callback reference for later
        localCallBack = CallBack;
        //Display the media, different depending on filetype
        switch (item.mediaType)
        {
            case MediaType.Audio:
                //Don't play new audio if something else is already playing
                if (videoPlayer.isPlaying == false && audioSource.isPlaying == false)
                {
                    //Turn off video player
                    videoPlayer.GetComponent<MeshRenderer>().enabled = false;
                    videoPlayer.enabled = false;
                    //Leave the Image display enabled, in case we need audio with an image
                    //Turn on the audio player
                    audioSource.enabled = true;

                    //Get and set audio clip
                    //audioSource.clip = media.GetAudioClip(MediaName); DEPRECATED

                    //audioSource.clip = media.GetLabAudioClip(item.id); WILL BE USED WITH NEW CLASS
                    audioSource.Play();

                    //Callback invocation condition is checked in the update loop
                }
                break;

            case MediaType.Video:
                //Don't play new video if something else is already playing
                if (videoPlayer.isPlaying == false && audioSource.isPlaying == false)
                {
                    //Disable audio and image dispaly
                    imageDisplay.gameObject.SetActive(false);
                    imageDisplay.enabled = false;
                    audioSource.enabled = false;
                    //Turn on the video player
                    videoPlayer.GetComponent<MeshRenderer>().enabled = true;
                    videoPlayer.enabled = true;

                    //Get and set video clip
                    //videoPlayer.clip = media.GetVideoClip(MediaName); DEPRECATED

                    //videoPlayer.url = media.GetLabVideoURL(item.id); WILL BE USED WITH NEW CLASS
                    videoPlayer.Play();

                    //Subscribe handler function to respond to when the end of the video is reached.
                    videoPlayer.loopPointReached += VideoEndReached;
                }
                break;

            case MediaType.Image:
                //Disable the video and audio players
                videoPlayer.GetComponent<MeshRenderer>().enabled = false;
                videoPlayer.enabled = false;
                audioSource.enabled = false;
                //Enable image display
                imageDisplay.gameObject.SetActive(true);
                imageDisplay.enabled = true;

                //Get the texture, construct the sprite.
                //Texture2D tex = media.GetImage(MediaName); DEPRECATED

                //Texture2D tex = media.GetLabTexture(item.id); WILL BE USED WITH NEW CLASS
                //imageDisplay.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);

                //Immediately invoke callback
                localCallBack.Invoke();
                break;
        }
    }
    #endregion Public Functions

    #region Event Handlers
    /// <summary>
    /// Pauses audio or video sources, Pause button handler
    /// </summary>
    public void Pause()
    {
        if (audioSource.enabled)
        {
            audioSource.Pause();
        }
        if (videoPlayer.enabled)
        {
            videoPlayer.Pause();
        }
    }

    /// <summary>
    /// Plays audio or video sources, Play button handler
    /// </summary>
    public void Play()
    {
        if (audioSource.enabled)
        {
            audioSource.Play();
        }
        if (videoPlayer.enabled)
        {
            videoPlayer.Play();
        }
    }

    /// <summary>
    /// Skip forward button handler
    /// </summary>
    public void SkipForward()
    {
        Skip(5);
    }

    /// <summary>
    /// Skip back button handler
    /// </summary>
    public void SkipBackward()
    {
        Skip(-5);
    }

    /// <summary>
    /// Restart button handler, sets playback time to 0
    /// </summary>
    public void Restart()
    {
        if (audioSource.enabled)
        {
            audioSource.time = 0;
            audioSource.Play();
        }
        if (videoPlayer.enabled)
        {
            videoPlayer.time = 0;
            videoPlayer.Play();
        }
    }

    /// <summary>
    /// Scrubs through media in response to slider value
    /// </summary>
    /// <param name="value">0-1, percent complete</param>
    public void handleOnSliderValueChanged(float value)
    {
        if (videoPlayer.enabled)
            videoPlayer.frame = (long)Math.Floor(videoPlayer.clip.frameCount * value);
        
        else if(audioSource.enabled)
        {
            audioSource.time = audioSource.clip.length * value;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Responds to when the end of a video is reached
    /// </summary>
    /// <param name="vp">The video player that reached the end of a clip</param>
    public void VideoEndReached(UnityEngine.Video.VideoPlayer vp)
    {
        vp.loopPointReached -= VideoEndReached;
        videoPlayer.Stop();
        videoPlayer.skipOnDrop = false;
        localCallBack.Invoke();
    }
    #endregion Event Handlers

    #region Private Methods
    /// <summary>
    /// Handles skipping through media, is the backend for
    /// both skip button handlers
    /// </summary>
    /// <param name="delta">time in seconds to skip. (+/-) is forward/backward</param>
    private void Skip(int delta)
    {
        if (audioSource.enabled)
        {
            audioSource.time = Mathf.Clamp(audioSource.time + delta, 0, audioSource.clip.length);
            audioSource.Play();
        }
        if (videoPlayer.enabled)
        {
            videoPlayer.time = Mathf.Clamp((float)(videoPlayer.time + delta), 0, (float)videoPlayer.length);
            videoPlayer.Play();
        }
    }
    #endregion Private Methods
}
