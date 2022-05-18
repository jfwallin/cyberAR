using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles file download requests, stores relative to Resources folder
/// </summary>
public class DownloadUtility : MonoBehaviour
{
    #region Variables
    // Local File Toggle
    [Header("Toggle Downloading")]
    [Tooltip("This toggle allows for use of local files instead of pulling them from the web")]
    public bool useLocalFiles = false;

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
    /// <param name="path">filepath to store at, relative to resources folder</param>
    /// <param name="callback">function to call once the download is complete</param>
    public void DownloadFile(string url, string path, System.Action<int> callback)
    {
        if(!useLocalFiles)
        {
            logger.InfoLog(entity, "Trace", $"Starting download of file ${path} from url: {url}");
            StartCoroutine(downloadRoutine(url, "Assets/Resources/" + path, callback));
        }
        else // Use local files
        {
            // Check if the local file is there
            if(Resources.Load(path.Substring(0, path.LastIndexOf("."))) != null)
                callback.Invoke(0); // Local file exists
            else // There is no local file
                callback.Invoke(-1); // Local file does not exist
        }
    }
    #endregion Public Methods

    #region Coroutines
    private IEnumerator downloadRoutine(string url, string path, System.Action<int> callback)
    {
        // Download code taken from Nico's work
        var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
        uwr.downloadHandler = new DownloadHandlerFile(path);
        yield return uwr.SendWebRequest();
        if (uwr.result != UnityWebRequest.Result.Success)
        {
            logger.InfoLog(entity, "Trace", $"Download of {path} failed with error:\n{uwr.error}");
            // Invoke callback w/-1, telling client the download failed
            callback.Invoke(-1);
        }
        else
        {
            logger.InfoLog(entity, "Trace", $"Successfully downloaded {path}");
            // Invoke callback w/0, telling client the download succeeded
            callback.Invoke(0);
        }
    }
    #endregion Coroutines
}
