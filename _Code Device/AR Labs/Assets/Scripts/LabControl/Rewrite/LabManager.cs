using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    public void Start()
    {
        spawnDemo();
    }
    public void Initialize(string[] moduleData)
    {
        //Initialize data
        modules = moduleData;

        //Start Lab
        SpawnModule();
    }


    public void spawnDemo()
    {
        demoObject = Instantiate(demoPrefab, Vector3.zero, Quaternion.identity);  //, rootUITransform);
    }

    public void demoCompleted()
    {
        Destroy(demoObject);
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
