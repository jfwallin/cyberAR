﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Video;
using UnityEngine;
using UnityEngine.UI;

public enum MediaType { Audio, Video, Image }

/// <summary>
/// Handles media playback of files retrieved from the media manager,
/// responds to function calls passing the name of the file to be 
/// played and its filetype. Also implements various media playback
/// controls
/// </summary>
public class MediaPlayer : MonoBehaviour
{
    #region Variables
    //Helper class
    public MediaManager media = null;

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
        //Use an audio source on the main camera to ensure the best chance  of users being able to hear
        audioSource = Camera.main.GetComponent<AudioSource>();
        if (!audioSource)
        {
            Debug.LogWarning("Could not find AudioSource for the mediaPlayer.");
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
    public void PlayMedia (string[] item, System.Action CallBack)
    {
        //Store callback reference for later
        localCallBack = CallBack;

        //Get needed info from string array
        string MediaName = item[0]; //Filename
        MediaType TYPE =(MediaType) Convert.ToInt32(item[1]); //Filetype

        //Display the media, different depending on filetype
        switch (TYPE)
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
                    audioSource.clip = media.GetAudioClip(MediaName);
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
                    videoPlayer.clip = media.GetVideoClip(MediaName);
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
                Texture2D tex = media.GetImage(MediaName);
                imageDisplay.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);

                //Immediately invoke callback
                localCallBack.Invoke();
                break;
        }
    }

    /// <summary>
    /// Pauses audio or video sources
    /// </summary>
    public void Pause()
    {
        if(audioSource.enabled)
        {
            audioSource.Pause();
        }
        else if(videoPlayer.enabled)
        {
            videoPlayer.Pause();
        }
    }

    /// <summary>
    /// Plays audio or video sources
    /// </summary>
    public void Play()
    {
        if (audioSource.enabled)
        {
            audioSource.Play();
        }
        else if (videoPlayer.enabled)
        {
            videoPlayer.Play();
        }
    }
    #endregion Public Functions

    #region Event Handlers
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
}
