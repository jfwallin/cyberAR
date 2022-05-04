using System.Collections;
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

    private string LogPath = "Assets/Resources/Logs/";// Log filepath, will be updated with student's name and id
    private string logFilname = "";              // Name of the logfile, will have student's name and id
    private bool initialized = false;            // Whether the student has logged in, and the log initialized
    private string preInitLogs = "";             // All logs made pre-Init, will be added to the actual log on Init
    private float startTime = 0.0f;               // Time the logger started
    private float connectionTimer = 0.0f;        // Tracks how long since last connection attempt, as to prevent a timout

    [SerializeField]
    private bool saveLogsLocally = false;
    [SerializeField]
    private bool uploadLogs = true;
    #endregion Variables

    #region Unity Methods
    public void Awake()
    {
        //If not the only instance
        if ((_instance != null && _instance != this))
        {
            //Destroy self, leave exisitng instance
            Destroy(this);
        }
        else //Only instance
        {
            //Assign self as the instance
            _instance = this;
        }
    }

    public void Start()
    {
        startTime = Time.time;
        //Connect to the website immediately, if we are going to upload
        if(uploadLogs)
            StartCoroutine(Connect());
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
        // Set the path of the log to be a unique combo of student infor and current date
        logFilname = mNum + "_" + System.DateTime.Now.ToString("MM-dd-yyyy_HH:mm") + ".txt";
        LogPath = LogPath + logFilname;
        initialized = true;
        // Add initiliazation statement to the log
        appendToLog($"\n\nLog file for student: {name},  M{mNum}.\nCurrent Time: {System.DateTime.Now}\n");
        // Put in all the logs that were cached prior to initialization
        appendToLog(preInitLogs);
    }

    /// <summary>
    /// Adds information item to the log, prefaced by the time since the log was initialized,
    /// and the entity that sent the information. formatting is left to the sender
    /// </summary>
    /// <param name="entity">name of the component that sent the log request</param>
    /// <param name="information">information to be added to the log, preformatted by sender</param>
    public void InfoLog(string entity, string tag, string information)
    {
        string curTime = System.DateTime.Now.ToString("HH:mm");
        string relTime = $"{Time.time - startTime}";
        // CURTIME, RELTIME | ENTITY, TAG : INFO
        string log = $"{curTime}, {relTime} | {entity.ToUpper()}, {tag.ToUpper()} : {information}";

        if(initialized)
        {
            appendToLog(log);
        }
        else // Log path not yet initialized, so store line to be added later
        {
            preInitLogs += log + "\n";
        }
    }

    /// <summary>
    /// Called by LoginManager when the application is going to close. Checks whether to upload
    /// or save the logs locally.
    /// </summary>
    /// <param name="onDoneSubmitting">Function to call when log submission is done</param>
    public void SubmitLog(Action onDoneSubmitting)
    {
        // If the user never made it to the login phase, then setup the log w/out their id
        if(!initialized)
        {
            logFilname = System.DateTime.Now.ToString("MM-dd-yyyy_HH:mm") + ".txt";
            LogPath = LogPath + logFilname;
            appendToLog($"Log file for unknown user.\nCurrent Time: {System.DateTime.Now}\n");
            appendToLog(preInitLogs);
        }

        // Upload the files when it is not a development build, or when it is, and the upload flag is set
        if(!Debug.isDebugBuild || (Debug.isDebugBuild && uploadLogs))
        {
            StartCoroutine(Upload(onDoneSubmitting));
            InfoLog(this.GetType().ToString(), "Submit",
                $"Application ending after {Time.time - startTime} seconds, " +
                $"time is now {DateTime.Now.ToString("HH:mm")}");
            Debug.Log($"Application ending after {Time.time - startTime} seconds, " +
                $"time is now {DateTime.Now.ToString("HH:mm")}");
        }    
        else // Not uploading logs
        {
            // local log files are always created by default.
            // Do not delete them if it is a development build with the "saveLocalLogs" flag set
            // Only delete local logs here if we are not trying to upload them first, in which case they will
            // be deleted after the upload is finished
            if(!Debug.isDebugBuild || (Debug.isDebugBuild && !saveLogsLocally))
                deleteLocalLogs();
        }
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Writes a line to the log file
    /// </summary>
    /// <param name="line">Line to be appended to the log</param>
    private void appendToLog(string line)
    {
        StreamWriter myfile = File.AppendText(LogPath);
        myfile.WriteLine(line);
        myfile.Close();
    }

    /// <summary>
    /// Removes the local log file constructed during program runtime, prevents clutter
    /// </summary>
    private void deleteLocalLogs()
    {
        File.Delete(LogPath);
    }
    #endregion Private Methods

    #region Coroutines
    /// <summary>
    /// Connect to cyberlearnar server with email and password
    /// </summary>
    private IEnumerator Connect()
    {
        print($"Connect called at { DateTime.Now.ToString()} ");
      
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
        byte[] txtByte = File.ReadAllBytes(LogPath); //("Assets/Resources/test2.txt");
        //create a webForm object
        WWWForm form = new WWWForm();

        //public void AddBinaryData(string fieldName, byte[] contents, string fileName = null, string mimeType = null);
        //this function to upload files and images to a web server application.
        //Note that the data is read from the contents of byte array and not from a file.
        //The fileName parameter is for telling the server what filename to use when saving the uploaded file.
        form.AddBinaryData("file", txtByte, logFilname, "txt");

        //// submit file to server
        UnityWebRequest www = UnityWebRequest.Post("http://cyberlearnar.cs.mtsu.edu/upload_file", form);
        yield return www.SendWebRequest();
        // Check result
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }

        // If we are supposed to delete the local logs, do so now
        if(!Debug.isDebugBuild || (Debug.isDebugBuild && !saveLogsLocally))
        {
            deleteLocalLogs();
        }

        doneUploading.Invoke();
    }
    #endregion Coroutines
}
