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

            // Start waiting for people to connect, setup and enable UI
            transmissionWaitUI.transform.SetPositionAndRotation(Camera.main.transform.position + Camera.main.transform.forward * 2, Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up));
            transmissionWaitUI.SetActive(true);
            setTransmissionUI(TransmissionWaitStatus.Wait);

            // Sets up Transmission to listen for general lab messages from peers
            bridge.ConnectToTransmission();
            LabLogger.Instance.InfoLog(entity, LabLogger.LogTag.DEBUG, $"Our Address: {NetworkUtilities.MyAddress}");

            // Check if we already have peers  NOT SURE IF WE NEED THIS ANYMORE
            StartCoroutine("CheckForPeers");

            // Track changes to number of peers
            Transmission.Instance.OnPeerFound.AddListener((string ip, long time) => changeNumPeers(1));
            Transmission.Instance.OnPeerLost.AddListener((string ip) => changeNumPeers(-1));

            // Listen if we find a peer that is older
            Transmission.Instance.OnOldestPeerUpdated.AddListener(handleOldestPeerUpdated);

            // Set Shared origin
            Pose newOrigin = new Pose(transform.position, transform.rotation);
            Transmission.Instance.sharedOrigin = newOrigin;
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
        Transmission.Instance.OnOldestPeerUpdated.RemoveAllListeners();
        transmissionWaitUI.SetActive(false);
        // Start the lab
        SpawnModule();
    }
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Identifies if we have found peers and what our relationship to them is
    /// </summary>
    private enum TransmissionWaitStatus { Wait, Host, Peer };

    /// <summary>
    /// Sets button and text on the Transmission connection UI
    /// </summary>
    /// <param name="status"></param>
    private void setTransmissionUI(TransmissionWaitStatus status)
    {
        LabLogger.Instance.InfoLog(entity, LabLogger.LogTag.TRACE, $"setTransmissionUI({status})");
        switch(status)
        {
            case TransmissionWaitStatus.Wait:
                peerCountText.text = "0";
                transmissionStartLabButton.onClick.RemoveAllListeners();
                transmissionStartLabButton.interactable = false;
                transmissionStartLabButton.GetComponentInChildren<Text>().text = "Wait for Peers";
                break;
            case TransmissionWaitStatus.Host:
                transmissionStartLabButton.onClick.AddListener(handleStartLabButton);
                transmissionStartLabButton.interactable = true;
                transmissionStartLabButton.GetComponentInChildren<Text>().text = "Start Lab";
                break;
            case TransmissionWaitStatus.Peer:
                transmissionStartLabButton.onClick.RemoveAllListeners();
                transmissionStartLabButton.interactable = false;
                transmissionStartLabButton.GetComponentInChildren<Text>().text = "Wait for Host";
                break;
        }
    }

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
        //if(transmissionLab)
        //{
        //    List<GameObject> targets = new List<GameObject>(Transmission.Instance.rpcTargets);
        //    targets.Add(gameObject);
        //    Transmission.Instance.rpcTargets = targets.ToArray();
        //}

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
    public void AlignmentOnLocalized()
    {
        LabLogger.Instance.InfoLog(entity, LabLogger.LogTag.EVENT, "SPATIAL ALIGNMENT LOCALIZED");
    }

    /// <summary>
    /// Listener to Transmission OnOldestPeerUpdated() event, receives a string
    /// address of the new oldest peer. Your address can be found using 
    /// MagicLeapTools.NetworkUtilities.MyAddress
    /// </summary>
    /// <param name="peerAddress"></param>
    private void handleOldestPeerUpdated(string peerAddress)
    {
        LabLogger.Instance.InfoLog(entity, LabLogger.LogTag.TRACE, $"handleOldestPeerUpdated({peerAddress})");
        if (peerAddress != NetworkUtilities.MyAddress)
        {
            transmissionHost = false;
            setTransmissionUI(TransmissionWaitStatus.Peer);
        }
        else
        {
            transmissionHost = true;
            setTransmissionUI(TransmissionWaitStatus.Host);
        }
    }

    private void handleStartLabButton()
    {
        LabLogger.Instance.InfoLog(entity, LabLogger.LogTag.TRACE, "handleStartLabButton()");
        if (transmissionHost)
        {
            // Start the lab by sending message to all known peers
            // Since this does not include ourselves, we must also call it explicitly here.
            Transmission.Send(new RPCMessage("TransmissionStartLab", "", "", TransmissionAudience.KnownPeers));
            TransmissionStartLab();
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

    IEnumerator CheckForPeers()
    {
        yield return new WaitForSeconds(1.0f);
        int initNumPeers = Transmission.Instance.Peers.Length;
        LabLogger.Instance.InfoLog(entity, LabLogger.LogTag.DEBUG, $"Delayed Checking for peers, found {initNumPeers}");
        if (initNumPeers > 0)
        {
            peerCountText.text = initNumPeers.ToString();
            if (Transmission.Instance.OldestPeer != NetworkUtilities.MyAddress) // Not sure if this works
            {
                transmissionHost = false;
                setTransmissionUI(TransmissionWaitStatus.Peer);
            }
            else // We are the oldest
            {
                transmissionHost = true;
                setTransmissionUI(TransmissionWaitStatus.Host);
            }
        }
        var started = MLPersistentCoordinateFrames.IsStarted;
        var pcf_localized = MLPersistentCoordinateFrames.IsLocalized;
        var spatial_localized = SpatialAlignment.Localized;
        LabLogger.Instance.InfoLog(entity, LabLogger.LogTag.EVENT, $"pcf started: ${started}, pcf localized : ${pcf_localized}, spatial_localized : ${spatial_localized}");
    }
    #endregion Coroutines
}
