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

        string url = "http://cyberlearnar.cs.mtsu.edu/get_file/scene/scene-example.json";
        string ss = "{\"urlJson\": \"" + url + "\"} ";



        Debug.Log(ss);
        currentModuleObject = Instantiate(demoPrefab2, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);  //, rootUITransform);
        currentModuleScript = currentModuleObject.GetComponent<ActivityModule>();


        ss = "{\"moduleName\":\"duck\",\"specificName\":\"quacky\",\"prefabName\":\"demoPrefab\",\"prerequisiteActivities\":[],\"educationalObjectives\":[\"show that ducks are superiour\",\"quack a joke frequently\", \"promote duckyness everywhere\", \"duck the responsiblity of bad grades\"],\"instructions\":[\"click on something... anything!\"],\"numRepeatsAllowed\":0,\"numGradableRepeatsAllowed\":0,\"gradingCriteria\":\"\",\"currentScore\":0.0,\"bestScore\":0.0,\"completed\":false,\"currentSubphase\":0,\"subphaseNames\":[],\"demoJson\":\"\",\"urlJson\":\"http://cyberlearnar.cs.mtsu.edu/get_file/scene/scene-example.json\"}";
        ActivityModuleData tmpData = new ActivityModuleData();
        Debug.Log("ssss  = " + ss);
        JsonUtility.FromJsonOverwrite(ss, tmpData);
        Debug.Log(tmpData.moduleName);

        string eobj = "Educational Objectives: \n\n";
        string s;
        for (int i = 0; i < tmpData.educationalObjectives.Length; i++ )
        {
            s = tmpData.educationalObjectives[i];
            eobj = eobj  + i.ToString() + ") " + s + "\n";
        }
        instructionCanvas.GetComponent<Text>().text = eobj;

        
        currentModuleScript.Initialize(ss);
    }


    public void spawnDemo()
    {
        //demoObject = Instantiate(demoPrefab, Vector3.zero, Quaternion.identity);  //, rootUITransform);


        //ActivityModuleData tmpData = new ActivityModuleData();
        //JsonUtility.FromJsonOverwrite(modules[index], tmpData);
        //Load prefab from resources
        //GameObject tmpPrefab = (GameObject)Resources.Load($"Prefabs/{tmpData.prefabName}");
        //There will need to be some sort of placement routine, but for now it will be placed at 0,0,0
        //currentModuleObject = Instantiate(tmpPrefab, Vector3.zero, Quaternion.identity);
        //currentModuleScript = currentModuleObject.GetComponent<ActivityModule>();

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
