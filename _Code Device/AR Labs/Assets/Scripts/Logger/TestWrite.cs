using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public enum InputType { info, Media, Debug, MCQ, TFQ, SAQ, OrderQ}
public class TestWrite : MonoBehaviour
{
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


    //Counter to use as hash key
    public int flag = 1; // This flag used to start th connection to the server
    public static int infoCount = 1; 
    public static int mediaCount = 1;
    public static int debugCount = 1;


    //Using time stamps to track how long it take to perform each step
    public float startTime = 0.0f;
    public float currentTime;
    public float usedTime;
    public float lastTimeInterval = 0.0f;
    public string Path = "Assets/Resources/test2.txt";// this need to be change to be "Assets/Resources/" + varaible name such as id or student name

    public void Awake()
    {
        //Check if there is already another instance, destroy self if that is the case
        if ((_instance != null && _instance != this))
        {
            Destroy(this);
        }
        else //Only instance
        {
            _instance = this;
        }
    }

    public void start()
    {
        WriteToString( 0, "null");
        StartCoroutine(Connect());
       // time stamp to the log file 
        //print("Application make connect at " + Time.time + " seconds and time is" + DateTime.Now.ToString());
    }

    // Main write information menthod
    //Take at least two input up to as many .... input
    // first input is a enum InputType { info, Media, Debug, MC, TF ...}
    // info for inprmation such as login information
    // Medai for media inofrmation
    // Debug for debug
    //Second input depend on the InputType
    //Third input is any optional string values 
    public void WriteToString(InputType type, string name, params string[] other )
    {
        // for the first this called a conncetion method to the server will be called
        if (flag == 1)
           {
            StartCoroutine(Connect());
           // print("Application make connect at " + Time.time + " seconds and time is" + DateTime.Now.ToString());
            flag = 0;
           }
        // string Path = "Assets/Resources/test2.txt";
        //  if (type == InputType.info)
        //    Path = "Assets/Resources/TXT/" + other[0] + other[1] + ".txt";
        // StreamWriter writer = new StreamWriter(Path, true);

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

   

  // Make connection to the server by providing email and password
IEnumerator Connect()
{
    print($"Connect called at { DateTime.Now.ToString()} ");
  
    //create a webForm object
    WWWForm form = new WWWForm();
    ///
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

}
