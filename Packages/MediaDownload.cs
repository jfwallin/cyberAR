using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Video;

public class MediaDownload : MonoBehaviour
{
    //Holds the information about a single lab
    [Serializable]
    private class LabInfo
    {
        string lab_description;
        int lab_id; 
    };

    //Holds which labs need downloaded media assets
    [Serializable]
    private class LabsManifest
    {
        LabInfo[] labs;
    };

    //Hold the information for a single media asset
    [Serializable]
    private class ResourceInfo
    {
        int resource_id;
        int resource_lab_id;
        string resource_type;
        string resource_url;
    };
    
    //Holds what media assets to download for a single lab
    [Serializable]
    private class ResourcesManifest
    {
        ResourceInfo[] labResources;
    };

    //Contains the media assets for an individual lab
    private class LabMedia
    {
        Dictionary<Tuple<int, int>, Texture2D> labTextures = new Dictionary<Tuple<int, int>, Texture2D>(); //Stores lab texture assets
        Dictionary<Tuple<int, int>, AudioClip> labAudio = new Dictionary<Tuple<int, int>, AudioClip>();    //Stores lab audio assets
        Dictionary<Tuple<int, int>, string> labVideos = new Dictionary<Tuple<int, int>, string>();         //Stores lab video url for streaming
        int lab_id;  //Will be used as the key for the downloadedLabs dictionary in the MediaCatalogue object
        string lab_description;
    };

    //Class to hold a dictionary of LabMedia objects
    private class MediaCatalogue
    {
        Dictionary<int, LabMedia> downloadedLabs = new Dictionary<int, LabMedia>();

        //Retrieve a Texture2D asset for a lab
        //Tuple is lab ID number and resource ID number
        //If lab isn't found or if Texture2D asset isn't found, an error message is displayed in the debug console and a null value is returned.
        public Texture2D GetLabTexture(Tuple<int,int> textureKey)
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
                    Debug.Log("Texture asset not found: Check resource ID.");
            }
            else
                Debug.Log("Lab media not found: Check lab ID.");
            return retrievedTexture;
        }

        //Retrieve an AudioClip asset for a lab
        //Tuple is lab ID number and resource ID number
        //If lab isn't found or if AudioClip asset isn't found, an error message is displayed in the debug console and a null value is returned.
        public AudioClip GetLabAudioClip(Tuple<int,int> audioKey)
        {
            LabMedia retrievedLab = null;
            AudioClip retrievedAudioClip = null;
            if downloadedLabs.ContainsKey(Tuple<int, int> audioKey)
            {
                retrievedLab = downloadedLabs[audioKey.Item1];
                if (retrievedLab.labAudio.ContainsKey(audioKey))
                {
                    retrievedAudioClip = retrievedLab.labAudio[audioKey];
                }
                else
                    Debug.Log("AudioClip asset not found: Check resource ID.");
            }
            else
                Debug.Log("Lab media not found: Check lab ID.");
            return retrievedAudioClip;
        }

        //Retrieve the API endpoint for a lab video
        //Tuple is lab ID number  and resource ID number
        //If lab isn't found or if API endpoint isn't found, an error message is displayed in the debug console and a null value is returned.
        public string GetLabVideoURL(Tuple<int,int> videoKey)
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
                    Debug.Log("Video URL not found: Check resource ID.");
            }
            else
                Debug.Log("Lab media not found: Check lab ID.");
            return retrievedVideoURL;
        }

        //Retrieves the lab description for a lab
        //labKey is the lab ID number
        //If the lab isn't found, an error message is displayed in the debug console and a null value is returned.
        public string GetLabDescription(int labKey)
        {
            LabMedia retrievedLab = null;
            string retrievedDescription = null;
            if (downloadedLabs.ContainsKey(labKey))
            {
                retrievedLab = downloadedLabs[labKey];
                retrievedDescription = retrievedLab.lab_description;
            }
            else
                Debug.Log("Lab media not found: Check lab ID.");
            return retrievedDescription;
        }
    };

    //API endpoints for manifest files
    public string labsManifestEndpoint = null;
    public string resourcesManifestEndpoint = null;
    public string mediaEndpoint = null;

    void Start()
    {
        //Dictionary to hold all LabMedia objects (key: lab_id, value: LabMedia object)
        MediaCatalogue catalogue; //Holds all of the LabMedia objects

        //Download labs manifest file and serialize into LabsManifest object
        string json = null; 
        StartCoroutine(DownloadManifest(labsManifestEndpoint, json));
        LabsManifest manifest = JsonUtility.FromJson<LabsManifest>(json);

        //Supported audio file types
        Dictionary<string, AudioType> supportedAudio = new Dictionary<string, AudioType>()
        {
            { "wav", AudioType.WAV },
            { "ogg", AudioType.OGGVORBIS },
            { "mp3", AudioType.MPEG }
        };

        //Begin download process for each lab
        foreach(LabInfo lab in manifest.labs)
        {
            //Retrieve manifest file for an individual lab's media resources
            StartCoroutine(DownloadManifest(resourcesManifestEndpoint + lab.lab_id, json));
            ResourcesManifest mediaManifest = JsonUtility.FromJson<ResourcesManifest>(json); 

            //Create a new LabMedia object for the current lab's downloaded media
            LabMedia media = new LabMedia();
            media.lab_id = lab.lab_id;
            media.lab_description = lab.lab_description;

            //Download and asset creation for each requested media file
            foreach(ResourceInfo file in mediaManifest.labResources)
            {
                //Determine file name and API endpoint for the resource to be downloaded
                int index = file.resource_url.IndexOf('/');
                string fileName = file.resource_url.Substring(index + 1);
                string endpoint = mediaEndpoint + file.resource_url;
                Tuple<int, int> resourceKey = new Tuple<int, int>(media.lab_id, file.resource_id); //Key to retrieve resource from the dictionary

                //Determine which function to call based on type of resource to be downloaded
                switch (file.resource_type)
                {
                    case "image":
                        if (media.labTextures.ContainsKey(resourceKey) == false) 
                            StartCoroutine(DownloadTexture(endpoint, media.labTextures, resourceKey));
                        break;
                    case "audio":
                        //Add code to determine audio type later on
                        if (media.labAudio.ContainsKey(resourceKey) == false)
                        {
                            //Add dictionary for audiotype lookup
                            string extension = fileName.Substring(fileName.Length - 3);
                            AudioType audioFileType = AudioType.UNKNOWN;
                            if (supportedAudio.ContainsKey(extension) == true)
                                audioFileType = supportedAudio[extension];
                            StartCoroutine(DownloadAudio(endpoint, media.labAudio, resourceKey, audioFileType));
                        }
                        break;
                    case "video":
                        if(media.labVideos.ContainsKey(resourceKey) == false)
                            media.labVideos.Add(resourceKey, endpoint); //Nothing to download, just add the endpoint to the dictionary
                        break;
                }
            }
            catalogue.downloadedLabs.Add(media.lab_id, media); //Add a LabMedia object to the MediaCatalogue's downloadedLabs dictionary.
            Debug.Log("Lab media added to catalogue");
        }
    }

    //Function to retrieve the JSON manifests from the server
    IEnumerator DownloadManifest(string url, string response)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
            Debug.Log(uwr.error);
        else
            //Retriveve results, decode binary data, and save as a string
            response = System.Text.Encoding.UTF8.GetString(uwr.downloadHandler.data);
    }

    //Function to sequentially download an image, create Texture2D asset, and store the created texture in a dictionary
    IEnumerator DownloadTexture(string url, Dictionary<Tuple<int, int>, Texture2D> textures, Tuple<int, int> key)
    {
        using(UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
                Debug.Log(uwr.error);
            else
                textures.Add(key, DownloadHandlerTexture.GetContent(uwr));
        }
    }

    //Function to sequentially download audio file, create AudioClip asset, and store the AudioClip in a dictionary 
    IEnumerator DownloadAudio(string url, Dictionary<Tuple<int, int>, AudioClip> audio, Tuple<int, int> key, AudioType fileType)
    {
        using(UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, fileType))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
                Debug.Log(uwr.error);
            else
                audio.Add(key, DownloadHandlerAudioClip.GetContent(uwr));
        }
    }
}