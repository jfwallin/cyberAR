﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class loginLogic : MonoBehaviour
{
    #region Public Variables
    [Header("Toggle Downloading")]
    [Tooltip("This toggle allows for use of immediate local files instead of pulling them from the web")]
    public bool useLocalFiles = true;
    //public bool useLocalFiles = false;

    [Header("GameObjects")]
    public GameObject controller;
    public GameObject intro;
    public GameObject placement_prop;
    public GameObject anchor;

    public GameObject LoginUI;
    public GameObject guestButton;
    public GameObject usr;
    public GameObject pas;
    public GameObject loading;
    public GameObject keyboard;

    public GameObject lab_options;
    public GameObject labTemp;

    public GameObject sandbox;
    public GameObject labStarter;

    public AudioClip welcomeAudio;
    public AudioClip placementAudio;
    public AudioClip loginAudio;
    public AudioClip labselectAudio;

    #endregion

    #region Private Variables 
    private List<GameObject> labList;
    List<string> labOptions;
    private string labSelected = "none";
    private string allLabs;

    private bool placed = false;
    private bool playAnimation = true;
    private bool setAnimationAnchor = true;

    private AudioSource aud;

    private int currState = -1;
    private enum state
    {
        placement,
        usr_entry,
        pass_entry,
        authentication,
        lab_selection,
        lab_initiation,
        end_of_states
    }
    #endregion

    #region MonoBehaviour

    // Start is called before the first frame update
    void Start()
    {
        labList = new List<GameObject>();
        StartCoroutine(DownloadFile("http://cyberlearnar.cs.mtsu.edu/show_uploaded/test_names.csv", "Assets/Resources/csv bank/test_names.csv"));
        StartCoroutine(DownloadFile("http://cyberlearnar.cs.mtsu.edu/show_uploaded/crn_to_labs.csv", "Assets/Resources/csv bank/crn_to_labs.csv"));
        StartCoroutine(DownloadFile("http://cyberlearnar.cs.mtsu.edu/labs", "Assets/Resources/csv bank/allLabs.json"));
        allLabs = Resources.Load<TextAsset>("csv bank/allLabs").text;

        aud = GetComponent<AudioSource>();
        StartCoroutine(WaitForClip(welcomeAudio));

        intro.SetActive(true);
        toggleLineRender(false);
    }


    // Update is called once per frame
    void Update()
    {
        // After Start, sets intro position - such that it exists in worldspace and isn't bound to head movemnet
        if (setAnimationAnchor && Camera.main.transform.position != new Vector3(0, 0, 0)) // This took longer than it should've to come up with
        {
            toggleLineRender(false);
            setAnimationAnchor = false;
            intro.transform.position = Camera.main.transform.position + Camera.main.transform.rotation * new Vector3(0, 0, 5);
            intro.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
        }

        // After initial animation, this will initiate placement scene, then the login screen 
        // intro.active throws warning, but don't trust the stinky computer
        if (playAnimation && intro.active && intro.gameObject.transform.GetChild(0).GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            // prevents update from checking if intro is playing every frame
            playAnimation = false;

            // starts the rest of events in motion
            print("Animation at idle; starting placement scene. :)\n");
            next();
        }

        // place the scene object to set the coordinant system
        if (placement_prop.active && !placed)
        {
            // Same as realign() - CONSIDERING REWORK
            anchor.transform.position = controller.transform.position;
            anchor.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
        }
    }
    #endregion

    #region Public Events
    // OnHomeButtonDown() realigns UI to position of controller and angle head is pointing
    public void realign()
    {
        print("Realigning UI.");
        anchor.transform.position = controller.transform.position;
        anchor.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);

        // change orientation of starfield
        // **Starfield disabled for now due to incapatibility**
        // GameObject.Find("Starfield").transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
    }


    // Keep the flow of events involving the Login UI
    public void next()
    {
        print("Current state: " + (state)(++currState) + "\n========================");

        switch (currState)
        {
            case (int)state.placement: // Placement scene: 0
                {
                    StartCoroutine(WaitForClip(placementAudio));
                    
                    // disable linerenderer and MTSU model to look cleaner
                    toggleLineRender(false);
                    intro.gameObject.transform.GetChild(0).gameObject.SetActive(false);

                    // Destroy Lab Selection Options if looped through to beginning - MOVEABLE
                    foreach (GameObject o in labList) { Destroy(o); }

                    // Start placement scene and ensure it hasn't already been anchored before it started 
                    placement_prop.SetActive(true);
                    placed = false;
                    break;
                }

            case (int)state.usr_entry: // User Entry: 1
                {

                    StopCoroutine(WaitForClip(placementAudio));
                    StartCoroutine(WaitForClip(loginAudio));

                    // Cleanup placement object
                    toggleLineRender(true);
                    placement_prop.SetActive(false);

                    // Cleanup failed authentication
                    loading.SetActive(false);
                    print("clearing usernames\n");
                    usr.GetComponent<Dropdown>().options.Clear();

                    // Start UI
                    LoginUI.SetActive(true);
                    usr.SetActive(true);
                    keyboard.SetActive(true);
                    guestButton.SetActive(true);
                    pas.SetActive(false);
                    break;
                }

            case (int)state.pass_entry: // Pass Entry: 2
                {
                    // Disable User entry
                    usr.SetActive(false);

                    // Enable Pass Entry
                    pas.SetActive(true);

                    // Sets the keyboard text box to the Password Text box
                    keyboard.GetComponent<VRKeyboard.Utils.KeyboardManager>().setText(pas.gameObject.transform.GetChild(0).GetComponent<Text>());
                    break;
                }

            case (int)state.authentication: // Authentication: 3
                {
                    // Cleanup in-case authentication fails
                    usr.GetComponent<autofill>().refreshText();
                    keyboard.GetComponent<VRKeyboard.Utils.KeyboardManager>().resetText();

                    // Submit request
                    pas.SetActive(false);
                    keyboard.SetActive(false);
                    guestButton.SetActive(false);
                    loading.SetActive(true);


                    // THE AUTHENTICATION GOES HERE
                    authenticate(usr.transform.GetChild(0).GetComponent<Text>().text, pas.transform.GetChild(0).GetComponent<Text>().text);
                    break;
                }

            case (int)state.lab_selection: // Lab selection: 4
                {
                    StopCoroutine(WaitForClip(loginAudio));
                    StartCoroutine(WaitForClip(labselectAudio));
                    // Disable UI and Keyboard 
                    LoginUI.SetActive(false);
                    keyboard.SetActive(false);
                    labStarter.transform.parent.gameObject.SetActive(false);

                    // Load Modules 
                    lab_options.SetActive(true);
                    setLabs();
                    break;
                }

            case (int)state.lab_initiation: // Insantiate lab: 5
                {
                    // Disable all UI
                   // Debug.Log("doing the case of lab_initialize");
                    lab_options.SetActive(false);

                    // Start Lab Manager
                    labStarter.transform.parent.gameObject.SetActive(true);
                    startLab();
                    break;
                }

            // Catch if looped and extends past defined states 
            // returns back to placement scene
            default:
                {
                    // Disable lab selection
                    lab_options.SetActive(false);
                    print(labSelected);

                    // Loop back to Start
                    gotoState(0);
                    break;
                }
        }
    }


    // Anchors scene to location of controller 
    public void place()
    {
        // Ensures that prop isn't anchored before the placement scene
        if (placement_prop.active && !placed)
        {
            print("Scene has been placed\n");
            placed = true;
            next();
        }
    }


    // Called to change state if the state requested cannot be reached calling next();
    public void gotoState(int state)
    {
        currState = state - 1;
        next();
    }


    // Called to automatically log in as "guest"
    public void guestLogin()
    {
        print("Guest button hit; logging in as Guest user.");
        // Fill in usr/pas perameters so no errors will be thrown 
        // Also disables placeholders so it doesn't look ugly if user didn't type in user or pas perameters and has to retern to start
        usr.transform.GetChild(0).GetComponent<Text>().text = "guest";
        usr.transform.GetChild(1).gameObject.SetActive(false);
        pas.transform.GetChild(0).GetComponent<Text>().text = "guest";
        pas.transform.GetChild(1).gameObject.SetActive(false);

        // Goes through normal authentication after filling in both perameters
        gotoState((int)state.authentication);
    }
    #endregion

    #region Private Events
    // Downloads CSVs before autofill script on usr_dropbox is instantiated 
    IEnumerator DownloadFile(string webpath, string path)
    {
        if (useLocalFiles && System.IO.File.Exists(path))
        {
            print("File: " + path.Substring(path.LastIndexOf("/") + 1) + " exists and will not be pulled from the website.");
            yield return null;
        }

        else
        {
            var uwr = new UnityWebRequest(webpath, UnityWebRequest.kHttpVerbGET);
            uwr.downloadHandler = new DownloadHandlerFile(path);
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
                Debug.LogError(uwr.error);
            else
                Debug.Log("File successfully downloaded and saved to " + path + "\n");
        }

        // This makes sure Media catalogue can download before starting lab
        if (currState == 5) { getJson(path); }
    }



    private void getJson(string path)
    {
        // crop the path 
        path = path.Substring(path.IndexOf("es/") + 3);
        path = path.Substring(0, path.Length - 5);

        string jsonString = Resources.Load<TextAsset>(path).text;
        if (useLocalFiles)   // this was added to override the media file downloads
        {

        }
        else
        {
            LabDataObject LabData = new LabDataObject();
            JsonUtility.FromJsonOverwrite(jsonString, LabData);

            // pass info to Media Catalogue and Lab Manager
            StartCoroutine(Catalogue(LabData));
        }
    }


    IEnumerator WaitForClip(AudioClip aclip)
    {
        aud.clip = aclip;

        if (aud.clip != null)
            aud.Play();
        yield return new WaitForSeconds(aud.clip.length);
      
    }




    IEnumerator Catalogue(LabDataObject LabData)
    {
        labStarter.GetComponent<MediaCatalogue>().enabled = true;
        labStarter.GetComponent<MediaCatalogue>().addToCatalogue(LabData);
        yield return new WaitUntil(() => labStarter.GetComponent<MediaCatalogue>().done);
        StartCoroutine(Start(LabData));
    }

    IEnumerator Start(LabDataObject LabData)
    {
        labStarter.GetComponent<LabManager>().enabled = true;
        labStarter.GetComponent<LabManager>().Initialize(LabData.ActivityModules);
        yield return null;
    }



    // Toggles line renderer emitted from controller
    private void toggleLineRender(bool flag)
    {
        // print("doing the dirty work [|8^(");
        controller.GetComponent<LineRenderer>().enabled = flag;
        controller.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = flag;
    }


    // Calls script in autofill to authenticate based on usr/pas logged 
    private void authenticate(string usr, string pas) {
        if (this.usr.GetComponent<autofill>().authenticate(usr, pas))
            gotoState((int)state.lab_selection);
        else
            gotoState((int)state.usr_entry);

        // One-line alternative
        // gotoState((int)(this.usr.GetComponent<autofill>().authenticate(usr, pas)) ? state.modules : state.usr_entry);
    }


    // Instantiates UP TO 5 lab options that are clickable and load 
    private void setLabs()
    {
        // pull labs as a Dictionary <string lab_name, string jsonUrl> 
        //            username-field   script     method               (username text)
        labOptions = usr.GetComponent<autofill>().getLabs(usr.transform.GetChild(0).GetComponent<Text>().text);

        // Loop through labs pulling up to 5 of them and instantiating an interface to pick one
        int count = 0;
        foreach (string lab in labOptions)
        {
            labList.Add(createLab(lab, getDesc(lab), count++));
            if (count == 5) { break; }
        }

        // Create one last button that allows user to exit application
        labList.Add(createLab("Exit", "Closes program and sends data back to server", count++));
    }

    // Used by SetLabs() to instantiate variants of lab buttons to select lab
    private GameObject createLab(string lab, string description, int count)
    {
        // Create instance and position it
        GameObject newlab = Instantiate(labTemp, lab_options.transform);
        newlab.transform.position += .42f * (count % 2) * anchor.transform.right + new Vector3(0, -.15f * (count / 2), 0);

        // Give each lab a unique value onClick
        newlab.GetComponent<Button>().onClick.AddListener(delegate () { setLab(lab); });

        // Set Lab Title, description, name, and visibility
        newlab.transform.GetChild(0).GetComponent<Text>().text = getName(lab);
        newlab.transform.GetChild(1).GetComponent<Text>().text = description;

        // This makes the Panel name the lab's unique identifier not the title
        newlab.name = lab.Equals("Exit") ? lab : "Lab: " + lab;
        newlab.SetActive(true);
        return newlab;
    }

    // Removes underscores and replaces them with spaces
    private string format(string lab)
    {
        char[] chars = lab.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == '_') { chars[i] = ' '; }
        }
        return new string(chars);
    }


    // Returns lab Name as a string
    private string getName(string labId)
    {
        if (labId.ToUpper().Equals("EXIT")) { return "Exit"; }
        string temp = allLabs;
        try
        {
            temp = allLabs.Substring(allLabs.IndexOf("\"lab_id\":" + labId) + 23 + labId.Length);
            temp = temp.Substring(0, temp.IndexOf("\""));
        }
        catch { print("No Name found on the website for lab: " + labId); temp = "Lab title not found on site"; }
        return temp;
    }


    // Returns lab description as a string
    private string getDesc(string labId)
    {
        string temp = allLabs;
        try
        {
            temp = allLabs.Substring(0, allLabs.IndexOf("\"lab_id\":" + labId) - 2);
            temp = temp.Substring(temp.LastIndexOf("\"lab_description\":\"") + 19);
        }
        catch { print("No description found on the website for lab: " + labId); temp = "Lab description not found on site"; }
        return temp;
    }


    // Called when a lab is clicked on; sets value of (string) labSelected
    private void setLab(string lab)
    {
        labSelected = lab;
        print("Lab Selected: " + format(labSelected) + "\n");
        next();
    }


    // Instantiates the lab selected
    private void startLab()
    {

        //Debug.Log("in startLab =" + labSelected.ToString());
        // If exit is selected from the list, End program here
        if (labSelected.ToUpper().Equals("EXIT"))
        {
          //  print("Exit tab selected; Exiting application");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // If no lab has been selected (forced next()), returns to lab selection
        else if (labSelected.Equals("none"))
        {
            print("No lab selected; returning to lab selection");
            gotoState((int)state.lab_selection);
        }

        else
        {
            // Sandbox while lab is loading
            toggleLineRender(false);
            //sandbox.SetActive(true);

            // THE URL PROVIDED CURRENTLY DOES NOTHING - THIS NEEDS TO BE PROVIDED WEB-SIDE
            string jsonPath = "http://cyberlearnar.cs.mtsu.edu/show_uploaded/moon_lab.json";
            //"http://cyberlearnar.cs.mtsu.edu/show_uploaded/JsonTest.txt"; //"http://cyberlearnar.cs.mtsu.edu/lab_json/ " + labSelected;

            //try { StartCoroutine(DownloadFile(jsonPath, "Assets/Resources/csv bank/LabJson.json")); }
            //catch { print("Cant load Lab list from url"); }

            // Debug.Log(" starting that json get region");
            GameObject go = GameObject.Find("[_DYNAMIC]");
            if (go == null)
            {
                //Debug.Log("no dynamic!");
            }
            {
                go.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
                float yangle = Camera.main.transform.eulerAngles.y;

                float offset = 1.5f;  // distance in front of camera for the scene
                float dx = offset * Mathf.Cos(yangle * Mathf.Deg2Rad);
                float dy = offset * Mathf.Sin(yangle * Mathf.Deg2Rad);
                go.transform.position = Camera.main.transform.position - Vector3.up * 0.2f;
                
                //GameObject light = GameObject.Find("Directional Light");
                //if (light == null) Debug.Log("no light");
                //light.transform.eulerAngles = new Vector3(0.0f, yangle, 0.0f);

                //go.transform.position = Camera.main.transform.position + dx*Vector3.left + -0.4f * Vector3.up + dz* Vector3.forward;
                controller.GetComponent<LineRenderer>().enabled = true;
                controller.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
                go.GetComponent<LabManager>().spawnDemoNew();
            }
            //disable sandbox
            // toggleLineRender(true);
            // sandbox.SetActive(false);
        }
    }
    #endregion
}
