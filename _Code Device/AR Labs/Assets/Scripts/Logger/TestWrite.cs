using System.Collections;
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

    //Using time stamps to track how long it take to perform each step
    public float startTime = 0.0f;
    public float currentTime;
    public float usedTime;
    public float lastTimeInterval = 0.0f;
    //This will be updated once the student logs in to cointain the student's id
    public string Path = "Assets/Resources/";
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
    #endregion Unity Methods

    #region Public Methods
    /// <summary>
    /// Sets the filepath using the passed user ID, prints a first line
    /// </summary>
    /// <param name="id">The id entered by the user</param>
    public void InitializeLog(string id)
    {
        Path = Path + id + "_" + System.DateTime.Now + ".txt";
        appendToLog($"\n\nLog file for student M{id}.\nCurrent Time: {System.DateTime.Now}\n");
    }

    public void labSelected(string labName)
    {
        appendToLog($"The lab selected is: {labName}.");
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
        StreamWriter myfile = File.AppendText(Path);
        //Get the input type and teh current time            
        InputType TYPE = (InputType)type;
        currentTime = Time.realtimeSinceStartup;
        usedTime = currentTime -lastTimeInterval ;
        lastTimeInterval = currentTime;
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
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Writes a line to the log file
    /// </summary>
    /// <param name="line">Line to be appended to the log</param>
    private void appendToLog(string line)
    {
        StreamWriter myfile = File.AppendText(Path);
        myfile.WriteLine(line);
        myfile.Close();
    }

    /// <summary>
    /// Writes an array of strings to the log one line at a time. 
    /// </summary>
    /// <param name="lines">Array of lines written to the log file</param>
    private void appendToLog(string[] lines)
    {
        StreamWriter myfile = File.AppendText(Path);
        foreach(string line in lines)
        {
            myfile.WriteLine(line);
        }
        myfile.Close();
    }

    // Make connection to the server by providing email and password
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
    #endregion Private Methods
}
