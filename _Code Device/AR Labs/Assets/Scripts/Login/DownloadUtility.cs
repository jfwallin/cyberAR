using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles file download requests, stores relative to Resources folder
/// </summary>
public class DownloadUtility : MonoBehaviour
{
    #region Variables
    // Singelton implementation
    private static DownloadUtility _instance;
    public static DownloadUtility Instance
    {
        get
        {
            // Check if there is already an instance assigned
            if(_instance == null)
            {
                // Try to find an existing instance
                DownloadUtility search = FindObjectOfType<DownloadUtility>();
                if (search != null)
                    _instance = search;
                else // No instance found
                {
                    // Find object to attach DownloadUtility to
                    GameObject go = FindObjectOfType<LabManager>()?.gameObject;
                    if (go != null)
                    {
                        _instance = go.AddComponent<DownloadUtility>();
                    }
                    else
                        Debug.LogError("Could not find instance of DownloadUtility, returning null");
                }
            }

            return _instance;
        }
    }

    private LabLogger logger;  //reference to logger that prints writes information to file
    private string entity;     //name of this script, gets sent to the logger
    #endregion Variables

    #region Unity Methods
    public void Awake()
    {
        // Singleton management
        // If not the only instance:
        if ((_instance != null && _instance != this))
        {
            // Destroy self, leave exisitng instance
            Destroy(this);
        }
        else // Only instance
        {
            // Assign self as the instance
            _instance = this;
        }

        logger = LabLogger.Instance;
        entity = this.GetType().ToString();
    }
    #endregion Unity Methods

    #region Public Methods
    /// <summary>
    /// Called by any other code to download a file
    /// </summary>
    /// <param name="url">endpoint to download file from</param>
    /// <param name="path">full filepath to store at</param>
    /// <param name="callback">function to call once the download is complete,
    ///                        called with -1: error occured, 0: success</param>
    public void DownloadFile(string url, string path, System.Action<int> callback, bool tryUseLocalFiles=true)
    {
        if(!tryUseLocalFiles)
        {
            //if (File.Exists(path))
            //{
            //    logger.InfoLog(entity, "Debug", $"File {path} already exists");
            //    callback.Invoke(0);
            //    return;
            //}
            logger.InfoLog(entity, "TRACE", $"Starting download of file ${path} from url: {url}");
            StartCoroutine(downloadRoutine(url, path, callback));
        }
        else // Try to Use local files
        {
            // Check if the local file is there
            if(File.Exists(path))
            {
                logger.InfoLog(entity, "TRACE", $"Found local file ${path}, Not downloading");
                callback.Invoke(0); // Local file exists
            }
            else // There is no local file
            {
                // Try to download it then
                logger.InfoLog(entity, "TRACE", $"Could not find local file ${path}, Trying to download from {url}");
                StartCoroutine(downloadRoutine(url, path, callback));
            }
        }
    }

    /// <summary>
    /// Called to download and then extract a zip file
    /// </summary>
    /// <param name="url">endpoint to download from</param>
    /// <param name="path">full filepath to store at</param>
    /// <param name="callback">function to call once the download is complete,
    ///                        called with -1: error occured, 0: success</param>
    public void DownloadAndExtractZip(string url, string path, System.Action<int> callback, bool tryUseLocalFiles=true)
    {
        if (!tryUseLocalFiles)
        {
            // Check if files exist locally, remove it if it does
            /*if (File.Exists(path))
            {
                callback.Invoke(0);
                return;
            }*/
            logger.InfoLog(entity, "Debug", $"Starting download of file ${path} from url: {url}");
            StartCoroutine(downloadRoutine(url, path, (int x) => ExtractZip(x, path, callback)));
        }
        else // Use local files
        {
            // Check if the local file is there
            if (File.Exists(path))
            {
                logger.InfoLog(entity, "TRACE", $"Found local file ${path}, Not downloading");
                ExtractZip(0, path, callback); // Local file exists, continue to extraction
            }
            else // There is no local file
            {
                logger.InfoLog(entity, "TRACE", $"Could not find local file ${path}, Trying to download from {url}");
                StartCoroutine(downloadRoutine(url, path, (int x) => ExtractZip(x, path, callback)));
            }
        }
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Extracts a zip file at 'path' into the same folder
    /// </summary>
    /// <param name="rc">The return code from the download step</param>
    /// <param name="path">The location of the zip file to extract</param>
    /// <param name="callback">Function to call once extraction is complete,
    ///                        called with -1: error occured, 0: success</param>
    private void ExtractZip(int rc, string path, System.Action<int> callback)
    {
        // Check the return code to see if the file successfully downloaded
        if (rc == -1)
        {
            callback.Invoke(-1);
            return;
        }
        // Check the file path
        if (!File.Exists(path) || !path.EndsWith(".zip"))
        {
            logger.InfoLog(entity, "Error",
                $"Failed to extract zip file '{path}', file either does not exist or does not end with '.zip'");
            callback.Invoke(-1);
            return;
        }
        logger.InfoLog(entity, "Trace", $"Starting to extract file: {path}");

        // Open the zip file and iterate through each item in the compressed archive
        FileStream zipstream = new FileStream(path, FileMode.Open);
        ZipArchive archive = new ZipArchive(zipstream);
        logger.InfoLog(entity, "Debug",
            $"Number of entries to extract: {archive.Entries.Count}");
        foreach(ZipArchiveEntry entry in archive.Entries)
        {
            // If this entry is not a file but just a folder, it's name will be blank, and don't extract it
            if(entry.Name.Length != 0)
            {
                logger.InfoLog(entity, "Debug",
                    $"Extracting file: {entry.Name}");
                FileInfo entryInfo = new FileInfo(Path.Combine(
                    Application.persistentDataPath,
                    "lab_resources",
                    entry.FullName));
                // Make sure that the directory to put it in exists
                entryInfo.Directory.Create();
                // If the file already exists, delete it and replace
                if(File.Exists(entryInfo.FullName))
                    File.Delete(entryInfo.FullName);
                entry.ExtractToFile(entryInfo.FullName);
            }
        }
        // Clean up
        archive.Dispose();
        // Notify caller that extraction is complete
        callback.Invoke(0);
    }
    #endregion

    #region Coroutines
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            //Simply return true no matter what
            return true;
        }
    }

    private IEnumerator downloadRoutine(string url, string path, System.Action<int> callback)
    {
        // Download code taken from Nico's work
        using(var uwr = UnityWebRequest.Get(url))
        {
            uwr.certificateHandler = new BypassCertificate();
            uwr.downloadHandler = new DownloadHandlerFile(path);
            logger.InfoLog(entity, "Debug", $"Inside download coroutine, URL: {url}, path: {path}");
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                logger.InfoLog(entity, "Error", $"Download of {path} failed with error:\n{uwr.result.ToString()} | {uwr.error}");
                // Invoke callback w/-1, telling client the download failed
                uwr.Dispose();
                callback.Invoke(-1);
            }
            else
            {
                logger.InfoLog(entity, "Debug", $"Successfully downloaded {path}");
                // Invoke callback w/0, telling client the download succeeded
                uwr.Dispose();
                callback.Invoke(0);
            }
        }
        // var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
    }
    #endregion Coroutines
}
