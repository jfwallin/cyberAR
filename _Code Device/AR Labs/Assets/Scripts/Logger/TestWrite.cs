﻿using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public enum InputType { info, Media, Debug, MCQ, TFQ, SAQ, OrderQ}
public class TestWrite : MonoBehaviour
{
    #region Variables
    //Singleton access
    private static TestWrite _instance;
    public static TestWrite Instance
    {
        get
        {
            //Check if there is already an instance assigned
            if (_instance == null)
            {
                //Try to find an existing catalogue, assign it if found
                TestWrite search = FindObjectOfType<TestWrite>();
                if (search != null)
                {
                    _instance = search;
                }
                else //Could find no existing catalogue
                {
                    //Try to find the LabManager, add it to that same GameObject
                    GameObject go = GameObject.FindObjectOfType<LabManager>()?.gameObject;
                    if (go != null)
                    {
                        _instance = go.AddComponent<TestWrite>();
                    }
                    else //Could not find a lab manager, try the "_DYNAMIC" object instead
                    {
                        //Try to find the [_DYNAMIC] GameObject, attach
                        GameObject dyn = GameObject.Find("[_DYNAMIC]");
                        if (dyn != null)
                        {
                            _instance = go.AddComponent<TestWrite>();
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

    private string LogPath = "Assets/Resources/"; // Log filepath, will be updated with student's name and id
    private string logFilname = "";              // Name of the logfile, will have student's name and id
    private bool initialized = false;            // Whether the student has logged in, and the log initialized
    private string preInitLogs = "";             // All logs made pre-Init, will be added to the actualy log on Init
    private float initTime = 0.0f;               // Time the log was initialized with student's name and id
    private float prevTime = 0.0f;               // Time of the last log call
    private float connectionTimer = 0.0f;        // Tracks how long since last connection attempt, as to prevent a timout
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
        //Connect to the website immediately.
        StartCoroutine(Connect());
    }

    public void Update()
    {
        // Reconnect to the arlabs website after a timeout occurs
        connectionTimer += Time.deltaTime;
        if (connectionTimer > 3600)
        {
            Connect();
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
        // Mark the time at which the log was initialized.
        initTime = Time.time;
        prevTime = initTime;
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
        string relTime = $"{Time.time - initTime}";
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
    /// Writes information to the log, depends on what type of information.
    /// </summary>
    /// <param name="type">Type of information to log, possibilities are:
    /// Info, Media, Debug, MCQ,TFQ, SAQ, OrderedQ</param>
    /// <param name="name">Depends on input type</param>
    /// <param name="other">Extra optional strings to print</param>
    public void WriteToString(InputType type, string name, params string[] other )
    {
        //Open file for writting append if exists
        StreamWriter myfile = File.AppendText(LogPath);
        //Get the input type and teh current time            
        InputType TYPE = (InputType)type;
        float currentTime = Time.realtimeSinceStartup;
        float usedTime = currentTime -prevTime ;
        prevTime = currentTime;
        //based on type select teh output format
        switch (TYPE)
        {
            // We need to gree in teh format of the infor that will be send 
            // such as First name , Last name, ID, Lab name, and other information
            case InputType.info: 
                for (int i = 0; i < other.Length-1; i+=2)
                    myfile.WriteLine($"Information {other[i]} is {other[i+1]}");
                myfile.WriteLine($"Time now is:  { DateTime.Now.ToString()}It took  {usedTime} second to finish ");
                //could use a loop to read all data from the array
                /*
                 foreach (var ValeD in other)
                 {
                     myfile.WriteLine($"Information {name} is {ValeD}");
                     informationTable.Add(infoCount, ValeD);
                     infoCount++;
                 }
                */
                break;
            case InputType.Debug:
                foreach (var ValeD in other)
                {
                   // if (ValeD != "null")
                    myfile.WriteLine($"Debug type {name} is: {ValeD}");
                }
                break;
            case InputType.Media:
                
                foreach (var ValeD in other)
                {
                    myfile.WriteLine($"\"Media Type\": \"{name}\" is \"{ValeD}\"");

                }
                myfile.WriteLine($"It took  {usedTime} second to finish ");//This could be removed or formated differently
                break;
            case InputType.MCQ:
                 myfile.WriteLine($"MC_{name}: \"{other[0]}\"");
                 myfile.WriteLine($"It took  {usedTime} second to finish ");//This could be removed or formated differently
                break;
            case InputType.TFQ:
                myfile.WriteLine($"TF_{name}: \"{other[0]}\"");
                myfile.WriteLine($"It took  {usedTime} second to finish ");
                break;

            case InputType.SAQ:
                    myfile.WriteLine($"SA_{name}:\"{other[0]}\"");
                    myfile.WriteLine($"It took  {usedTime} second to finish ");
                break;

            case InputType.OrderQ:
                myfile.Write($"OQ_{name}:\"");
                foreach (var ValeD in other)
                {
                    //  if(ValeD != "null")
                    myfile.Write($" {ValeD}");

                }
                myfile.WriteLine("\"");
                myfile.WriteLine($"It took  {usedTime} second to finish ");
                break;
        }
        myfile.Close();
    }

    public void SubmitLog(Action onDoneUploading)
    {
        StartCoroutine(Upload(onDoneUploading));
        InfoLog(this.GetType().ToString(), "Trace",
            $"Application ending after {Time.time - initTime} seconds, " +
            $"time is now {DateTime.Now.ToString("HH:mm")}");
        Debug.Log($"Application ending after {Time.time - initTime} seconds, " +
            $"time is now {DateTime.Now.ToString("HH:mm")}");
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
    /// Writes an array of strings to the log one line at a time
    /// </summary>
    /// <param name="lines">Array of lines written to the log file</param>
    private void appendToLog(string[] lines)
    {
        StreamWriter myfile = File.AppendText(LogPath);
        foreach(string line in lines)
        {
            myfile.WriteLine(line);
        }
        myfile.Close();
    }

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
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }

        doneUploading.Invoke();
    }
    #endregion Private Methods
}
