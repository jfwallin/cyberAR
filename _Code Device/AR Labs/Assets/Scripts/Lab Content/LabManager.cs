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
    [Header("Transmission Wait UI")]
    [SerializeField]
    private GameObject transmissionWaitUI;
    [SerializeField]
    private Button transmissionStartLabButton;
    [SerializeField]
    private Text peerCountText;
    [Header("Lab Data")]
    [SerializeField]
    private string[] modules;                  // List of lab module data
    private int index = 0;                     // index of currently running module in list
    private int indexIncrement = 1;            // How to move to next module
    private GameObject currentModuleObject;    // Reference to object holding the module script
    private ActivityModule currentModuleScript;// Reference to current module script

    private bool transmissionLab = false;
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
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "Initialize()");

        // Initialize data
        modules = data.ActivityModules;

        // Check if Transmission
        if (data.Transmission)
        {
            transmissionLab = true;
            transmissionHost = false;
            // Enable components
            GetComponent<MLPrivilegeRequesterBehavior>().enabled = true;
            try
            {
                GetComponent<Transmission>().enabled = true;
                GetComponent<SpatialAlignment>().enabled = true;
            }
            catch (Exception ex)
            {
                LabLogger.Instance.InfoLog(
                    this.ToString(),
                    LabLogger.LogTag.ERROR,
                    ex.ToString()
                );
            }
            // Sets up Transmission to listen for general lab messages from peers
            bridge.ConnectToTransmission();

            // Start waiting for people to connect, setup and enable UI
            transmissionWaitUI.SetActive(true);
            // Initialize UI to wait for peers
            peerCountText.text = "0";
            transmissionStartLabButton.onClick.RemoveAllListeners();
            transmissionStartLabButton.interactable = false;
            transmissionStartLabButton.GetComponentInChildren<Text>().text = "Waiting for Peers";

            // Check if we have already conencted to an older peer
            handleOldestPeerUpdated(Transmission.Instance.OldestPeer);
            // Listen if we find a peer that is older
            Transmission.Instance.OnOldestPeerUpdated.AddListener(handleOldestPeerUpdated);
            // Track number of peers
            peerCountText.text = Transmission.Instance.Peers.Length.ToString();
            Transmission.Instance.OnPeerFound.AddListener((string ip, long time) => changeNumPeers(1));
            Transmission.Instance.OnPeerLost.AddListener((string ip) => changeNumPeers(-1));
        }
        else
        {
            transmissionLab = false;
            // Start the lab
            SpawnModule();
        }
    }

    /// <summary>
    /// Called via Transmission RPC call, so all peers start the lab at the same time.
    /// </summary>
    public void TransmissionStartLab()
    {
        LabLogger.Instance.InfoLog(entity, LabLogger.LogTag.TRACE, "TransmissionStartLab()");
        // Disconnect and close UI
        transmissionStartLabButton.onClick.RemoveAllListeners();
        Transmission.Instance.OnPeerFound.RemoveAllListeners();
        Transmission.Instance.OnPeerLost.RemoveAllListeners();
        transmissionWaitUI.SetActive(false);
        // Start the lab
        SpawnModule();
    }
    #endregion Public Methods

    #region Private Methods
    private void SpawnModule()
    {
        logger.InfoLog(entity, LabLogger.LogTag.STATE_START, $"Module {index}");
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

        // If this is a transmission lab, then add the activity as a rpc target
        if(transmissionLab)
        {
            List<GameObject> targets = new List<GameObject>(Transmission.Instance.rpcTargets);
            targets.Add(gameObject);
            Transmission.Instance.rpcTargets = targets.ToArray();
        }

        //Start the module
        currentModuleScript.TransmissionHost = transmissionHost;
        currentModuleScript.TransmissionActivity = transmissionLab;
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
        LabLogger.Instance.InfoLog(entity, LabLogger.LogTag.TRACE, $"handleOldestPeerUpdated({peerAddress})");
        if (String.IsNullOrEmpty(peerAddress))
        {
            transmissionStartLabButton.onClick.RemoveAllListeners();
            transmissionStartLabButton.interactable = false;
            transmissionStartLabButton.GetComponentInChildren<Text>().text = "Wait for Host";
        }
        if (peerAddress != NetworkUtilities.MyAddress)
        {
            transmissionHost = false;
            // Disable the button
            transmissionStartLabButton.onClick.RemoveAllListeners();
            transmissionStartLabButton.interactable = false;
            transmissionStartLabButton.GetComponentInChildren<Text>().text = "Wait for Host";
        }
        else
        {
            transmissionHost = true;
            // Enable the button
            transmissionStartLabButton.onClick.AddListener(handleStartLabButton);
            transmissionStartLabButton.interactable = true;
            transmissionStartLabButton.GetComponentInChildren<Text>().text = "Start Lab";
        }
    }

    private void handleStartLabButton()
    {
        LabLogger.Instance.InfoLog(entity, LabLogger.LogTag.TRACE, "handleStartLabButton()");
        if (transmissionHost)
        {
            // Start the lab by sending message to all peers
            // (Including self) to call this method.
            // Transmission is on this same gameobject, so the message should reach here.
            Transmission.Send(new RPCMessage("TransmissionStartLab"));
        }
    }

    private void changeNumPeers(int chg)
    {
        LabLogger.Instance.InfoLog(entity, LabLogger.LogTag.TRACE, $"changeNumPeers({chg})");
        peerCountText.text = (int.Parse(peerCountText.text) + chg).ToString();
    }

    public void ModuleComplete()
    {
        index = index + indexIncrement;
        if (index < 0)
            index = 0;

        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "ModuleComplete()");

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
