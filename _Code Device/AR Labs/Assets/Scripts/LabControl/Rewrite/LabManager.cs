using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class LabManager : MonoBehaviour
{
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

    public void Start()
    {
        // ultimately - we might use Initialize externally 
        // For now - we will manually read in the json.
        // create instances of media player prefab here


        //ibox = InstructionBox.Instance;
        labform = GameObject.Find("[_DYNAMIC]").transform;
        //spawnDemoNew();
    }

    public void Initialize(LabDataObject data)
    {
        modules = data.ActivityModules;

        //SpawnModule();
        spawnDemoNew();

    }


    public void spawnDemoNew()
    {

        // this is a temporary way to load a json file
        //string jsonpath = "C:/Users/jfwal/OneDrive/Documents/GitHub/cyberAR/_Code Device/AR Labs/Assets/Resources/jsonDefinitions/";
        string jsonpath = "Assets/Resources/jsonDefinitions/";
        string fname = "demo10.json";
        string fpath = jsonpath + fname;
        modules = System.IO.File.ReadAllLines(fpath);
        Debug.Log("modules loaded = " + modules.Length.ToString());

        Initialize(modules);
        SpawnModule();   
    }

// --------------------------------------------------------
    public void Initialize(string[] moduleData)
    {
        // this creates the instruction canvas
        //createInstructions();

        //Initialize data
        modules = moduleData;

        //Start Lab
        SpawnModule();
    }

    private void SpawnModule()
    {
        Debug.Log("spawning module #" + index.ToString());

        //Deserialize JSON to get the prefab name
        ActivityModuleData tmpData = new ActivityModuleData();
        JsonUtility.FromJsonOverwrite(modules[index], tmpData);

        // update the student instructions, objectives, and nav screen
        //updateInstructions(tmpData);

        //Load prefab from resources
        GameObject tmpPrefab = (GameObject)Resources.Load($"Prefabs/{tmpData.prefabName}");

        if (tmpPrefab == null)
            Debug.Log("it is null!");
        else
            Debug.Log("tmpdata = " + tmpData.prefabName);

        //There will need to be some sort of placement routine, but for now it will be placed at 0,0,0
        currentModuleObject = Instantiate(tmpPrefab, labform);
        currentModuleScript = currentModuleObject.GetComponent<ActivityModule>();

        //Start the module
        currentModuleScript.Initialize(modules[index]);
    }
 
    public void ModuleComplete()
    {
        index = index + indexIncrement;
        if (index < 0)
            index = 0;

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
        Debug.Log("next Module ---------------------------------------------------------------");
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
        Debug.Log("endLab callback");
            //Destroy(currentModuleObject);
            //EndLab();
    }
    IEnumerator NewModule()
    {
        yield return new WaitForSeconds(1.0f);
        Destroy(currentModuleObject);
        SpawnModule();

    }

    private void EndLab()
    {
        // TODO SEND MESSAGE TO LOGIN LOGIC SCRIPT
        print("Returning to Lab Selection.");
        // MODIFIDED FOR TESTING jw
        //GameObject.Find("[LOGIC]").GetComponent<loginLogic>().gotoState(4);
        Application.Quit();
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

    //Should module specific changes to the instruction box be done in the module code?
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
}
