using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.IO.Compression;
using System.Linq;

//csc.rsp file required for System.IO.Compression.dll file (found in assets folder)

public class MediaCatalogue : MonoBehaviour
{
    #region Variables
    // Singleton access
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

    // Public Variables
    // Dictionaries for media assets
    public Dictionary<string, Texture2D> labTextures = new Dictionary<string, Texture2D>();
    public Dictionary<string, AudioClip> labAudio = new Dictionary<string, AudioClip>();
    public Dictionary<string, string> labVideos = new Dictionary<string, string>();
    
    // Public flag to indicate the catalogue is ready to be used
    public bool DoneLoadingAssets { get => _doneLoadingAssets; }

    // Private variables
    private bool _doneLoadingAssets = false; // Flag indicating when the media catalogue is ready to be used, retreived by public property
    private bool initializeCalled = false;   // Flag indicating that catalaogue initialization has started, but isn't necessarily done yet
    private int numResources;                // Number of resources that need to be loaded, used to check if loading is done
    // List of folder names that are looped through to load assets, name determines how asset is loaded
    List<string> ResourceFolderNames = new List<string> { "Audios", "Textures", "Videos" };

    // Used to convert audio file extensions into AudioType instances
    private Dictionary<string, AudioType> AudioTypeMap = new Dictionary<string, AudioType>()
    {
        { "wav", AudioType.WAV },
        { "ogg", AudioType.OGGVORBIS },
        { "mp3", AudioType.MPEG }
    };
    #endregion Variables

    #region Unity Methods
    private void Awake()
    {
        // Check if there is already another instance, destroy self if that is the case
        if((_instance != null && _instance != this))
        {
            Destroy(this);
        }
        else //Only instance
        {
            _instance = this;
        }

        // Set flag to uninitialized
        _doneLoadingAssets = false;
    }
    #endregion Unity Methods

    #region Public Methods
    /// <summary>
    /// Loads all resources needed by the lab from the disk
    /// </summary>
    /// <param name="labResourcesFileInfo">FileInfo describing the directory holding the lab resources</param>
    public void InitializeCatalogue(DirectoryInfo labResourcesFolderInfo)
    {
        Debug.Log($"Started catalogue, folder: {labResourcesFolderInfo.FullName}, exists: {labResourcesFolderInfo.Exists}, subfolders: {new List<DirectoryInfo>(labResourcesFolderInfo.GetDirectories()).Aggregate("", (acc, x) => acc + x.Name)}");
        // Check if the catalogue has already been initialized.
        // If it has, then clear it out before re-initializing
        if(initializeCalled)
        {
            labAudio.Clear();
            labTextures.Clear();
            labVideos.Clear();
            numResources = 0;
        }
        else // Set flag, so Update can start checking that all the files have been downloaded
            initializeCalled = true;

        // Count the number of files that need to need to be loaded
        foreach(DirectoryInfo folder in labResourcesFolderInfo.GetDirectories())
        {
            // Record the number of files to load, or skip the folder if it is not named like an asset folder
            if (ResourceFolderNames.Contains(folder.Name))
                numResources += folder.GetFiles().Length;
        }

        // Log start of loading
        LabLogger.Instance.InfoLog(
            this.GetType().ToString(),
            LabLogger.LogTag.DEBUG,
            $"Started Initializing Media Catlogue, {numResources} resources to load");

        // Start Asrynchronously loading the media
        StartCoroutine(LoadMedia(labResourcesFolderInfo));
    }

    /// <summary>
    /// Retrieve a texture from the catalogue using the filename of the texture without file extensions,
    /// If it is not found, null is returned
    /// </summary>
    /// <param name="textureName">Filename of the texture, without file extension</param>
    /// <returns></returns>
    public Texture2D GetTexture(string textureName)
    {
        if (labTextures.ContainsKey(textureName))
            return labTextures[textureName];
        else // Texture not found in Catalogue
        {
            LabLogger.Instance.InfoLog(
                this.GetType().ToString(),
                LabLogger.LogTag.ERROR,
                $"Could not find texture in catalogue: {textureName}, returning null");
            return null;
        }
    }

    /// <summary>
    /// Retrieve an AudioClip from the catalogue using the filename of the audio without file extensions,
    /// If it is not found, null is returned
    /// </summary>
    /// <param name="audioName">Filename of the audio, without file extension</param>
    /// <returns></returns>
    public AudioClip GetAudioClip(string audioName)
    {
        if (labAudio.ContainsKey(audioName))
            return labAudio[audioName];
        else // Audio not found in Catalogue
        {
            LabLogger.Instance.InfoLog(
                this.GetType().ToString(),
                LabLogger.LogTag.ERROR,
                $"Could not find audio in catalogue: {audioName}, returning null");
            return null;
        }
    }

    /// <summary>
    /// Retrieves the URI of a video on disk from the catalogue using the name of the video without file extensions,
    /// If it is not found, an empty string is returned
    /// </summary>
    /// <param name="videoName">Filename of the video, without file extensions</param>
    /// <returns></returns>
    public string GetVideoURI(string videoName)
    {
        if (labVideos.ContainsKey(videoName))
            return labVideos[videoName];
        else // Video not found in Catalogue
        {
            LabLogger.Instance.InfoLog(
                this.GetType().ToString(),
                LabLogger.LogTag.ERROR,
                $"Could not find video in catalogue: {videoName}, returning empty string");
            return "";
        }
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Asynchronously loads the files, I think
    /// </summary>
    /// <param name="labResourcesFolderInfo">Directory Info pointing to the media for the lab</param>
    /// <returns></returns>
    private IEnumerator LoadMedia(DirectoryInfo labResourcesFolderInfo)
    {
        // Loop through the subdirectories and load files
        foreach(DirectoryInfo folder in labResourcesFolderInfo.GetDirectories())
        {
            // Only load the files if it is an asset folder name
            if (ResourceFolderNames.Contains(folder.Name))
            {
                // Loop through the files in the subdirectories
                foreach(FileInfo file in folder.GetFiles())
                {
                    LabLogger.Instance.InfoLog(
                        this.GetType().ToString(),
                        LabLogger.LogTag.DEBUG,
                        $"Started loading file: {file.Name}");
                    // Only Load files from folders describing asset types
                    if (folder.Name == "Audios")
                        yield return LoadAudio(file);
                    else if (folder.Name == "Textures")
                        yield return LoadTexture(file);
                    else if (folder.Name == "Videos")
                        LoadVideo(file); // Not a coroutine b/c it doesn't use web request
                }
            }
        }
        // Indicate that the resources are done loading
        _doneLoadingAssets = true;
    }

    /// <summary>
    /// Loads a Texture asset from disk into the catalogue
    /// </summary>
    /// <param name="textureFileInfo">FileInfo describing a texture file on disk</param>
    private IEnumerator LoadTexture(FileInfo textureFileInfo)
    {
        // Construct URI for the file, automatically adds file:// to the path
        System.Uri textureURI = new System.Uri(textureFileInfo.FullName);
        
        // Create the webrequest to load the data
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(textureURI))
        {
            // Send request
            yield return uwr.SendWebRequest();

            // Once it returns, check if it was successful
            if (uwr.result != UnityWebRequest.Result.Success)
                LabLogger.Instance.InfoLog(
                    this.GetType().ToString(),
                    LabLogger.LogTag.ERROR,
                    $"Error loading texture asset: {uwr.error}");
            else // If successful, add the texture to the catalogue
                labTextures.Add(
                    textureFileInfo.Name.Substring(0, textureFileInfo.Name.LastIndexOf(".")),
                    DownloadHandlerTexture.GetContent(uwr));
        }
    }

    /// <summary>
    /// Loads an Audio asset from disk into the catalogue
    /// </summary>
    /// <param name="audioFileInfo">FileInfo describing an audio file on disk</param>
    private IEnumerator LoadAudio(FileInfo audioFileInfo)
    {
        // Convert the file extension to an AudioType instance
        AudioType type = AudioTypeMap[audioFileInfo.Name.Substring(audioFileInfo.Name.LastIndexOf(".") + 1)];

        // Construct URI for the file, automatically adds file:// to the path
        System.Uri audioURI = new System.Uri(audioFileInfo.FullName);

        // Create the webrequest to load the data
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(audioURI, type))
        {
            // Send request
            yield return uwr.SendWebRequest();

            // Once it returns, check if it was successful
            if (uwr.result != UnityWebRequest.Result.Success)
                LabLogger.Instance.InfoLog(
                    this.GetType().ToString(),
                    LabLogger.LogTag.ERROR,
                    $"Error loading audio asset: {uwr.error}");
            else // If successful, add to the catalogue
                labAudio.Add(
                    audioFileInfo.Name.Substring(0, audioFileInfo.Name.LastIndexOf(".")),
                    DownloadHandlerAudioClip.GetContent(uwr));
        }
    }

    /// <summary>
    /// Adds a video from disk into the catalogue by tracking its full file location
    /// </summary>
    /// <param name="videoFileInfo">FileInfo describing a video file on disk</param>
    private void LoadVideo(FileInfo videoFileInfo)
    {
        // Simply add the file location to the dictionary, since it can be streamed from the folder
        labVideos.Add(
            videoFileInfo.Name.Substring(0, videoFileInfo.Name.LastIndexOf(".")),
            videoFileInfo.FullName);
    }
    #endregion Private Methods

    #region Old Catalogue
    /*
    public void addToCatalogue(LabDataObject data)
    {
        StartCoroutine(DownloadLabMedia(data.Assets));
    }

    // Retrieve a Texture2D asset for a lab
    // textureKey is the url that the raw image file is retrieved from
    // If the Texture2D asset isn't found an error message is displayed in the debug console and a null value is returned.
    public Texture2D GetLabTexture(string textureKey) // Safe gaurds against possible runtime errors need to be implemented still
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

    // Retrieve an AudioClip asset for a lab
    // audioKey is the url that the raw image file is retrieved from
    // If AudioClip asset isn't found, an error message is displayed in the debug console and a null value is returned
    public AudioClip GetLabAudioClip(string audioKey) // Safe gaurds against possible runtime errors need to be implemented still
    {
        AudioClip retrievedAudioClip = null;
        if (labAudio.ContainsKey(audioKey))
            retrievedAudioClip = labAudio[audioKey];
        else
            UnityEngine.Debug.Log("AudioClip asset not found.");
        return retrievedAudioClip;
    }

    // Retrieve the API endpoint for a lab video
    // videoKey is the url that the raw image file is retrieved from
    // If API endpoint isn't found, an error message is displayed in the debug console and a null value is returned.
    public string GetLabVideoURL(string videoKey) // Safe gaurds against possible runtime errors need to be implemented still
    {
        string retrievedVideoURL = null;
        if (labVideos.ContainsKey(videoKey))
            retrievedVideoURL = labVideos[videoKey];
        else
            UnityEngine.Debug.Log("Video file not found.");
        return retrievedVideoURL;
    }

    // Removes a specified downloaded resource from the Catalogue
    public void DeleteDownloadedMediaAsset(string resourceKey)
    {
        if (labTextures.ContainsKey(resourceKey))
            labTextures.Remove(resourceKey);
        else if (labAudio.ContainsKey(resourceKey))
            labAudio.Remove(resourceKey);
        else if (labVideos.ContainsKey(resourceKey))
        {
            File.Delete(labVideos[resourceKey]); // Delete video file from disk
            labVideos.Remove(resourceKey);       // Remove entry from dictionary
        }
        else
            UnityEngine.Debug.Log("Resource not found.");
    }

    // Clears all downloaded assets of a specific media type
    // mediaType should be set to "image", "audio", or "video"
    // passing "video" will delete all downloaded video files from disk
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
                Dictionary<string, string>.ValueCollection videoFilePaths = labVideos.Values; // Get a collection of filepaths for all downloaded video files
                foreach (string videoFilePath in videoFilePaths)
                {
                    File.Delete(videoFilePath); // Delete each video file from disk
                }
                labVideos.Clear(); // Clear the labVideos dictionary
                break;
        }
    }

    // Clears all downloaded media asset dictionaries and deletes all downloaded video files that are saved to disk
    public void DeleteAllDownloadedMediaAssets()
    {
        labTextures.Clear();
        labAudio.Clear();
        Dictionary<string, string>.ValueCollection videoFilePaths = labVideos.Values; // Get a collection of filepaths for all downloaded video files
        foreach (string videoFilePath in videoFilePaths)
        {
            File.Delete(videoFilePath); // Delete each video file from disk
        }
        labVideos.Clear(); // Clear teh labVideos dictionary
    }

    // Downloads an image file, creates a Texutre2D asset, and stores the asset in the labTextures dictionary
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

    // Downloads an audio file, create an AudioClip asset, and stores the asset in the labAudio dictionary
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

    // Downloads url endpoint of mp4 file and saves the url endpoint to disk.
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
                string filename = url.Substring(url.LastIndexOf("/")+1);
                //Path to downloaded video file
                string videoPath = Path.Combine(Application.persistentDataPath,
                                                Path.Combine("videos", filename));
                File.WriteAllBytes(videoPath, uwr.downloadHandler.data); //Save video file to disk
                labVideos.Add(url, videoPath); //Add path to video to the videos dictionary
            }
        }
    }

    // Creates Unity media assets (Texture2D/AudioClip) from a downloaded zip file
    // Downloads a zip file containing JPEG or WAVE files, unzips in memory, and processes each entry in the zip file
    // Untested
    IEnumerator DownloadZipFile(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
                Debug.Log(uwr.error);
            else
            {
                Stream data = new MemoryStream(uwr.downloadHandler.data);
                Stream unzippedEntryStream;
                ZipArchive archive = new ZipArchive(data);

                // Process each zipped raw media asset in the zip file
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    Debug.Log(entry.Name);

                    // Determine extension of raw media asset file
                    int index = entry.Name.LastIndexOf('.');
                    string extension = entry.Name.Substring(index + 1);

                    unzippedEntryStream = entry.Open();                      // Open a zipped entry contained in the zip file
                    MemoryStream unzippedMemoryStream = new MemoryStream();  // Convert the Stream to a MemoryStream
                    unzippedEntryStream.CopyTo(unzippedMemoryStream);        // Copy the Stream to the MemoryStream (necessary to use ToArray() method)
                    byte[] unzippedData = unzippedMemoryStream.ToArray();    // Convert the MemoryStream to a byte array

                    // Determine the file type of the unzipped media asset, create the appropriate Unity media asset, & add the new Unity media asset to the appropriate dictionary
                    switch (extension)
                    {
                        case "jpg":
                            Texture2D unzippedTexture = new Texture2D(2, 2);
                            unzippedTexture.LoadImage(unzippedData);
                            labTextures.Add(entry.Name, unzippedTexture);
                            break;
                        case "wav":
                            WavHandler wav = new WavHandler(unzippedData);
                            AudioClip unzippedAudioClip = AudioClip.Create(entry.Name, wav.GetScaledAudioSamplesLength(), wav.GetNumChannels(), wav.GetFrequency(), false);
                            unzippedAudioClip.SetData(wav.GetScaledAudioSamples(), 0);
                            labAudio.Add(entry.Name, unzippedAudioClip);
                            break;
                        default:
                            Debug.Log("Unsupported media asset file type.");
                            Debug.Log("Supported media asset file types: JPEG and WAVE.");
                            break;
                    }
                }
            }
        }
    }

    // Downloads all the media assets for a single lab
    IEnumerator DownloadLabMedia(MediaInfo[] labAssetList)
    {
        // Currently supported audio file types
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
            switch (asset.resource_type) // Determine type of asset to be downloaded
            {
                case MediaType.Image:
                    if(labTextures.ContainsKey(resourceKey) == false)
                        yield return StartCoroutine(DownloadTexture(resourceKey));
                    break;
                case MediaType.Audio:
                    if(labAudio.ContainsKey(resourceKey) == false)
                    {
                        // Determine file name and audio type
                        int index = asset.resource_url.IndexOf('/');
                        string fileName = asset.resource_url.Substring(index + 1);
                        string extension = fileName.Substring(fileName.Length - 3);
                        AudioType audioFileType = AudioType.UNKNOWN;

                        // Determine if the audio file format is currently supported
                        // If it's not supported the AudioType remains UNKNOWN (may cause issues)
                        if (supportedAudio.ContainsKey(extension) == true)
                            audioFileType = supportedAudio[extension];
                        yield return StartCoroutine(DownloadAudio(resourceKey, audioFileType));
                    }
                    break;
                case MediaType.Video:
                    if (labVideos.ContainsKey(resourceKey) == false)
                        yield return StartCoroutine(DownloadVideo(resourceKey));
                    break;
                case MediaType.Zip: // Untested and will need to add Zip to the MediaType on server side
                    yield return StartCoroutine(DownloadZipFile(resourceKey));
                    break;
            }
        }
        doneLoadingAssets = true;
    }
    */
    #endregion Old Catalogue
}