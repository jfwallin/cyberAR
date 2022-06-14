using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.IO;

public class LabManager : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private string[] modules;
    private int index = 0;
    private int indexIncrement = 1;
    private GameObject currentModuleObject;
    private ActivityModule currentModuleScript;

    [SerializeField]
    GameObject instructionPrefab;
    private GameObject instructionHolder;
    private GameObject instructionCanvas;

    //private InstructionBox ibox;
    private Transform labform;

    private LabLogger logger;
    private string entity;
    #endregion Variables

    #region Unity Methods
    public void Start()
    {
        entity = this.GetType().ToString();
        logger = LabLogger.Instance;


        //ibox = InstructionBox.Instance;
        labform = GameObject.Find("[CURRENT_LAB]").transform;
        //spawnDemoNew();
    }
    #endregion Unity Methods

    #region Public Methods
    public void Initialize(LabDataObject data)
    {
        logger.InfoLog(entity, "Trace", "Initializing lab");
        modules = data.ActivityModules;

        SpawnModule();
        //spawnDemoNew();
    }

    public void Initialize(string[] moduleData)
    {
        logger.InfoLog(entity, "Trace", "Initializing lab");
        // this creates the instruction canvas
        //createInstructions();

        //Initialize data
        modules = moduleData;

        //Start Lab
        SpawnModule();
    }

    public void spawnDemoNew()
    {

        // this is a temporary way to load a json file
        //string jsonpath = "C:/Users/jfwal/OneDrive/Documents/GitHub/cyberAR/_Code Device/AR Labs/Assets/Resources/jsonDefinitions/";
        //string jsonpath = "Assets/Resources/jsonDefinitions/";
        //string fname = "demo10.json";
        //string fpath = jsonpath + fname;
        //modules = System.IO.File.ReadAllLines(fpath);


        string jdata = Resources.Load<TextAsset>("jsonDefinitions/demo10").text;
        string[] modulesList = jdata.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        // this is a kludge to problems associated with end of the file issues
        // and random linefeeds within list of json files.  The splitting routine
        // often adds a zero size module at the end of the file.  I will copy
        // over any modules with length > 10 (a somewhat arbitrary choice)
        // into the final array of modules.  This problem doesn't occur when you
        // use the ReadAllLines option, but this is not available if you 
        // are using a string array from a downloaded file or a Resource load.
        int nonzero = 0;
        for (int i = 0; i < modulesList.Length; i++)
            if (modulesList[i].Length > 10)
                nonzero = nonzero + 1;

        int j = 0;
        string[] modules = new string[nonzero];
        for (int i = 0; i < modulesList.Length; i++)
        {
            if (modulesList[i].Length > 10)
            {
                modules[j] = modulesList[i];
                j = j + 1;
            }
        }

        Initialize(modules);
        SpawnModule();   
    }
    #endregion Public Methods

    #region Private Methods
    private void SpawnModule()
    {
        //Debug.Log("spawning module #" + index.ToString());

        //Deserialize JSON to get the prefab name
        ActivityModuleData tmpData = new ActivityModuleData();
        print($"Json Labmodule: {modules[index]}");
        JsonUtility.FromJsonOverwrite(modules[index], tmpData);

        // update the student instructions, objectives, and nav screen
        //updateInstructions(tmpData);

        print($"Prefab for module filepath: Prefabs/{tmpData.prefabName}");
        //Load prefab from resources
        GameObject tmpPrefab = (GameObject)Resources.Load($"Prefabs/{tmpData.prefabName}");
        print($"tmpPrefab for module is null: {tmpPrefab == null}");
     //   if (tmpPrefab == null)
     //       Debug.Log("it is null!");
     //   else
     //       Debug.Log("tmpdata = " + tmpData.prefabName);

        //There will need to be some sort of placement routine, but for now it will be placed at 0,0,0
        currentModuleObject = Instantiate(tmpPrefab, labform);
        currentModuleScript = currentModuleObject.GetComponent<ActivityModule>();

        //Start the module
        currentModuleScript.Initialize(modules[index]);
    }
    /// <summary>
    /// Called when the lab manager identifies that the lab is out of modules.
    /// </summary>
    private void EndLab()
    {
        // Get rid of current module objects and references
        Destroy(currentModuleObject);
        currentModuleScript = null;
        // Notify the login manager that the lab is completed
        GameObject.Find("[LOGIC]").GetComponent<LoginManager>().LabComplete();
    }
    void createInstructions()
    {
        //GameObject root = GameObject.Find("Dynamic");
        instructionHolder = Instantiate(instructionPrefab, new Vector3(0.0f, -0.3f, 2.0f), Quaternion.Euler(0.0f, 180.0f, 0.0f));
        instructionCanvas = GameObject.Find("MainInstructions");
        instructionCanvas.GetComponent<Text>().text = "Test Text";


        FindObjectOfType<MagicLeapTools.ControlInput>().OnDoubleBumper.AddListener(InstructionBox.Instance.HandleDoubleBumper); //responds to double bumper to appear and disappear


        //FindObjectOfType<MagicLeapTools.ControlInput>().OnDoubleBumper.AddListener(InstructionBox.Instance.HandleDoubleBumper); //responds to double bumper to appear and disappear
        //FindObjectOfType<MagicLeapTools.ControlInput>().OnDoubleBumper.AddListener(InstructionBox.Instance.HandleDoubleBumper); //responds to double bumper to appear and disappear

        //ibox.GetComponent<MagicLeapTools.InputReceiver>(). OnSelected.RemoveAllListeners();
        //InstructionBox.Instance.transform.position = new Vector3(-0.3f, 1.5f, 1.0f); //Creates instance, places it. by default it points at the user
        //FindObjectOfType<MagicLeapTools.ControlInput>().OnDoubleBumper.AddListener(InstructionBox.Instance.HandleDoubleBumper); //responds to double bumper to appear and disappear
    }

    void updateInstructions(ActivityModuleData tmpData)
    {
        // case the current data activity module data into a local variable
        //ActivityModuleData tmpData = new ActivityModuleData();
        //JsonUtility.FromJsonOverwrite(modules[index], tmpData);

        string eobj = "Educational Objectives: \n\n";
        string s;
        for (int i = 0; i < tmpData.educationalObjectives.Length; i++)
        {
            s = tmpData.educationalObjectives[i];
            eobj = eobj + i.ToString() + ") " + s + "\n";
        }
        //instructionCanvas.GetComponent<Text>().text = eobj;

        InstructionBox.Instance.AddPage("Objectives", eobj, true); //Creates tab for the educational objectives and shows it.

    }
    #endregion Private Methods

    #region Event Handlers
    public void ModuleComplete()
    {
        index = index + indexIncrement;
        if (index < 0)
            index = 0;

        logger.InfoLog(entity, "Trace", $"Moving to module at index {index}");

        if (index < modules.Length)
        {
            StartCoroutine(NewModule());
        }
        else
        {
            Destroy(currentModuleObject);
            EndLab();
        }
    }

    public void nextModuleCallback()
    {
        indexIncrement = 1;
        ModuleComplete();
    }

    public void previousModuleCallback()
    {
        indexIncrement = -1;
        ModuleComplete();
    }

    public void endLabCallback()
    {
        index = modules.Length - 1;
        ModuleComplete();
    }
    #endregion Event Handlers

    #region Coroutines
    IEnumerator NewModule()
    {
        yield return new WaitForSeconds(1.0f);
        Destroy(currentModuleObject);
        SpawnModule();

    }
    #endregion Coroutines

}
