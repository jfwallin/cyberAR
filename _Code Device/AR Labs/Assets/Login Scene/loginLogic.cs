using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class loginLogic : MonoBehaviour
{
    #region Public Variables
    public GameObject LoginUI; 
    public GameObject usr;
    public GameObject pas;
    public GameObject loading;
    public GameObject intro;
    public GameObject keyboard;
    public GameObject anchor; 
    public GameObject placement_prop;
    public GameObject controller;
    public GameObject lab_options;
    public GameObject guestButton;
    public GameObject labTemp;
    #endregion

    #region Private Variables 
    private List<GameObject> labList;
    Dictionary<string, string> labOptions;
    private string labSelected = "none";
    private bool placed = false;
    private bool playAnimation = true;
    private bool setAnimationAnchor = true;
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
            intro.transform.eulerAngles = new Vector3(0,Camera.main.transform.eulerAngles.y,0);
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
    public void realign()  // TODO I WANT TO FIX THIS RUNNING OUT OF TIME THO :(
    {
        print("Realigning UI.");
        anchor.transform.position = controller.transform.position;
        anchor.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);

        // Alternative to align closer to the body - UNTESTED
        // anchor.transform.position = new Vector3((controller.transform.position.x + Camera.main.transform.position.x)/2, controller.transform.position.y,(controller.transform.position.x + Camera.main.transform.position.x)/2);

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
                    // Disable UI and Keyboard 
                    LoginUI.SetActive(false);
                    keyboard.SetActive(false);

                    // Load Modules 
                    lab_options.SetActive(true);
                    setLabs();
                    break;
                }

            case (int)state.lab_initiation: // Insantiate lab: 5
                {
                    // Disable all UI
                    lab_options.SetActive(false);

                    // Start Lab Manager
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
        var uwr = new UnityWebRequest(webpath, UnityWebRequest.kHttpVerbGET);
        uwr.downloadHandler = new DownloadHandlerFile(path);
        yield return uwr.SendWebRequest();
        if (uwr.result != UnityWebRequest.Result.Success)
            Debug.LogError(uwr.error);
        else
            Debug.Log("File successfully downloaded and saved to " + path + "\n");
    }


    // Toggles line renderer emitted from controller
    private void toggleLineRender(bool flag)
    {
        // print("doing the dirty work [|8^(");
        controller.GetComponent<LineRenderer>().enabled = flag;
        controller.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = flag;
    }


    // Called to change state if the state requested cannot be reached calling next();
    private void gotoState(int state)
    {
        currState = state-1;  
        next();
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


    // Instantiates UP TO 6 lab options that are clickable and load 
    private void setLabs()
    {
        // pull labs as a Dictionary <string lab_name, string jsonUrl> 
        //            username-field   script     method               (username text)
        labOptions = usr.GetComponent<autofill>().getLabs(usr.transform.GetChild(0).GetComponent<Text>().text);

        // Loop through labs pulling up to 5 of them and instantiating an interface to pick one
        int count = 0;
        foreach (string lab in labOptions.Keys)
        {
            labList.Add(createLab(lab, getDesc(labOptions[lab]), count++));
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
        newlab.transform.GetChild(0).GetComponent<Text>().text = format(lab); //format doesn't work here but it really doesn't matter
        newlab.transform.GetChild(1).GetComponent<Text>().text = description;
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


    // Returns lab description as a string  TODO
    private string getDesc(string jsonUrl)
    {
        // string temp = jsonUrl.Substring(jsonUrl.indexOf("Description: {"));
        // return jsonUrl.Substring(temp, temp.indexOf("}"));

        return jsonUrl;
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
        // If exit is selected from the list, End program here
        if (labSelected.ToUpper().Equals("EXIT")) 
        {
            print("Exit tab selected; Exiting application");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        } 

        // If no lab has been selected (forced next()), returns to lab selection - TODO FOR SOME REASON, THIS IS CALLED BEOFRE A LAB IS SELECTED
        else if (labSelected.Equals("none")) 
        {
            print("No lab selected; returning to lab selection");
            gotoState((int)state.lab_selection); 
        }

        else
        {
            print("Lab selected: " + format(labSelected) + ", but no info to load for now :^)\n");
            var manifestPath = "http://cyberlearnar.cs.mtsu.edu/lab_manifest/" + labSelected;
            var jsonPath = labOptions[labSelected];
            // media downloader here. 

            // pass in Lab Object
            // GameObject.Find("LabManager").GetComponent<LabManagerScript>().startLab(manifestPath,jsonPath);
            // GameObject.Find("LabManager").GetComponent<LabManager>().Initialize(LabDataObject);
        }
    }
#endregion
}
