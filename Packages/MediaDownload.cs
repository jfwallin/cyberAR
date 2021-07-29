using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;

public class MediaDownload : MonoBehaviour
{

    //Holds the information about a single lab
    [Serializable]
    private class LabInfo
    {
        public string lab_description;
        public int lab_id;
    };

    //Holds which labs need downloaded media assets
    [Serializable]
    private class LabsManifest
    {
        public LabInfo[] labs;
    };

    //Hold the information for a single media asset
    [Serializable]
    private class ResourceInfo
    {
        public int resource_id;
        public int resource_lab_id;
        public string resource_type;
        public string resource_url;
    };

    //Holds what media assets to download for a single lab
    [Serializable]
    private class ResourcesManifest
    {
        public ResourceInfo[] labResources;
    };

    //Contains the media assets for an individual lab
    public class LabMedia
    {
        public Dictionary<Tuple<int, int>, Texture2D> labTextures = new Dictionary<Tuple<int, int>, Texture2D>(); //Stores lab texture assets
        public Dictionary<Tuple<int, int>, AudioClip> labAudio = new Dictionary<Tuple<int, int>, AudioClip>();    //Stores lab audio assets
        public Dictionary<Tuple<int, int>, string> labVideos = new Dictionary<Tuple<int, int>, string>();         //Stores lab video url for streaming
        public int lab_id;  //Will be used as the key for the downloadedLabs dictionary in the MediaCatalogue object
        public string lab_description;
    };

    //Class to hold downloaded labs' media assets and provide a means to retrieve those assets
    public class MediaCatalogue
    {
        //Singleton access
        private static MediaCatalogue _instance;
        public static MediaCatalogue Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MediaCatalogue();
                }

                return _instance;
            }
        }

        //Holds all downloaded labs' media assets
        public Dictionary<int, LabMedia> downloadedLabs = new Dictionary<int, LabMedia>();

        //Retrieve a Texture2D asset for a lab
        //Tuple is lab ID number and resource ID number
        //If lab isn't found or if Texture2D asset isn't found, an error message is displayed in the debug console and a null value is returned.
        public Texture2D GetLabTexture(Tuple<int, int> textureKey) //Safe gaurds against possible runtime errors need to be implemented still
        {
            LabMedia retrievedLab = null;
            Texture2D retrievedTexture = null;
            if (downloadedLabs.ContainsKey(textureKey.Item1))
            {
                retrievedLab = downloadedLabs[textureKey.Item1];
                if (retrievedLab.labTextures.ContainsKey(textureKey))
                {
                    retrievedTexture = retrievedLab.labTextures[textureKey];
                }
                else
                    UnityEngine.Debug.Log("Texture asset not found: Check resource ID.");
            }
            else
                UnityEngine.Debug.Log("Lab media not found: Check lab ID.");
            return retrievedTexture;
        }

        //Retrieve an AudioClip asset for a lab
        //Tuple is lab ID number and resource ID number
        //If lab isn't found or if AudioClip asset isn't found, an error message is displayed in the debug console and a null value is returned.
        public AudioClip GetLabAudioClip(Tuple<int, int> audioKey) //Safe gaurds against possible runtime errors need to be implemented still
        {
            LabMedia retrievedLab = null;
            AudioClip retrievedAudioClip = null;
            if (downloadedLabs.ContainsKey(audioKey.Item1))
            {
                retrievedLab = downloadedLabs[audioKey.Item1];
                if (retrievedLab.labAudio.ContainsKey(audioKey))
                {
                    retrievedAudioClip = retrievedLab.labAudio[audioKey];
                }
                else
                    UnityEngine.Debug.Log("AudioClip asset not found: Check resource ID.");
            }
            else
                UnityEngine.Debug.Log("Lab media not found: Check lab ID.");
            return retrievedAudioClip;
        }

        //Retrieve the API endpoint for a lab video
        //Tuple is lab ID number  and resource ID number
        //If lab isn't found or if API endpoint isn't found, an error message is displayed in the debug console and a null value is returned.
        public string GetLabVideoURL(Tuple<int, int> videoKey) //Safe gaurds against possible runtime errors need to be implemented still
        {
            LabMedia retrievedLab = null;
            string retrievedVideoURL = null;
            if (downloadedLabs.ContainsKey(videoKey.Item1))
            {
                retrievedLab = downloadedLabs[videoKey.Item1];
                if (retrievedLab.labVideos.ContainsKey(videoKey))
                {
                    retrievedVideoURL = retrievedLab.labVideos[videoKey];
                }
                else
                    UnityEngine.Debug.Log("Video URL not found: Check resource ID.");
            }
            else
                UnityEngine.Debug.Log("Lab media not found: Check lab ID.");
            return retrievedVideoURL;
        }

        //Retrieves the lab description for a lab
        //labKey is the lab ID number
        //If the lab isn't found, an error message is displayed in the debug console and a null value is returned.
        public string GetLabDescription(int labKey) //Safe gaurds against possible runtime errors need to be implemented still
        {
            LabMedia retrievedLab = null;
            string retrievedDescription = null;
            if (downloadedLabs.ContainsKey(labKey))
            {
                retrievedLab = downloadedLabs[labKey];
                retrievedDescription = retrievedLab.lab_description;
            }
            else
                UnityEngine.Debug.Log("Lab media not found: Check lab ID.");
            return retrievedDescription;
        }
    };

    //Used for passing into IEnumerator functions to download and retrieve Json manifests
    //Also fixes parsing issues due to Unity's constraints on top-level Json objects
    private class JsonHolder
    {
        public string downloaded_json = null;
        public string labs_json_beginning = "{\"labs\":";
        public string resources_json_beginning = "{\"labResources\":";
        public string json = null;
    };

    //API endpoints for manifest files
    public string labsManifestEndpoint = null;
    public string resourcesManifestEndpoint = null;
    public string mediaEndpoint = null;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DownloadAllListedLabs()); //Currently, the entire media download system must take place in a coroutine
    }

    //Downloads all media assets for all labs listed in the labs manifest file
    IEnumerator DownloadAllListedLabs()
    {
        MediaCatalogue catalogue = new MediaCatalogue();
        JsonHolder json_holder = new JsonHolder();

        //Currently supported audio file types
        Dictionary<string, AudioType> supportedAudio = new Dictionary<string, AudioType>()
        {
            { "wav", AudioType.WAV },
            { "ogg", AudioType.OGGVORBIS },
            { "mp3", AudioType.MPEG }
        };

        //Start DownloadManifest() and halt DownloadAllListedLabs() execution until DownloadManifest() is finished
        yield return StartCoroutine(DownloadManifest(labsManifestEndpoint, json_holder));
        json_holder.json = json_holder.labs_json_beginning + json_holder.downloaded_json + "}";
        LabsManifest manifest = JsonUtility.FromJson<LabsManifest>(json_holder.json);

        //Begin downlaod process for each lab
        foreach (LabInfo lab in manifest.labs)
        {
            //Retrieve manifest file for an individual lab's media resources
            json_holder.json = json_holder.resources_json_beginning + json_holder.downloaded_json + "}"; //Makes Json string fully parsable
            ResourcesManifest mediaManifest = JsonUtility.FromJson<ResourcesManifest>(json_holder.json);

            //Create a new LabMedia object to hold current lab's downloaded media
            LabMedia media = new LabMedia();
            media.lab_id = lab.lab_id;
            media.lab_description = lab.lab_description;

            //Download and asset creation for each listed media file
            foreach (ResourceInfo file in mediaManifest.labResources)
            {
                //Determine file name and API endpoint for the resource to be downloaded
                int index = file.resource_url.IndexOf('/');
                string fileName = file.resource_url.Substring(index + 1);
                string endpoint = file.resource_url;
                Tuple<int, int> resourceKey = new Tuple<int, int>(media.lab_id, file.resource_id); //Key to retrieve resource from the dictionary 

                //Determine which function to call based on type of resource to be downloaded
                switch (file.resource_type)
                {
                    case "image":
                        if (media.labTextures.ContainsKey(resourceKey) == false)
                        {
                            yield return StartCoroutine(DownloadTexture(endpoint, media, resourceKey)); //Halt DownloadAllListedLabs() until DownloadTexture() is finished executing
                        }
                        break;
                    case "audio":
                        if (media.labAudio.ContainsKey(resourceKey) == false)
                        {
                            //Determine the type of audio file
                            string extension = fileName.Substring(fileName.Length - 3);
                            AudioType audioFileType = AudioType.UNKNOWN;
                            //Determine if the audio file format is currently supported
                            //If it's not supported the AudioType remains UNKNOWN (may cause issues)
                            if (supportedAudio.ContainsKey(extension) == true)
                                audioFileType = supportedAudio[extension];
                            yield return StartCoroutine(DownloadAudio(endpoint, media, resourceKey, audioFileType)); //Halt DownloadAllListedLabs() until DownloadAudio() is finished executing
                        }
                        break;
                    case "video":
                        if (media.labVideos.ContainsKey(resourceKey) == false)
                            media.labVideos.Add(resourceKey, endpoint); //Originally designed under assumption that video files would be streamed from server, will change in next version.
                        break;
                }
            }
            catalogue.downloadedLabs.Add(media.lab_id, media); //Add the current lab and its downloaded media assets to the catalogue
            UnityEngine.Debug.Log("LabMedia object added to media catalogue");
        }
    }

    //Downloads a Json manifest from the web server
    IEnumerator DownloadManifest(string url, JsonHolder json_holder)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
            UnityEngine.Debug.Log(uwr.error);
        else
        {
            //Retrieve results, decode binary data, and save as string
            json_holder.downloaded_json = System.Text.Encoding.UTF8.GetString(uwr.downloadHandler.data);
        }
    }

    //Downloads an image file, creates a Texutre2D asset, and stores the texture in a LabMedia's Texture2D dictionary
    IEnumerator DownloadTexture(string url, LabMedia media, Tuple<int, int> key)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
                UnityEngine.Debug.Log(uwr.error);
            else
            {
                media.labTextures.Add(key, DownloadHandlerTexture.GetContent(uwr));
            }
        }
    }

    //Downloads an audio file, create an AudioClip asset, and store the AudioClip in a LabMedia's AudioClips dictionary
    IEnumerator DownloadAudio(string url, LabMedia media, Tuple<int, int> key, AudioType fileType)
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
                media.labAudio.Add(key, DownloadHandlerAudioClip.GetContent(uwr));
            }
        }
    }
}