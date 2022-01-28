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
    }
    #endregion Unity Methods

    /// <summary>
    /// Called by any other code to download a file
    /// </summary>
    /// <param name="url">endpoint to download file from</param>
    /// <param name="path">filepath to store at, relative to resources folder</param>
    /// <param name="callback">function to call once the download is complete</param>
    public void DownloadFile(string url, string path, System.Action callback)
    {
        StartCoroutine(downloadRoutine(url, "Assets/Resources/" + path, callback));
    }

    private IEnumerator downloadRoutine(string url, string path, System.Action callback)
    {
        // Download code taken from Nico's work
        var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
        uwr.downloadHandler = new DownloadHandlerFile(path);
        yield return uwr.SendWebRequest();
        if (uwr.result != UnityWebRequest.Result.Success)
            Debug.LogError(uwr.error);
        else
            Debug.Log("File successfully downloaded and saved to " + path + "\n");
        
        // Download complete, notify original program
        callback.Invoke();
    }
}
