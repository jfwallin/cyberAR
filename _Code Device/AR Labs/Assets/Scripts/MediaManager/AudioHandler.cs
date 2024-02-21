using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    #region Variables
    // Singleton Instance
    private static AudioHandler _instance;
    public static AudioHandler Instance
    {
        get
        {
            //Check if there is already an instance assigned
            if (_instance == null)
            {
                // If there isn't already an instance, then make a new one on the Logic object
                GameObject go = GameObject.Find("[LOGIC]");
                _instance = go.AddComponent<AudioHandler>();
            }

            return _instance;
        }
    }

    // Reference to the catalogue to retreive audios from
    private MediaCatalogue mediacatalogue;
    // Reference to the audio source to play from
    private AudioSource speaker;
    // float timer for how long to wait for the catalogue to initialize
    private float mediacatalogue_waittime = 60.0f;
    // float timer for how long between re-checking the catalogue
    private float mediacatalogue_rechecktime = 5.0f;
    // Transmission flag, determines how we play audio. Should be set by external script when starting the lab
    public bool transmissionHost = false;
    #endregion Variables

    private void Awake()
    {
        // Check if there is already another instance, destroy self if that is the case
        if ((_instance != null && _instance != this))
        {
            Destroy(this);
        }
        else //Only instance
        {
            _instance = this;
        }
    }

    private void Start()
    {
        // Get references
        mediacatalogue = MediaCatalogue.Instance;
        // Use an audiosource on the camera / headset position (must be enabled)
        speaker = Camera.main.GetComponent<AudioSource>();
    }

    /// <summary>
    /// Main entry point for all scripts. Manages playing audio for transmission uses or single user
    /// </summary>
    /// <param name="clipName">Name of the audio clip file to play, pulled from the MediaCatalogue</param>
    public void PlayAudio(string clipName)
    {
        if (transmissionHost)
        {
            // Send RPC call. This will play the audio on the peer
            // this requires that the object that the AudioHandler is attached to is in the list of rpcTargets on transmission
            MagicLeapTools.Transmission.Send(new MagicLeapTools.RPCMessage("PlayAudioLocal", clipName));
        }

        // Then play the audio locally
        PlayAudioLocal(clipName);
    }

    /// <summary>
    /// Used to play audio locally. Called by Transmission rpc to play on peers, or by the class to play on a host or single user.
    /// </summary>
    /// <param name="clipName">Name of the audio clip file to play, pulled from the MediaCatalogue</param>
    public void PlayAudioLocal(string clipName)
    {
        AudioClip audioclip;

        if (mediacatalogue.DoneLoadingAssets)
        {
            audioclip = mediacatalogue.GetAudioClip(clipName);
        }
        else
        {
            // media catalogue not ready, wait a bit
            IEnumerator playAudioDelayed = WaitForCatalogue(clipName);
            StartCoroutine(playAudioDelayed);
        }
    }

    /// <summary>
    /// Coroutine used to wait on the media catalogue to initialize
    /// </summary>
    /// <param name="clipName">Name of the audio clip file to play, pulled from the MediaCatalogue</param>
    /// <returns></returns>
    IEnumerator WaitForCatalogue(string clipName)
    {
        float total_wait_time = 0.0f;

        while (total_wait_time < mediacatalogue_waittime)
        {
            yield return new WaitForSecondsRealtime(mediacatalogue_rechecktime);

            if (mediacatalogue.DoneLoadingAssets)
            {
                PlayAudio(clipName);
                break;
            }
        }
    }
}
