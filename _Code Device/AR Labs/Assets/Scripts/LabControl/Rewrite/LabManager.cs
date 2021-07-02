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
    private GameObject currentModuleObject;
    private ActivityModule currentModuleScript;



    [SerializeField]
    private GameObject demoPrefab = null;
    [SerializeField]
    private GameObject demoPrefab2 = null;
    private GameObject demoObject = null;

    [SerializeField]
    GameObject instructionPrefab;
    private GameObject instructionHolder;
    private GameObject instructionCanvas;
    

    public void Start()
    {
        createInstructions();
        spawnDemoNew();
    }


    public void spawnDemoNew()
    {

        instructionCanvas.GetComponent<Text>().text = "Now is the time for all good men";

        currentModuleObject = Instantiate(demoPrefab2, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);  //, rootUITransform);
        currentModuleScript = currentModuleObject.GetComponent<ActivityModule>();


        string jsonpath = "C:/Users/jfwal/OneDrive/Documents/GitHub/cyberAR/_Code Device/AR Labs/Assets/Resources/jsonDefinitions/";
        string fname = "demo10.json";
        string ss = "";

        string fpath = jsonpath + fname;


        //if (File.Exists(fpath))
        //{

        string[] lines = System.IO.File.ReadAllLines(fpath);

        ss = lines[0];
        Debug.Log("lines length = " + lines.Length.ToString());
        for (int i = 0; i < lines.Length; i++)
            Debug.Log(i.ToString() + "  : " + lines[i]);


        ActivityModuleData tmpData = new ActivityModuleData();
        JsonUtility.FromJsonOverwrite(ss, tmpData);

        string eobj = "Educational Objectives: \n\n";
        string s;
        for (int i = 0; i < tmpData.educationalObjectives.Length; i++)
        {
            s = tmpData.educationalObjectives[i];
            eobj = eobj + i.ToString() + ") " + s + "\n";
        }
        instructionCanvas.GetComponent<Text>().text = eobj;

        spawnDemo(ss);
    }


    public void spawnDemo(string ssss)
    {


        ActivityModuleData tmpData = new ActivityModuleData();
        //JsonUtility.FromJsonOverwrite(modules[index], tmpData);
        //Load prefab from resources
//        GameObject tmpPrefab = (GameObject)Resources.Load($"Prefabs/{tmpData.prefabName}");
        //There will need to be some sort of placement routine, but for now it will be placed at 0,0,0
        //currentModuleObject = Instantiate(tmpPrefab, Vector3.zero, Quaternion.identity);
        currentModuleObject = Instantiate(demoPrefab, Vector3.zero, Quaternion.identity);
        currentModuleScript = currentModuleObject.GetComponent<ActivityModule>();
        //currentModuleScript.Initialize(modules[index]);
        Debug.Log(">>>>>>>>>  " + ssss);
        currentModuleScript.Initialize(ssss);

    }

    public void demoCompleted()
    {
        Destroy(demoObject);
    }

    void createInstructions()
    {
        instructionHolder = Instantiate(instructionPrefab, new Vector3(-0.3f, 1.5f, 1.0f), Quaternion.Euler(0.0f, 180.0f, 0.0f));
        //instructionHolder = Instantiate(instructionPrefab, new Vector3(0.3f, -0.2f, -1.0f), Quaternion.Euler(0.0f, 180.0f, 0.0f));
        instructionCanvas = GameObject.Find("MainInstructions");
        instructionCanvas.GetComponent<Text>().text = "Test Text";
    }



// --------------------------------------------------------


    public void Initialize(string[] moduleData)
    {
        //Initialize data
        modules = moduleData;

        //Start Lab
//        SpawnModule();
    }





 /*   private void SpawnModule()
    {
        //Deserialize JSON to get the prefab name
        ActivityModuleData tmpData = new ActivityModuleData();
        JsonUtility.FromJsonOverwrite(modules[index], tmpData);
        //Load prefab from resources
        GameObject tmpPrefab = (GameObject)Resources.Load($"Prefabs/{tmpData.prefabName}");
        //There will need to be some sort of placement routine, but for now it will be placed at 0,0,0
        currentModuleObject = Instantiate(tmpPrefab, Vector3.zero, Quaternion.identity);
        currentModuleScript = currentModuleObject.GetComponent<ActivityModule>();
        //Start the module
        currentModuleScript.Initialize(modules[index]);
    }
 */
    public void ModuleComplete()
    {
        Destroy(currentModuleObject);
        index++;
        if (index < modules.Length)
        {
   //         SpawnModule();
        }
        else
        {
            EndLab();
        }
    }

    private void EndLab()
    {
        ;
    }
}
