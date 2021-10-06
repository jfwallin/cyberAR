using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            //Check if there is already an instance assigned
            if(_instance == null)
            {
                //Try to find an existing catalogue, assign it if found
                MediaCatalogue search = FindObjectOfType<MediaCatalogue>();
                if(search != null)
                {
                    _instance = search;
                }
                else //Could find no existing catalogue
                {
                    //Try to find the LabManager, add it to that same GameObject
                    GameObject go = GameObject.FindObjectOfType<LabManager>()?.gameObject;
                    if(go != null)
                    {
                        _instance = go.AddComponent<MediaCatalogue>();
                    }
                    else //Could not find a lab manager, try the "_DYNAMIC" object instead
                    {
                        //Try to find the [_DYNAMIC] GameObject, attach
                        GameObject dyn = GameObject.Find("[_DYNAMIC]");
                        if(dyn != null)
                        {
                            _instance = go.AddComponent<MediaCatalogue>();
                        }
                        else //Can't Find _DYNAMIC either
                        {
                            Debug.LogError("Could not find object to attach MediaCatalogue to. returning null");
                        }
                    }
                }
            }

            return _instance;
        }
    }

    public void Awake()
    {
        //Check if there is already another instance, destroy self if that is the case
        if((_instance != null && _instance != this))
        {
            Destroy(this);
        }
        else //Only instance
        {
            _instance = this;
        }
    }

    //Dictionaries for media assets
    //Key for dictionaries are the urls that the raw media file is retrieved from
    public Dictionary<string, Texture2D> labTextures = new Dictionary<string, Texture2D>();
    public Dictionary<string, AudioClip> labAudio = new Dictionary<string, AudioClip>();
    public Dictionary<string, string> labVideos = new Dictionary<string, string>();
    [HideInInspector]
    public bool done = false;

    public void addToCatalogue(LabDataObject data)
    {
        StartCoroutine(DownloadLabMedia(data.Assets));
    }

    //Retrieve a Texture2D asset for a lab
    //textureKey is the url that the raw image file is retrieved from
    //If the Texture2D asset isn't found an error message is displayed in the debug console and a null value is returned.
    public Texture2D GetLabTexture(string textureKey) //Safe gaurds against possible runtime errors need to be implemented still
    {
        Texture2D retrievedTexture = null;
        if (labTextures.ContainsKey(textureKey))
        {
            retrievedTexture = labTextures[textureKey];
            
        }
        else
        {
            string avail = "";
            foreach(string s in labTextures.Keys)
            {
                avail += $", {s}";
            }
            Debug.Log($"Texture asset not found. Used key was: { textureKey}, Available Texture keys:\n{avail},\nSize of dictionary: {labTextures.Count}");
        }
        return retrievedTexture;
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
            UnityEngine.Debug.Log("Video file not found.");
        return retrievedVideoURL;
    }

    //Removes a specified downloaded resource from the Catalogue
    public void DeleteDownloadedMediaAsset(string resourceKey)
    {
        if (labTextures.ContainsKey(resourceKey))
            labTextures.Remove(resourceKey);
        else if (labAudio.ContainsKey(resourceKey))
            labAudio.Remove(resourceKey);
        else if (labVideos.ContainsKey(resourceKey))
        {
            File.Delete(labVideos[resourceKey]); //Delete video file from disk
            labVideos.Remove(resourceKey); //Remove entry from dictionary
        }
        else
            UnityEngine.Debug.Log("Resource not found.");
    }

    //Clears all downloaded assets of a specific media type
    //mediaType should be set to "image", "audio", or "video"
    //passing "video" will delete all downloaded video files from disk
    public void DeleteDownloadedMediaType(string mediaType)
    {
        switch (mediaType)
        {
            case "image":
                labTextures.Clear();
                break;
            case "audio":
                labAudio.Clear();
                break;
            case "video":
                Dictionary<string, string>.ValueCollection videoFilePaths = labVideos.Values; //Get a collection of filepaths for all downloaded video files
                foreach (string videoFilePath in videoFilePaths)
                {
                    File.Delete(videoFilePath); //Delete each video file from disk
                }
                labVideos.Clear(); //Clear the labVideos dictionary
                break;
        }
    }

    //Clears all downloaded media asset dictionaries and deletes all downloaded video files that are saved to disk
    public void DeleteAllDownloadedMediaAssets()
    {
        labTextures.Clear();
        labAudio.Clear();
        Dictionary<string, string>.ValueCollection videoFilePaths = labVideos.Values; //Get a collection of filepaths for all downloaded video files
        foreach (string videoFilePath in videoFilePaths)
        {
            File.Delete(videoFilePath); //Delete each video file from disk
        }
        labVideos.Clear(); //Clear teh labVideos dictionary
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
                print($"Added texture to catalogue, key: {url}");
                labTextures.Add(url, DownloadHandlerTexture.GetContent(uwr));
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
                labAudio.Add(url, DownloadHandlerAudioClip.GetContent(uwr));
        }
    }

    IEnumerator DownloadVideo(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                UnityEngine.Debug.Log(uwr.error);
                UnityEngine.Debug.Log(url);
            }
            else
            {
                int index = url.LastIndexOf("/");
                index += 1;
                string filename = url.Substring(index);
                string videoPath = Application.dataPath + "/" + filename;    //Path to downloaded video file
                File.WriteAllBytes(videoPath, uwr.downloadHandler.data); //Save video file to disk
                labVideos.Add(url, videoPath); //Add path to video to the videos dictionary
            }
        }
    }

    //Downloads all the media assets for a single lab
    IEnumerator DownloadLabMedia(MediaInfo[] labAssetList)
    {
        //Currently supported audio file types
        Dictionary<string, AudioType> supportedAudio = new Dictionary<string, AudioType>()
        {
            { "wav", AudioType.WAV },
            { "ogg", AudioType.OGGVORBIS },
            { "mp3", AudioType.MPEG }
        };

        foreach (MediaInfo asset in labAssetList)
        {
            print("downloading new asset of type: " + asset.resource_type);
            string resourceKey = asset.resource_url;
            switch (asset.resource_type) //Determine type of asset to be downloaded
            {
                case MediaType.Image:
                    if(labTextures.ContainsKey(resourceKey) == false)
                        yield return StartCoroutine(DownloadTexture(resourceKey));
                    break;
                case MediaType.Audio:
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
                case MediaType.Video:
                    if (labVideos.ContainsKey(resourceKey) == false)
                        yield return StartCoroutine(DownloadVideo(resourceKey));
                    break;
            }
        }
        done = true;
    }


}