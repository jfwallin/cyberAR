using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

public class LabLogger : MonoBehaviour
{
    #region Variables
    private static LabLogger _instance;
    public static LabLogger Instance
    {
        get
        {
            // Check if there is already an instance assigned
            if(_instance == null)
            {
                // Try to find an existing instance
                LabLogger search = FindObjectOfType<LabLogger>();
                if(search != null)
                {
                    _instance = search;
                }
                else // Could not find existing logger
                {
                    GameObject go = FindObjectOfType<LabManager>()?.gameObject;
                    if (go != null)
                    {
                        _instance = go.AddComponent<LabLogger>();
                    }
                    else // Couldn't find lab manager
                    {
                        Debug.LogError("Could not find LabManager object to attach LabLogger to, returning null");
                    }
                }
            }

            return _instance;
        }
    }

    private FileInfo logFileInfo;
    private string logDirectory = "";                 // Log filepath, will be updated with student's name and id
    private string logFileName = "";             // Name of the logfile, will have student's name and id
    private bool initialized = false;            // Tracks whether the log file has been renamed w/ the M number
    private bool submitted = false;              // Whether the log for the current session has been submitted
    private float startTime = 0.0f;              // Time the logger started
    private float connectionTimer = 0.0f;        // Tracks how long since last connection attempt, as to prevent a timout
    
    [SerializeField]
    private bool uploadLogs = true;
#if UNITY_EDITOR
    [SerializeField]
    private bool uploadAndStop = false;
#endif
    [SerializeField]
    private bool printLogsToConsole = true;
    [SerializeField]
    private bool saveLogsLocally = false;
    [SerializeField]
    private int numberOfLogsStored = 3;
    #endregion Variables

    #region Unity Methods
    public void Awake()
    {
        // If not the only instance
        if ((_instance != null && _instance != this))
        {
            // Destroy self, leave exisitng instance
            Destroy(this);
        }
        else // Only instance
        {
            // Assign self as the singleton instance
            _instance = this;
        }

        // Initialize file w/ persistentData path
        logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
        logFileName = System.DateTime.Now.ToString("MM-dd-yyyy_HH.mm")+".txt";
        logFileInfo = new FileInfo(Path.Combine(logDirectory, logFileName));
        logFileInfo.Directory.Create();
    }

    public void Start()
    {
        startTime = Time.time;
        // Connect to the website immediately, if we are going to upload
        if(uploadLogs)
            StartCoroutine(Connect());

        // Log where the files should be saving to
        InfoLog("LOGGER", "TRACE", $"Saving files to persistent data path: {logFileInfo.FullName}");
        InfoLog("Logger", "debug", $"location of Application.dataPath: {Application.dataPath}");

        // Delete any extra logs
        DirectoryInfo logDirectoryInfo = new DirectoryInfo(logDirectory);
        FileInfo[] files = logDirectoryInfo.GetFiles();
        // Sort array, most recent logs first
        Array.Sort(files, (f1, f2) => f2.CreationTime.CompareTo(f1.CreationTime));
        if(numberOfLogsStored != -1 && files.Length > numberOfLogsStored)
        {
            for(int i=files.Length-1; i>=numberOfLogsStored; i--)
                files[i].Delete();
        }
    }

    public void Update()
    {
        if(uploadLogs)
        {
            // Reconnect to the arlabs website after a timeout occurs
            connectionTimer += Time.deltaTime;
            if (connectionTimer > 3600)
            {
                StartCoroutine(Connect());
            }

#if UNITY_EDITOR
            // Only in the editor, check if we should upload the logs and stop
            if(uploadAndStop)
            {
                uploadAndStop = false;
                StartCoroutine(Upload(finishedUploadEditor));
            }
#endif
        }
    }
    #endregion Unity Methods

    #region Public Methods
    /// <summary>
    /// Sets the filepath using the passed user ID, prints a first line
    /// </summary>
    /// <param name="id">The id entered by the user</param>
    public void InitializeLog(string name, string mNum)
    {
        // Only rename if the log hasn't already been renamed
        if (initialized)
            return;

        // Set the path of the log to be a unique combo of student infor and current date
        logFileName = mNum + "_" + System.DateTime.Now.ToString("MM-dd-yyyy_HH.mm") + ".txt";
        logFileInfo.MoveTo(Path.Combine(logDirectory, logFileName));
        // Add initiliazation statement to the log
        string initText = $"\n\nLog file for student: {name},  M{mNum}.\nCurrent Time: {System.DateTime.Now}\n";
        appendToLog(initText);
        // Mark log as initialized
        initialized = true;
    }

    /// <summary>
    /// Adds information item to the log, prefaced by the time since the log was initialized,
    /// and the entity that sent the information. formatting is left to the sender
    /// </summary>
    /// <param name="entity">name of the component that sent the log request</param>
    /// <param name="tag">descriptive tag for the info, used to make parsing for specific info easier</param>
    /// <param name="information">information to be added to the log, preformatted by sender</param>
    public void InfoLog(string entity, string tag, string information)
    {
        string curTime = System.DateTime.Now.ToString("HH:mm:ss");
        string relTime = $"{Mathf.Round((Time.time-startTime)*100f)/100f}";
        // CURTIME, RELTIME | ENTITY, TAG : INFO
        string log = $"{entity.ToUpper()}, {tag.ToUpper()} | {curTime}, {relTime} : {information}\n";
        appendToLog(log);

        // Print log to console if flag is set, and not final build
        if (Debug.isDebugBuild && printLogsToConsole)
            Debug.Log(log);
    }

    /// <summary>
    /// Called by LoginManager when the application is going to close. Checks whether to upload
    /// or save the logs locally.
    /// </summary>
    /// <param name="onDoneSubmitting">Function to call when log submission is done</param>
    public void SubmitLog(Action onDoneSubmitting)
    {
        // Upload the files when it is a final build, or when it is a debug build with upload flag
        if(!Debug.isDebugBuild || (Debug.isDebugBuild && uploadLogs))
        {
            InfoLog("LOGGER", "SUBMIT",
                $"Application ending after {Time.time - startTime} seconds, " +
                $"time is now {DateTime.Now.ToString("HH:mm")}");
            StartCoroutine(Upload(onDoneSubmitting));
        }

        // Delete local logs only if non-debug final build, or debug build without save local flag
        if (!Debug.isDebugBuild || (Debug.isDebugBuild && !saveLogsLocally))
            deleteLocalLogs();

        // Mark log as submitted
        submitted = true;
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Writes a line to the log file
    /// </summary>
    /// <param name="line">Line to be appended to the log</param>
    private void appendToLog(string line)
    {
        File.AppendAllText(logFileInfo.FullName, line);
    }

    /// <summary>
    /// Removes the local log file constructed during program runtime, prevents clutter
    /// </summary>
    private void deleteLocalLogs()
    {
        logFileInfo.Delete();
    }
    #endregion Private Methods

    #region Coroutines
    /// <summary>
    /// Connect to cyberlearnar server with email and password
    /// </summary>
    private IEnumerator Connect()
    {
        InfoLog("LOGGER", "CONNECT", $"Connect called at " + DateTime.Now.ToString());
      
        //create a webForm object
        WWWForm form = new WWWForm();

        //add login information the form
        form.AddField("email", "rafet.al-tobasei@mtsu.edu");
        form.AddField("password", "DNkZOY");
        form.AddField("username", "rafet.al-tobasei@mtsu.edu");

        //submit information the server 
        UnityWebRequest www = UnityWebRequest.Post("http://cyberlearnar.cs.mtsu.edu/login", form);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            print("Form upload complete!");
        }
    }

    private IEnumerator Upload(Action doneUploading)
    {
        //Convert the file into binary
        byte[] txtByte = File.ReadAllBytes(logFileInfo.FullName);
        //create a webForm object
        WWWForm form = new WWWForm();

        //public void AddBinaryData(string fieldName, byte[] contents, string fileName = null, string mimeType = null);
        //this function to upload files and images to a web server application.
        //Note that the data is read from the contents of byte array and not from a file.
        //The fileName parameter is for telling the server what filename to use when saving the uploaded file.
        form.AddBinaryData("file", txtByte, logFileName, "txt");

        //// submit file to server
        UnityWebRequest www = UnityWebRequest.Post("http://cyberlearnar.cs.mtsu.edu/upload_file", form);
        yield return www.SendWebRequest();
        // Check result
        if(Debug.isDebugBuild)
        {
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form Upload Completed!");
            }
        }

        // If we are supposed to delete the local logs, do so now
        if(!Debug.isDebugBuild || (Debug.isDebugBuild && !saveLogsLocally))
        {
            deleteLocalLogs();
        }

        doneUploading.Invoke();
    }
    #endregion Coroutines
#if UNITY_EDITOR

    #region Event Handlers
    /// <summary>
    /// Stops the editor after the log is uploaded
    /// </summary>
    private void finishedUploadEditor()
    {
        UnityEditor.EditorApplication.isPlaying = false;
    }
    #endregion Event Handlers
#endif
}