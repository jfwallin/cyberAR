using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class LabManager : MonoBehaviour
{
    [SerializeField]
    private string[] modules;
    private int index = 0;
    private GameObject currentModuleObject;
    private ActivityModule currentModuleScript;



    [SerializeField]
    private GameObject demoPrefab = null;
    private GameObject demoObject = null;

    [SerializeField]
    GameObject instructionPrefab;
    private GameObject instructionHolder;
    private GameObject instructionCanvas;
    

    public void Start()
    {
        spawnDemo();
    }

    public void spawnDemo()
    {
        //demoObject = Instantiate(demoPrefab, Vector3.zero, Quaternion.identity);  //, rootUITransform);
        createInstructions();
        demoObject = Instantiate(demoPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);  //, rootUITransform);
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
        SpawnModule();
    }





    private void SpawnModule()
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

    public void ModuleComplete()
    {
        Destroy(currentModuleObject);
        index++;
        if (index < modules.Length)
        {
            SpawnModule();
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
