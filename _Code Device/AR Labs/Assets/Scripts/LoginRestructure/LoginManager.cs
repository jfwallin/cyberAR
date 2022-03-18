using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using MagicLeapTools;

public struct LabInfo
{
    public string id;
    public string name;
    public string description;

    public override string ToString()
    {
        return $"lab id: {id}\n  name: {name}\n  desc: {description}";
    }
}

public class LoginManager : MonoBehaviour
{
    #region Variables
    // Public Variables
    [Header("Introduction")]
    public GameObject controller;      //Used to set placement object, and access pointer renderer
    public GameObject introAnimation;  //Intro animation UI
    public GameObject placementProp;   //Shown during the placement phase to identify the anchor point
    public GameObject anchor;          //Root transform of anchored content.

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
    private TestWrite logger;          //Used to log data about the user session
    [SerializeField]
    private Authenticate auth;         //Used to authenticate pin logins

    [Header("Configuration")]
    [SerializeField]
    private float authTimeout = 15f;   //Used to limit waiting for Authenticate to be ready
    [SerializeField]
    private float labParseTimeout = 15f;//Used to limit waiting for lab parsing to be done

    [SerializeField]
    const string LABS_URL = "http://cyberlearnar.cs.mtsu.edu/labs";
    //const string LABS_URL = "http://cyberlearnar.cs.mtsu.edu/show_uploaded_labs";
    [SerializeField]
    const string LABS_FILEPATH = "login/allLabs";
    [Tooltip("Name of lab gets appended with .json, used to attempt to download json for the lab")]
    [SerializeField]
    const string LAB_JSON_BASE_URL = "http://cyberlearnar.cs.mtsu.edu/show_uploaded/lab_";
    [Tooltip("Relative to the Resources folder, where all individual lab JSONS are downloaded")]
    [SerializeField]
    const string LAB_JSON_BASE_FILEPATH = "lab_jsons/";

    private enum state
    {
        introduction,
        placement,
        pin_entry,
        authentication,
        lab_selection,
        lab_initiation,
        end_of_states
    }
    // Used to track current internal state of login scene
    private state currentState = state.introduction;

    private string entity;
    private bool placed = false;       //Whether the user has placed the scene anchor
    private bool labsReady = false;    //Whether the list of labs has been downloaded and parsed
    private List<GameObject> uiLabList;//The current list of labs the user can select from
    private List<LabInfo> labInfoList; //List of id, name, and description for each lab downloaded
    private LabInfo selectedLab;       //The lab selected by the user, used to pull its json
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
    }

    private void Start()
    {
        entity = this.GetType().ToString();
        logger = TestWrite.Instance;
        logger.InfoLog(entity, "Login Scene Starting....");
        // Setup keyboard
        keyboard.transform.Find("Content").Find("Keys").Find("row4").Find("Enter").GetComponent<Button>()
            .onClick.AddListener(() => PinEntered());

        // Make sure controller isn't visible
        HidePointer();

        // Download list of available labs
        // This could eventually be changed to pull only the labs related to the pin->cnum->CRNs, eventually not in Start
        DownloadUtility.Instance.DownloadFile(LABS_URL, LABS_FILEPATH + ".json", LabsDownloaded);

        // Start Intro Animation (MT Logo) and wait for it to complete
        StartCoroutine(SetupIntroAnimation());
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
                    logger.InfoLog(entity, "Started placement");
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
                    // Bind the place function to the trigger
                    controller.GetComponent<ControlInput>().OnTriggerDown.AddListener(Place);

                    // WE NOW WAIT UNTIL A TRIGGER PRESS TO GO ON TO pin_entry
                    break;
                }
            case state.pin_entry:
                {
                    logger.InfoLog(entity, "Started Pin Entry");
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
                    logger.InfoLog(entity, "Started Authenticating pin");
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
                    logger.InfoLog(entity, "Started lab selection");
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
                    logger.InfoLog(entity, "Started lab Initiaion");
                    // Disable all UI
                    labOptions.SetActive(false);

                    // Enable the object that contains the lab objects
                    labStarter.SetActive(true);
                    labStarter.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
                    labStarter.transform.position = Camera.main.transform.position - Vector3.up * 0.2f;

                    // Download the JSON file describing the lab.
                    string labJsonUrl = LAB_JSON_BASE_URL + selectedLab.id + ".json";
                    string labJsonFilepath = LAB_JSON_BASE_FILEPATH + selectedLab.id + ".json";
                    DownloadUtility.Instance.DownloadFile(labJsonUrl, labJsonFilepath, LabJSONDownloaded);

                    // WAIT FOR THE LABJSON TO DOWNLOAD
                    // ONCE IT FINISHES, THE MEDIA CATALOGUE IS GIVEN THE INFO IT NEEDS TO INITIALIZE
                    // THEN WAIT FOR THE MEDIA CATALOGUE TO FINISH INITIALIZING BEFORE STARTING THE LAB
                    break;
                }
        }
    }

    /// <summary>
    /// Marks the anchor as placed, and unbinds itself from trigger
    /// </summary>
    private void Place()
    {
        logger.InfoLog(entity, "Lab has been placed");
        // Unbind place function from trigger
        controller.GetComponent<ControlInput>().OnTriggerDown.RemoveListener(Place);
        // Set placed flag, and move to the next state
        placed = true;
        ChangeStateTo(state.pin_entry);
    }

    /// <summary>
    /// This Will change depending on how and what is stored in lab jsons
    /// </summary>
    private void ParseLabs()
    {
        logger.InfoLog(entity, "Parsing labs");
        // Trim leading and trailing [{}]
        string labsString = Resources.Load<TextAsset>(LABS_FILEPATH).text.Trim(new char[] { '[', '{', '}', ']' });
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
        logger.InfoLog(entity, $"Finished parsing labs:\n{labInfoList}");

        labsReady = true;
    }

    /// <summary>
    /// Generates list of lab buttons for the user to select. also creates an exit button
    /// </summary>
    private void GenerateLabListUI()
    {
        logger.InfoLog(entity, "Generating lab selection buttons");
        // Generate 5 lab buttons, or less if the list of available labs is short
        int i = 0;
        while(i < 5 && i < labInfoList.Count)
        {
            // Create lab ui object and position it
            GameObject tmpLabUI = Instantiate(labTemp, labOptions.transform);
            tmpLabUI.transform.position += .42f * (i % 2) * anchor.transform.right + new Vector3(0, -.15f * (i / 2), 0);

            // Add listener to button to transition to next stage
            tmpLabUI.GetComponentInChildren<Button>().onClick.AddListener(() => LabSelected(labInfoList[i].id));

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

    private void InitializeLabManager(LabDataObject labData)
    {
        logger.InfoLog(entity, "Initializing Lab Manager");
        LabManager lm = labStarter.GetComponent<LabManager>();
        lm.enabled = true;
        lm.Initialize(labData.ActivityModules);
    }

    private void HidePointer() => ToggleControllerRendering(false);
    private void ShowPointer() => ToggleControllerRendering(true);
    private void ToggleControllerRendering(bool flag)
    {
        controller.GetComponent<LineRenderer>().enabled = flag;
        controller.GetComponentInChildren<MeshRenderer>().enabled = flag;
    }
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
        logger.InfoLog(entity, "Starting intro animation, waiting till it completes");
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

    /// <summary>
    /// Sets position of the UI anchor, effectively making all ui and the prop follow
    /// the controller. 
    /// </summary>
    private IEnumerator AlignUIWithController()
    {
        while(placementProp.activeSelf && !placed)
        {
            anchor.transform.position = controller.transform.position;
            anchor.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
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
                Debug.LogError($"Authentication timed out, waited {authTimeout} seconds.");
                logger.InfoLog(entity, $"Authentication timed out, waited {authTimeout} seconds.");
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
            logger.InfoLog(entity, $"Student authenticated: {studentName}, M{mNum}");
            // Go to lab selection state
            ChangeStateTo(state.lab_selection);
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
                Debug.LogError($"Lab Parsing timed out, waited ${labParseTimeout} seconds. STOPPING");
                logger.InfoLog(entity, $"Lab Parsing timed out, waited ${labParseTimeout} seconds. STOPPING");
                yield break;
            }
        }

        // If the lab list is ready, start creating the lab options UI
        labOptions.SetActive(true);
        GenerateLabListUI();
    }

    private IEnumerator AwaitMediaCatalogueInitialization(MediaCatalogue mc, LabDataObject labData)
    {
        yield return new WaitUntil(() => mc.done);
        InitializeLabManager(labData);
    }
    #endregion Coroutines

    #region Public Callbacks
    /// <summary>
    /// Called by the enter key on the keyboard, start authentication
    /// </summary>
    public void PinEntered()
    {
        logger.InfoLog(entity, "Pin Entered");
        ChangeStateTo(state.authentication);
    }

    /// <summary>
    /// Called by the lab manager when it determines that the lab is done
    /// </summary>
    public void LabComplete()
    {
        logger.InfoLog(entity, "Lab marked complete, returning to lab selection");
        ChangeStateTo(state.lab_selection);
    }    
    #endregion Public Callbacks

    #region Private Event Handlers
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
            Debug.LogError("Labs file download failed");
        }
    }

    /// <summary>
    /// Called when a lab selection button is clicked, triggering lab initiation
    /// </summary>
    /// <param name="labID">id of the lab that was selected</param>
    private void LabSelected(string labID)
    {
        // Set the current selected lab to match the passed id
        selectedLab = labInfoList.Find(x => x.id == labID);
        // Log what lab was selected
        logger.InfoLog(entity, $"Lab Selected: {selectedLab.id}, {selectedLab.name}");
        // Transition to next state
        ChangeStateTo(state.lab_initiation);
    }

    /// <summary>
    /// Called by the exit button on the lab selection screen, starts uploading log before quitting
    /// </summary>
    private void ExitSelected()
    {
        Debug.Log("Exit button selected");
        logger.InfoLog(entity, "Exit button selected");
        logger.SubmitLog(LogSubmitted);
    }

    /// <summary>
    /// Called by DownloadUtility once it is done with the lab json download
    /// </summary>
    /// <param name="rc">return code from DownloadFile(). 0 if succesful, -1 if failed</param>
    private void LabJSONDownloaded(int rc)
    {
        if(rc == 0)
        {
            // Initialize lab data object
            string jsonPath = LAB_JSON_BASE_FILEPATH + selectedLab.id;
            string jsonString = Resources.Load<TextAsset>(jsonPath).text;
            LabDataObject labData = new LabDataObject();
            JsonUtility.FromJsonOverwrite(jsonString, labData);

            // Find and start initializing the media catalogue
            MediaCatalogue mc = labStarter.GetComponent<MediaCatalogue>();
            mc.enabled = true;
            mc.addToCatalogue(labData);

            // Start waiting for the media catalogue to finish setting up
            StartCoroutine(AwaitMediaCatalogueInitialization(mc, labData));
        }
        else // Download failed
        {
            Debug.LogError("LabJSON failed to download. stopping");
        }
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
