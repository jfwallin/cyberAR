using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using MagicLeapTools;
using MagicLeap.Core;
using UnityEngine.XR.MagicLeap;

public struct LabInfo
{
    public string id;
    public string name;
    public string description;

    // Test comment
    public override string ToString()
    {
        return $"lab id: {id}\n  name: {name}\n  desc: {description}";
    }
}

public class LoginManager : MonoBehaviour
{
    #region Variables
    // Public Variables
    [Header("Animation / Placement")]
    public GameObject controller;      //Used to set placement object, and access pointer renderer
    public GameObject introAnimation;  //Intro animation UI
    public GameObject placementProp;   //Shown during the placement phase to identify the anchor point
    public GameObject anchor;          //Root transform of anchored content.
    public MagicLeap.Core.MLImageTrackerBehavior tracker1;

    [Header("Login UI")]
    public GameObject loginUI;         //UI panel for login
    public GameObject guestButton;     //Button to automatically login as a guest
    public GameObject pin;             //Pin entry UI collection
    public Text pinInput;              //Text input for the pin
    public GameObject loading;         //
    public GameObject keyboard;        //VR keyboard, used for text input

    [Header("Lab Selection")]
    public GameObject labOptions;      //UI containing list of labs that are clicked to select
    public GameObject labTemp;         //Used to clone and make new lab selection buttons
    public GameObject labStarter;      //The object holding

    // Private Variables
    [Header("Other Necessary Components")]
    [SerializeField]
    private Authenticate auth;         //Used to authenticate pin logins

    [Header("Timeout Configuration")]
    [SerializeField]
    private float authTimeout = 15f;   //Used to limit waiting for Authenticate to be ready
    [SerializeField]
    private float labParseTimeout = 15f;//Used to limit waiting for lab parsing to be done

    [Header("Debug Server Interactions")]
    [Tooltip("Sets whether to download files from production endpoints, or from show_uploaded/filename")]
    [SerializeField]
    private EndpointType endpointsType;
    [Tooltip("What to append to show_uploaded/ to download a list of labs")]
    [SerializeField]
    private string debugLabListFilename = "";
    [Tooltip("What to append to show_uploaded/ to download the zipped lab resources:")]
    [SerializeField]
    private string debugLabZipFilename = "";
    [Tooltip("Whether to download new files if we have local copies already")]
    [SerializeField]
    private bool forceDownloads = false;

    enum EndpointType { debug, production };
    [Header("Local Development")]
    [Tooltip("Whether to skip the initial animation when starting a lab")]
    [SerializeField]
    private bool skipIntroAnimation = false;
    [Tooltip("Toggle whether to skip login and lab download, and instead use local files specified by debugLabResources")]
    [SerializeField]
    private bool skipLoginAndDownload = false;
    [Tooltip("Extracted folder to pull media and lab JSON from for local testing, absolute path")]
    [SerializeField]
    private string debugLabResources = "";

    //Base URLs to download from
    const string LABS_URL = "https://cyberlearnar.cs.mtsu.edu/labs";
    const string LAB_ZIP_BASE_URL = "https://cyberlearnar.cs.mtsu.edu/show_uploaded/lab_";
    const string DEBUG_URL = "https://cyberlearnar.cs.mtsu.edu/show_uploaded/";
    
    // Used to reference downloaded files
    private string loginDirectory = "";  // Initialized with Persistent data path in awake
    private string labZipDirectory = ""; //   \/           \/            \/          \/
    private FileInfo allLabsFileInfo;    // Used to interact with downloaded files
    private FileInfo labZipFileInfo;     //   \/           \/            \/

    // Used to track current internal state of login scene
    private enum state
    {
        introduction,
        placement,
        pin_entry,
        authentication,
        lab_selection,
        lab_initiation,
        lab_running,
        end_of_states
    }
    private state currentState = state.introduction;
    private bool placed = false;       //Whether the user has placed the scene anchor
    private bool labsReady = false;    //Whether the list of labs has been downloaded and parsed
    private List<GameObject> uiLabList;//The current list of labs the user can select from
    private List<LabInfo> labInfoList; //List of id, name, and description for each lab downloaded
    private LabInfo selectedLab;       //The lab selected by the user, used to pull its json

    // Convenience variables for using the logger
    private string entity;             //Name of this class, as a string. init in Start
    private LabLogger logger;          //Used to log data about the user session, init in Start
    #endregion Variables

    #region Unity Methods
    private void Awake()
    {
        // Fail fast assertions
        Assert.IsNotNull(controller);
        Assert.IsNotNull(introAnimation);
        Assert.IsNotNull(placementProp);
        Assert.IsNotNull(anchor);
        Assert.IsNotNull(loginUI);
        Assert.IsNotNull(guestButton);
        Assert.IsNotNull(pin);
        Assert.IsNotNull(pinInput);
        Assert.IsNotNull(loading);
        Assert.IsNotNull(keyboard);
        Assert.IsNotNull(labOptions);
        Assert.IsNotNull(labTemp);
        Assert.IsNotNull(labStarter);
        Assert.IsNotNull(auth);

        // Variable initializations
        entity = this.GetType().ToString();
        logger = LabLogger.Instance;
        labInfoList = new List<LabInfo>();
        uiLabList = new List<GameObject>();

        // Initialize Download Directories
        loginDirectory = Path.Combine(Application.persistentDataPath, "login");
        allLabsFileInfo = new FileInfo(Path.Combine(loginDirectory, "All_Labs.json"));
        allLabsFileInfo.Directory.Create();
        // We don't know the name of the lab downloaded yet, so wait to initialize FileInfo
        labZipDirectory = Path.Combine(Application.persistentDataPath, "lab_resources");
    }

    private void Start()
    {
        logger.InfoLog(entity, LabLogger.LogTag.STATE_START, "Introduction");
        // If we are skipping login/download, check that debug resources folder exists
        if (skipLoginAndDownload == true && (debugLabResources == "" || !Directory.Exists(debugLabResources)))
            logger.InfoLog(entity,
                LabLogger.LogTag.ERROR,
                $"Skipping login, but path to local lab resources not set correctly: {debugLabResources}");

        // Make sure controller isn't visible
        HidePointer();

        // Download the lab list if this is a production build, or a debug build that is not skipping login
        if (!Debug.isDebugBuild || (Debug.isDebugBuild && !skipLoginAndDownload))
        { 
            // Download list of available labs
            // This could eventually be changed to pull only the labs related to the pin->cnum->CRNs,
            // eventually not in Start
            if (endpointsType == EndpointType.production)
                DownloadUtility.Instance.DownloadFile(LABS_URL, allLabsFileInfo.FullName, LabsDownloaded, !forceDownloads);
            else if (endpointsType == EndpointType.debug)
                DownloadUtility.Instance.DownloadFile(
                    Path.Combine(DEBUG_URL, debugLabListFilename),
                    allLabsFileInfo.FullName,
                    LabsDownloaded,
                    !forceDownloads);
        }

        // Run the Intro animation if this is a production build, or a debug build that is not skipping the anim
        if (!Debug.isDebugBuild || (Debug.isDebugBuild && !skipIntroAnimation))
        {
            // Start Intro Animation (MT Logo) and wait for it to complete
            StartCoroutine(SetupIntroAnimation());
        }
        else // Skip to Placement
            ChangeStateTo(state.placement);
    }
    #endregion Unity Methods

    #region Private Methods
    /// <summary>
    /// Changes current state, then executes the necessary tasks to go to that state
    /// </summary>
    /// <param name="newState">State to transition to</param>
    private void ChangeStateTo(state newState)
    {
        currentState = newState;

        switch(currentState)
        {
            case state.placement:
                {
                    introAnimation.SetActive(false);
                    placed = false;
                    tracker1.OnTargetFound += OnTarget1Found;
                    tracker1.OnTargetUpdated += OnTarget1Updated;
                    tracker1.OnTargetLost += OnTarget1Lost;
                    tracker1.enabled = true;
                    break;

                    logger.InfoLog(entity, LabLogger.LogTag.STATE_START, "Placement");
                    HidePointer();
                    // Hide the intro animation
                    // introAnimation.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    introAnimation.SetActive(false);

                    // Clear out lab list ,in case placement was looped to from lab selection
                    foreach (GameObject go in uiLabList)
                        Destroy(go);

                    // Enable placement object and set flag to not placed
                    placementProp.SetActive(true);
                    placed = false;
                    // Start coroutine to move the lab anchor object
                    StartCoroutine(AlignUIWithController());
                    // Start audio instruction playback
                    placementProp.GetComponent<AudioSource>()?.Play();
                    // Bind the place function to the trigger
                    controller.GetComponent<ControlInput>().OnTriggerDown.AddListener(Place);
                    
                    // WE NOW WAIT UNTIL A TRIGGER PRESS TO GO ON TO pin_entry
                    break;
                }
            case state.pin_entry:
                {
                    logger.InfoLog(entity, LabLogger.LogTag.STATE_START, "Pin Entry");
                    ShowPointer();
                    // Hide placement object
                    placementProp.SetActive(false);
                    // If authentication failed, cleanup leftovers
                    loading.SetActive(false);

                    // Activate UI
                    loginUI.SetActive(true);
                    pin.SetActive(true);
                    keyboard.SetActive(true);
                    guestButton.SetActive(true);

                    // WE NOW WAIT UNTIL "ENTER" IS PRESSED ON THE KEYBOARD TO GO TO authentication
                    break;
                }
            case state.authentication:
                {
                    logger.InfoLog(entity, LabLogger.LogTag.STATE_START, "Authentication");
                    // Disable pin entry
                    pin.SetActive(false);
                    keyboard.SetActive(false);
                    guestButton.SetActive(false);

                    // Cleanup incase authentication fails
                    keyboard.GetComponent<VRKeyboard.Utils.KeyboardManager>().resetText();

                    // Submit Authentication request
                    loading.SetActive(true);
                    StartCoroutine(AwaitAuthentication(pinInput.text));

                    // WE NOW WAIT ON THE RESULT OF AUTHENITACTION
                    // IF IT TIMES OUT, OR THE PIN IS NOT AUTHENTICATED, GO TO pin_entry
                    // IF IT IS AUTHENTICATED, GO TO lab_selection
                    break;
                }
            case state.lab_selection:
                {
                    logger.InfoLog(entity, LabLogger.LogTag.STATE_START, "Lab Selection");
                    // Disable Login UI and keyboard
                    loginUI.SetActive(false);
                    keyboard.SetActive(false);
                    labStarter.SetActive(false);

                    // Check if the labs list is ready
                    if(labsReady)
                    {
                        // If it is, start creating the lab options UI
                        labOptions.SetActive(true);
                        GenerateLabListUI();
                    }
                    else
                    {
                        // If it's not, then wait for it to be ready.
                        StartCoroutine(AwaitLabParsing());
                    }

                    // IF THE LABS WERE READY, NOW WE WAIT UNTIL THE USER SELECTS A LAB, GO TO lab_initiation
                    // IF THE LABS WEREN'T READY, BUT BECOME READY AFTER WAITING, THE SAME HAPPENS
                    // IF THE COROUTINE TIMES OUT WHILE WAITING, THE PROGRAM ERRORS AND STOPS
                    break;
                }
            case state.lab_initiation:
                {
                    logger.InfoLog(entity, LabLogger.LogTag.STATE_START, "Lab Initiation");
                    // Disable all UI
                    labOptions.SetActive(false);

                    // Enable the object that contains the lab objects
                    labStarter.SetActive(true);
                    //labStarter.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
                    //labStarter.transform.position = Camera.main.transform.position - Vector3.up * 0.2f;

                    // If skipLogin is true, just use the provided local resources for quick lab debugging
                    if (Debug.isDebugBuild && skipLoginAndDownload)
                        LabStart(new DirectoryInfo(debugLabResources));
                    else
                    { 
                        // Download the Zip file containing all lab resources
                        if (endpointsType == EndpointType.production)
                        {
                            labZipFileInfo = new FileInfo(Path.Combine(labZipDirectory, selectedLab.id + ".zip"));
                            labZipFileInfo.Directory.Create();
                            DownloadUtility.Instance.DownloadAndExtractZip(
                                Path.Combine(LAB_ZIP_BASE_URL, selectedLab.id + ".zip"),
                                labZipFileInfo.FullName,
                                LabZipDownloadedAndExtracted,
                                !forceDownloads);
                        }
                        else if (endpointsType == EndpointType.debug)
                        {
                            labZipFileInfo = new FileInfo(Path.Combine(labZipDirectory, debugLabZipFilename));
                            labZipFileInfo.Directory.Create();
                            DownloadUtility.Instance.DownloadAndExtractZip(
                                Path.Combine(DEBUG_URL, debugLabZipFilename),
                                labZipFileInfo.FullName,
                                LabZipDownloadedAndExtracted,
                                !forceDownloads);
                        }
                    }

                    // WAIT FOR THE LABJSON TO DOWNLOAD
                    // ONCE IT FINISHES, THE MEDIA CATALOGUE IS GIVEN THE INFO IT NEEDS TO INITIALIZE
                    // THEN WAIT FOR THE MEDIA CATALOGUE TO FINISH INITIALIZING BEFORE STARTING THE LAB
                    break;
                }
            default:
                break;
        }
    }

    /// <summary>
    /// Marks the anchor as placed, and unbinds itself from trigger
    /// </summary>
    private void Place()
    {
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "Place()");
        // Unbind place function from trigger
        controller.GetComponent<ControlInput>().OnTriggerDown.RemoveListener(Place);
        anchor.transform.position = placementProp.transform.position;
        anchor.transform.rotation = placementProp.transform.rotation;
        //labStarter.transform.position = placementProp.transform.position;
        //labStarter.transform.rotation = placementProp.transform.rotation;
        GameObject.Find("Directional Light").transform.SetParent(labStarter.transform);
        // Set placed flag, and move to the next state
        placed = true;
        // Stop Audio playback
        placementProp.GetComponent<AudioSource>()?.Stop();
        // Either move to login, or skip to lab start for quick local debugging
        if (Debug.isDebugBuild && skipLoginAndDownload)
            ChangeStateTo(state.lab_initiation);
        else
            ChangeStateTo(state.pin_entry);
    }

    private void ConfirmTarget1()
    {
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "ConfirmTarget1()");
        controller.GetComponent<ControlInput>().OnTriggerDown.RemoveListener(ConfirmTarget1);
        anchor.transform.position = placementProp.transform.position;
        anchor.transform.rotation = placementProp.transform.rotation;
        GameObject.Find("Directional Light").transform.SetParent(labStarter.transform);
        tracker1.OnTargetFound -= OnTarget1Found;
        tracker1.OnTargetUpdated -= OnTarget1Updated;
        tracker1.OnTargetLost -= OnTarget1Lost;
        tracker1.enabled = false;
        placed = true;
        if (Debug.isDebugBuild && skipLoginAndDownload)
            ChangeStateTo(state.lab_initiation);
        else
            ChangeStateTo(state.pin_entry);
    }

    /// <summary>
    /// This Will change depending on how and what is stored in lab jsons
    /// </summary>
    private void ParseLabs()
    {
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "ParseLabs()");
        // Trim leading and trailing [{}]
        string labsString = allLabsFileInfo.OpenText().ReadToEnd().Trim(new char[] { '[', '{', '}', ']' });
        // Split by },{ which only occurs between labs in the list
        string[] labElements = labsString.Split(new string[] { "},{" }, System.StringSplitOptions.None);

        foreach (string lab in labElements)
        {
            LabInfo tmpLabInfo = new LabInfo();

            // Find description
            string descSearch = "\"lab_description\":\"";
            int descLoc = lab.IndexOf(descSearch) + descSearch.Length;
            tmpLabInfo.description = lab.Substring(descLoc, lab.IndexOf("\"", descLoc) - descLoc);

            // Find id
            string idSeach = "\"lab_id\":";
            int idLoc = lab.IndexOf(idSeach) + idSeach.Length;
            tmpLabInfo.id = lab.Substring(idLoc, lab.IndexOf(",", idLoc) - idLoc);

            // Find name
            string nameSearch = "\"lab_title\":\"";
            int nameLoc = lab.IndexOf(nameSearch) + nameSearch.Length;
            tmpLabInfo.name = lab.Substring(nameLoc, lab.IndexOf("\"", nameLoc) - nameLoc);

            labInfoList.Add(tmpLabInfo);
        }
        // Log the successful parse
        string labNames = "";
        foreach(LabInfo lab in labInfoList)
            labNames += lab.name + ", ";
        logger.InfoLog(entity, LabLogger.LogTag.DEBUG, $"Finished parsing labs:" + labNames);
        labsReady = true;
    }

    /// <summary>
    /// Generates list of lab buttons for the user to select. also creates an exit button
    /// </summary>
    private void GenerateLabListUI()
    {
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "GenerateLabListUI()");
        // Generate 5 lab buttons, or less if the list of available labs is short
        int i = 0;
        while(i < 5 && i < labInfoList.Count)
        {
            // Create lab ui object and position it
            GameObject tmpLabUI = Instantiate(labTemp, labOptions.transform);
            tmpLabUI.transform.position += .42f * (i % 2) * anchor.transform.right + new Vector3(0, -.15f * (i / 2), 0);

            // Add listener to button to transition to next stage
            int i_copy = i; // This is done b/c of some anonymous function variable capturing loop variable technicalities
            tmpLabUI.GetComponentInChildren<Button>().onClick.AddListener(() => LabSelected(labInfoList[i_copy].id));

            // Setup object, add to list
            tmpLabUI.transform.Find("Lab Title").GetComponent<Text>().text = labInfoList[i].name;
            tmpLabUI.transform.Find("Lab Description").GetComponent<Text>().text = labInfoList[i].description;
            tmpLabUI.name = labInfoList[i].name;
            tmpLabUI.SetActive(true);
            uiLabList.Add(tmpLabUI);

            i++;
        }

        // Create an exit button using the lab button template
        GameObject exitUI = Instantiate(labTemp, labOptions.transform);
        exitUI.transform.position += .42f * (i % 2) * anchor.transform.right + new Vector3(0, -.15f * (i / 2), 0);

        // Add listiner to end application
        exitUI.GetComponentInChildren<Button>().onClick.AddListener(() => ExitSelected());

        // Set title and description
        exitUI.transform.Find("Lab Title").GetComponent<Text>().text = "Exit";
        exitUI.transform.Find("Lab Description").GetComponent<Text>().text = "Close ARLabs";

        exitUI.name = "ExitButton";
        exitUI.SetActive(true);
        uiLabList.Add(exitUI);
    }

    /// <summary>
    /// Starts initializing the MediaCatalogue for the lab
    /// </summary>
    /// <param name="labResourcesFolderInfo">Info about the folder holding the resources for the lab</param>
    private void LabStart(DirectoryInfo labResourcesFolderInfo)
    {
        // Find the Json file in the resources folder
        FileInfo labJsonInfo = null;
        foreach (FileInfo file in labResourcesFolderInfo.GetFiles())
        {
            if (file.Name.EndsWith(".json"))
            {
                labJsonInfo = file;
                break;
            }
        }
        if (labJsonInfo == null)
        {
            logger.InfoLog(entity, LabLogger.LogTag.ERROR, $"Could not find lab json after lab zip downloaded and extracted, stopping");
            return;
        }

        // Initialize lab data object
        LabDataObject labData = new LabDataObject();
        JsonUtility.FromJsonOverwrite(labJsonInfo.OpenText().ReadToEnd(), labData);

        // Find and start initializing the media catalogue
        MediaCatalogue mc = GetComponent<MediaCatalogue>();
        mc.enabled = true;
        mc.InitializeCatalogue(labResourcesFolderInfo);

        // Start waiting for the media catalogue to finish setting up
        // Once it finishes, initialize the lab manager (next function)
        StartCoroutine(AwaitMediaCatalogueInitialization(mc, labData));
    }

    /// <summary>
    /// Initializes the Lab Manager with data object filled with parsed JSON data
    /// </summary>
    /// <param name="labData">Data object describing the lab</param>
    private void InitializeLabManager(LabDataObject labData)
    {
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "InitializeLabManager()");
        LabManager lm = labStarter.GetComponent<LabManager>();
        lm.enabled = true;
        lm.Initialize(labData);
    }

    private void HidePointer() => ToggleControllerRendering(false);
    private void ShowPointer() => ToggleControllerRendering(true);
    private void ToggleControllerRendering(bool flag)
    {
        controller.GetComponent<LineRenderer>().enabled = flag;
        controller.GetComponentInChildren<MeshRenderer>().enabled = flag;
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "ToggleControllerRendering()");
    }

    /// <summary>
    /// Called to end the lab entirely, different behaivour depending on execution environment
    /// </summary>
    private void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion Private Methods

    #region Coroutines
    /// <summary>
    /// Waits until the main camera syncs to the headset position,
    /// then places the intro animation in the proper location
    /// </summary>
    private IEnumerator SetupIntroAnimation()
    {
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "SetupIntroAnimation()");
        // Go ahead and start the animation
        introAnimation.SetActive(true);

        // While the main camera hasn't moved from the origin, wait for it to move (sync to headset position)
        yield return new WaitUntil(() => Camera.main.transform.position != Vector3.zero);

        // Once the camera is synced to the headset position, Place the intro animation
        introAnimation.transform.position =
            Camera.main.transform.position + Camera.main.transform.forward * 5;
        // not sure why there is a quaternion multiplied by a vector added to position, so I did my best
        // to guess what he was doing
            //Camera.main.transform.position + Camera.main.transform.rotation * new Vector3(0, 0, 5);
        introAnimation.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);

        // Wait for intro to finish
        StartCoroutine(AwaitIntroAnimation());
    }

    private IEnumerator AwaitIntroAnimation()
    {
        // While the animator is running, and not in state "Idle", wait
        yield return new WaitUntil(
            () => introAnimation.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"));

        // Start Placement once the animation is done
        ChangeStateTo(state.placement);
    }

    private IEnumerator TurnOffProp()
    {
        yield return new WaitForSeconds(2.0f);
        placementProp.SetActive(false);
    }

    /// <summary>
    /// Sets position of the UI anchor, effectively making all ui and the prop follow
    /// the controller. 
    /// </summary>
    private IEnumerator AlignUIWithController()
    {
        while(placementProp.activeSelf && !placed)
        {
            placementProp.transform.position = controller.transform.position;
            placementProp.transform.eulerAngles = new Vector3(0, controller.transform.eulerAngles.y, 0);
            yield return null;
        }
    }

    /// <summary>
    /// Checks to see if Authenticate is ready for queries, waits until timeout. 
    /// Checks if pin is correct, and if so initializes the logger and start lab selection
    /// </summary>
    /// <param name="pin">pin to authenticate</param>
    /// <param name="timeout">how long to wait on Authenticate to be ready</param>
    private IEnumerator AwaitAuthentication(string pin)
    {
        float countdown = authTimeout;  //How long to wait for Authenticate to be ready

        // Check that Authenticat is ready to accept queries
        while (!auth.Ready)
        {
            yield return null;
            countdown -= Time.deltaTime;

            // Check and see if we time out
            if (countdown <= 0)
            {
                logger.InfoLog(entity, LabLogger.LogTag.ERROR, $"Authentication timed out, waited {authTimeout} seconds.");
                ChangeStateTo(state.pin_entry);
                // Exit the coroutine
                yield break;
            }
        }

        // If Auth is ready, authenticate the pin
        if(auth.AuthenticatePin(pin))
        {
            string studentName = auth.PinToName(pin);
            string mNum = auth.PinToMNum(pin);
            // Initialize the logger with the student information
            logger.InitializeLog(studentName, mNum);
            logger.InfoLog(entity, LabLogger.LogTag.DEBUG, $"Student authenticated: {studentName}, M{mNum}");
            // Go to lab selection state
            ChangeStateTo(state.lab_selection);
        }
        else // Pin was not authenticated
        {
            logger.InfoLog(entity, LabLogger.LogTag.DEBUG, "Pin failed to authenticate");
            ChangeStateTo(state.pin_entry);
        }
    }

    /// <summary>
    /// Waits until the labs are parsed, with a timeout
    /// </summary>
    private IEnumerator AwaitLabParsing()
    {
        float countdown = labParseTimeout;  //How long to wait for the labs to finish parsing

        // Wait while the labs list isn't parsed
        while(!labsReady)
        {
            yield return null;
            countdown -= Time.deltaTime;

            if(countdown <= 0)
            {
                logger.InfoLog(entity, LabLogger.LogTag.ERROR, $"Lab Parsing timed out, waited ${labParseTimeout} seconds. STOPPING");
                yield break;
            }
        }

        // If the lab list is ready, start creating the lab options UI
        labOptions.SetActive(true);
        GenerateLabListUI();
    }

    private IEnumerator AwaitMediaCatalogueInitialization(MediaCatalogue mc, LabDataObject labData)
    {
        yield return new WaitUntil(() => mc.DoneLoadingAssets);
        InitializeLabManager(labData);
        ChangeStateTo(state.lab_running);
    }
    #endregion Coroutines

    #region Public Callbacks
    public void SetForceDownload(bool val)
    {
        forceDownloads = val;
    }

    /// <summary>
    /// Called by the enter key on the keyboard, start authentication
    /// </summary>
    public void PinEntered()
    {
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "PinEntered()");
        ChangeStateTo(state.authentication);
    }

    public void GuestLogin()
    {
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "GuestLogin()");
        pinInput.text = "000000";
        ChangeStateTo(state.authentication);
    }

    /// <summary>
    /// Called by the lab manager when it determines that the lab is done
    /// </summary>
    public void LabComplete()
    {
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "LabComplete()");
        ChangeStateTo(state.lab_selection);
    }

    /// <summary>
    /// Called by the exit button on the lab selection screen, starts uploading log before quitting
    /// </summary>
    public void ExitSelected()
    {
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "ExitSelected()");
        logger.SubmitLog(LogSubmitted);
    }
    #endregion Public Callbacks

    #region Private Event Handlers
    private void OnTarget1Found(MLImageTracker.Target target, MLImageTracker.Target.Result result)
    {
        placementProp.transform.position = result.Position;
        placementProp.transform.eulerAngles = new Vector3(0, result.Rotation.eulerAngles.y, 0);
        placementProp.SetActive(true);
        controller.GetComponent<ControlInput>().OnTriggerDown.AddListener(ConfirmTarget1);
    }

    private void OnTarget1Updated(MLImageTracker.Target target, MLImageTracker.Target.Result result)
    {
        placementProp.transform.position = result.Position;
        placementProp.transform.eulerAngles = new Vector3(0, result.Rotation.eulerAngles.y, 0);
    }

    private void OnTarget1Lost(MLImageTracker.Target target, MLImageTracker.Target.Result result)
    {
        placementProp.SetActive(false);
        controller.GetComponent<ControlInput>().OnTriggerDown.RemoveListener(ConfirmTarget1);
    }

    /// <summary>
    /// Called by DownloadUtility once it is done with the lab list download
    /// </summary>
    /// <param name="rc">return code from DownloadFile(). 0 if succesful, -1 if failed</param>
    private void LabsDownloaded(int rc)
    {
        if(rc == 0) // File successfully downloaded
        {
            // Generate lab list with titles, ids, and descriptions
            ParseLabs();
        }
        else // Download Failed
        {
            logger.InfoLog(entity, LabLogger.LogTag.ERROR, "Labs failed to download");
        }
    }

    /// <summary>
    /// Called when a lab selection button is clicked, triggering lab initiation
    /// </summary>
    /// <param name="labID">id of the lab that was selected</param>
    private void LabSelected(string labID)
    {
        logger.InfoLog(entity, LabLogger.LogTag.TRACE, "LabSelected()");
        // Set the current selected lab to match the passed id
        selectedLab = labInfoList.Find(x => x.id == labID);
        // Log what lab was selected
        logger.InfoLog(entity, LabLogger.LogTag.DEBUG, $"Lab Selected: {selectedLab.id}, {selectedLab.name}");
        // Transition to next state
        ChangeStateTo(state.lab_initiation);
    }

    /// <summary>
    /// Called by DownloadUtility once it is done with the lab json download
    /// </summary>
    /// <param name="rc">return code from DownloadFile(). 0 if succesful, -1 if failed</param>
    private void LabZipDownloadedAndExtracted(int rc)
    {
        // Download and extraction were a success
        if (rc == 0)
        {
            if (endpointsType == EndpointType.debug)
                LabStart(new DirectoryInfo(Path.Combine(
                    labZipDirectory,
                    debugLabZipFilename.Substring(0, debugLabZipFilename.LastIndexOf(".")))));
            else if (endpointsType == EndpointType.production)
                LabStart(new DirectoryInfo(Path.Combine(
                    labZipDirectory,
                    selectedLab.id)));
        }
        else // Download failed
            logger.InfoLog(entity, LabLogger.LogTag.ERROR, $"LabZip failed to download. Stopping Program");
    }

    /// <summary>
    /// Called by the logger once the file is finished uploading
    /// </summary>
    private void LogSubmitted()
    {
        ExitApplication();
    }
    #endregion Private Event Handlers
}
