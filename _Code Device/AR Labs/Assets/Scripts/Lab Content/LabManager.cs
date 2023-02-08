using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MagicLeapTools;
using UnityEngine.XR.MagicLeap;
using System;

public class LabManager : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private string[] modules;                  // List of lab module data
    private int index = 0;                     // index of currently running module in list
    private int indexIncrement = 1;            // How to move to next module
    private GameObject currentModuleObject;    // Reference to object holding the module script
    private ActivityModule currentModuleScript;// Reference to current module script

    private DateTime transmissionStart;
    private bool transmissionHost;
    private Bridge bridge;
    private LabLogger logger; // Easy reference to the logger object
    private string entity;    // String of this class name, used when logging
    #endregion Variables

    #region Unity Methods
    public void Start()
    {
        // Setup for logging
        entity = this.GetType().ToString();
        logger = LabLogger.Instance;
        // Start up the Bridge for the lab
        bridge = Bridge.Instance;
    }
    #endregion Unity Methods

    #region Public Methods
    public void Initialize(LabDataObject data)
    {
        logger.InfoLog(entity, "Trace", "Initialize()");

        // Initialize data
        modules = data.ActivityModules;

        // Check if Transmission
        if(data.Transmission)
        {
            // Enable components
            GetComponent<MLPrivilegeRequesterBehavior>().enabled = true;
            GetComponent<Transmission>().enabled = true;
            GetComponent<SpatialAlignment>().enabled = true;
            bridge.ConnectToTransmission();

            // Assume we are the host for now
            transmissionHost = true;
            // Listen if we find a peer that is older
            Transmission.Instance.OnOldestPeerUpdated.AddListener(handleOldestPeerUpdated);
        }

        // Start the lab
        SpawnModule();
    }
    #endregion Public Methods

    #region Private Methods
    private void SpawnModule()
    {
        logger.InfoLog(entity, "State Start", $"Module {index}");
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

        //There will need to be some sort of placement routine, but for now it will be placed at 0,0,0
        currentModuleObject = Instantiate(tmpPrefab, transform);
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

        if (Transmission.Instance.enabled)
        {
            // Disable Transmission tools
            GetComponent<SpatialAlignment>().enabled = false;
            Transmission.Instance.enabled = false;
            GetComponent<MLPrivilegeRequesterBehavior>().enabled = false;
            bridge.DisconnectFromTransmission();
        }

        // Notify the login manager that the lab is completed
        GameObject.Find("[LOGIC]").GetComponent<LoginManager>().LabComplete();
    }
    #endregion Private Methods

    #region Event Handlers
    /// <summary>
    /// Listener to Transmission OnOldestPeerUpdated() event, receives a string
    /// address of the new oldest peer. Your address can be found using 
    /// MagicLeapTools.NetworkUtilities.MyAddress
    /// </summary>
    /// <param name="peerAddress"></param>
    private void handleOldestPeerUpdated(string peerAddress)
    {
        if (peerAddress != NetworkUtilities.MyAddress)
        {
            transmissionHost = false;
        }

    }
    public void ModuleComplete()
    {
        index = index + indexIncrement;
        if (index < 0)
            index = 0;

        logger.InfoLog(entity, "Trace", "ModuleComplete()");

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
