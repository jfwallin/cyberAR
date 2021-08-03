using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MediaCatalogue : MonoBehaviour
{
    //Singleton access
    private static MediaCatalogue _instance;
    public static MediaCatalogue Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new MediaCatalogue();
            }

            return Instance;
        }
    }

    //Dictionaries for media assets
    //Key for dictionaries are the urls that the raw media file is retrieved from
    public Dictionary<string, Texture2D> labTextures = new Dictionary<string, Texture2D>();
    public Dictionary<string, AudioClip> labAudio = new Dictionary<string, AudioClip>();
    public Dictionary<string, string> labVideos = new Dictionary<string, string>();

    //Currently supported audio file types
    Dictionary<string, AudioType> supportedAudio = new Dictionary<string, AudioType>()
    {
        { "wav", AudioType.WAV },
        { "ogg", AudioType.OGGVORBIS },
        { "mp3", AudioType.MPEG }
    };

    public void AddToCatalogue(LabDataObject data)
    {
        StartCoroutine(DownloadLabMedia(data.Assets));
    }

    //Removes a media asset from the MediaCatalogue, assetKey is the url the asset was retrieved from
    public void RemoveMediaAsset(string assetKey)
    {
        //Determine type of asset to be removed
        string mediaAssetType = assetKey.Substring(assetKey.Length - 3);
        if (supportedAudio.ContainsKey(mediaAssetType))
            mediaAssetType = "audio";
        switch (mediaAssetType)
        {
            case: "jpg"
                if (labTextures.ContainsKey(assetKey))
                {
                    labTextures.Remove(assetKey);
                    UnityEngine.Debug.Console("Texture2D removed from MediaCatalogue.");
                }
            case: "audio"
                if (labAudio.ContainsKey(assetKey))
                {
                    labAudio.Remove(assetKey);
                    UnityEngine.Debug.Console("AudioClip removed from MediaCatalogue.");
                }
                break;
            case: "mp4"
                if (labVideos.ContainsKey(assetKey))
                {
                    labVideos.Remove(assetKey);
                    UnityEngine.Debug.Console("Video URL removed from MediaCatalogue.");
                }
                break;
            default:
                UnityEngine.Debug.Console("ERROR: Asset not found in any dictionary.");
                UnityEngine.Debug.Console("Asset URL: " + assetKey);
                break;
        }
    }

    //Removes all media assets in the MediaCatalogue
    public void RemoveAllMediaAssets()
    {
        labTextures.Clear();
        labAudio.Clear();
        labVideos.Clear();
    }

    //Retrieve a Texture2D asset for a lab
    //textureKey is the url that the raw image file is retrieved from
    //If the Texture2D asset isn't found an error message is displayed in the debug console and a null value is returned.
    public Texture2D GetLabTexture(string textureKey) //Safe gaurds against possible runtime errors need to be implemented still
    {
        if (labTextures.ContainsKey(textureKey))
        {
            Texture2D retrievedTexture = labTextures[textureKey];
            return retrievedTexture;
        }
        else
        {
            UnityEngine.Debug.Log("Texture asset not found.");
        }
    }

    //Retrieve an AudioClip asset for a lab
    //audioKey is the url that the raw image file is retrieved from
    //If AudioClip asset isn't found, an error message is displayed in the debug console and a null value is returned
    public AudioClip GetLabAudioClip(string audioKey) //Safe gaurds against possible runtime errors need to be implemented still
    {
        AudioClip retrievedAudioClip = null;
        if (labAudio.ContainsKey(audioKey))
            retrievedAudioClip = labAudio[audioKey];
        else
            UnityEngine.Debug.Log("AudioClip asset not found.");
        return retrievedAudioClip;
    }

    //Retrieve the API endpoint for a lab video
    //videoKey is the url that the raw image file is retrieved from
    //If API endpoint isn't found, an error message is displayed in the debug console and a null value is returned.
    public string GetLabVideoURL(string videoKey) //Safe gaurds against possible runtime errors need to be implemented still
    {
        string retrievedVideoURL = null;
        if (labVideos.ContainsKey(videoKey))
            retrievedVideoURL = labVideos[videoKey];
        else
            UnityEngine.Debug.Log("Video URL not found.");
        return retrievedVideoURL;
    }

    //Downloads an image file, creates a Texutre2D asset, and stores the asset in the labTextures dictionary
    IEnumerator DownloadTexture(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                UnityEngine.Debug.Log(uwr.error);
                UnityEngine.Debug.Log(url);
            }
            else
            {
                labTextures.Add(url, DownloadHandlerTexture.GetContent(uwr));
                UnityEngine.Debug.Log("Texture downloaded from " + url);
            }
        }
    }

    //Downloads an audio file, create an AudioClip asset, and stores the asset in the labAudio dictionary
    IEnumerator DownloadAudio(string url, AudioType fileType)
    {
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, fileType))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                UnityEngine.Debug.Log(uwr.error);
                UnityEngine.Debug.Log(url);
            }
            else
            {
                labAudio.Add(url, DownloadHandlerAudioClip.GetContent(uwr));
                UnityEngine.Debug.Log("AudioClip downloaded from " + url);
            }
        }
    }

    //Downloads all the media assets for a single lab
    IEnumerator DownloadLabMedia(LabDataObject.submanifest[] labAssetList)
    {

        foreach (LabDataObject.subManifest asset in labAssetList)
        {
            string resourceKey = asset.resource_url;
            switch (asset.resource_type) //Determine type of asset to be downloaded
            {
                case "image":
                    if(labTextures.ContainsKey(resourceKey) == false)
                        yield return StartCoroutine(DownloadTexture(resourceKey));
                    break;
                case "audio":
                    if(labAudio.ContainsKey(resourceKey) == false)
                    {
                        //Determine file name and audio type
                        int index = asset.resource_url.IndexOf('/');
                        string fileName = asset.resource_url.Substring(index + 1);
                        string extension = fileName.Substring(fileName.Length - 3);
                        AudioType audioFileType = AudioType.UNKNOWN;

                        //Determine if the audio file format is currently supported
                        //If it's not supported the AudioType remains UNKNOWN (may cause issues)
                        if (supportedAudio.ContainsKey(extension) == true)
                            audioFileType = supportedAudio[extension];
                        yield return StartCoroutine(DownloadAudio(resourceKey, audioFileType));
                    }
                    break;
                case "video":
                    if (labVideos.ContainsKey(resourceKey) == false)
                        labVideos.Add(resourceKey, asset.resource_url);
                    break;
            }
        }
    }
}