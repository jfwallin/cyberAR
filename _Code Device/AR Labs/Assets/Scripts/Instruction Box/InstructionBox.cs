using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class InstructionBox : MonoBehaviour
{
    #region Variables
    //Static instance for easy acces
    private static InstructionBox _instance;
    public static InstructionBox Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InstructionBox>();
                if (_instance == null)
                {
                    _instance = Instantiate((GameObject)Resources.Load("Prefabs/Instruction Box"), GameObject.Find("[WORLD]").transform).GetComponent<InstructionBox>();
                }
            }

            return _instance;
        }
    }

    [SerializeField]
    private CanvasGroup cGroup;     //Used to adjust opacity for fading in/out
    [SerializeField]
    private ToggleGroup tGroup;     //Makes sure only one toggle is active at a time
    [SerializeField]
    private List<GameObject> pages; //List of all pages objects
    [SerializeField]
    private List<GameObject> tabs;  //List of all Tab toggle objects

    public bool BillBoard;          //Bool to enable pointing at headpose
    private bool visible;           //Internal state tracking of visibility

    public bool Visible { get => visible; }
    #endregion Variables

    #region Unity Methods
    private void Awake()
    {
        //Make sure there isn't more than one instruction box
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }

        // Fail fast null checks
        if (cGroup == null)
        {
            cGroup = GetComponent<CanvasGroup>();
            Assert.IsNotNull(cGroup, "Could not find canvas group on instruction box");
        }
        if (tGroup == null)
        {
            tGroup = GetComponentInChildren<ToggleGroup>();
            Assert.IsNotNull(tGroup, "Could not find toggle group on instruction box");
        }
        if (pages.Count == 0 || pages[0] == null)
        {
            Debug.LogError("No Pages assigned on instruction box");
        }
        if (tabs.Count == 0 || tabs[0] == null)
        {
            Debug.LogError("No tabs assigned on instruction box");
        }

        //Default to pointing at the main camera
        BillBoard = true;
    }

    void Start()
    {
        //Set the camera for the canvas
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    void Update()
    {
        if (_instance == null)
        {
            Debug.Log($"InstructionBox is null");
        }

        //Point the instructions at the camera if enabled
        if (BillBoard)
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position);
        }
    }
    #endregion Unity Methods

    #region Public Methods
    /// <summary>
    /// Adds a new page and tab to the instruction box
    /// </summary>
    /// <param name="pageTitle">Tab label and page title</param>
    /// <param name="pageContent">What is shown on the page</param>
    /// <param name="show">Whether to show the new page once made</param>
    public void AddPage(string pageTitle, string pageContent, bool show = false)
    {
        //Declare variables to hold references to the text we need to modify
        Text title;
        Text content;
        Text tabLabel;

        if (pages.Count == 1 && pages[0].transform.Find("Content").GetComponent<Text>().text == "No Content Set")
        {
            //There is no pages set yet. Just use the preexisting placeholder tab
            title = pages[0].transform.Find("Title").GetComponent<Text>();
            content = pages[0].transform.Find("Content").GetComponent<Text>();
            tabLabel = tabs[0].transform.GetComponentInChildren<Text>();
        }
        else
        {
            //The first tab is already used, or there's already more than 1 tab, so make a new one.
            GameObject newPage = Instantiate(pages[0], pages[0].transform.parent);
            GameObject newTab = Instantiate(tabs[0], tabs[0].transform.parent);
            newTab.GetComponent<Toggle>().group = tGroup;
            newTab.GetComponent<Toggle>().onValueChanged.AddListener((bool v) => HandleValueChanged(v, pages.Count - 1));
            tabs.Add(newTab);
            pages.Add(newPage);

            title = newPage.transform.Find("Title").GetComponent<Text>();
            content = newPage.transform.Find("Content").GetComponent<Text>();
            tabLabel = newTab.transform.GetComponentInChildren<Text>();
        }

        //Set the text to the correct value
        title.text = pageTitle;
        tabLabel.text = pageTitle;
        content.text = pageContent;

        //Display the new page to the user
        if (show)
        {
            ShowPage(pages.Count - 1);
        }
    }

    /// <summary>
    /// Changes the content of an existing page
    /// </summary>
    /// <param name="pageIndex">Which page, identified from numbering the tabs L to R</param>
    /// <param name="pageTitle">New page title and tab label</param>
    /// <param name="pageContent">New page content</param>
    /// <param name="show">Whether to show the new page once updated</param>
    public void SetPage(int pageIndex, string pageTitle, string pageContent, bool show = false)
    {
        if (pageIndex >= pages.Count)
        {
            Debug.LogWarning("tried to set page contents on a nonexistent page, index too high");
            return;
        }
        if (pageIndex < pages.Count)
        {
            if (pageTitle == "" && pageContent == "")
            {
                RemovePage(pageIndex);
            }
            else
            {
                Text title = pages[pageIndex].transform.Find("Title").GetComponent<Text>();
                Text content = pages[pageIndex].transform.Find("Content").GetComponent<Text>();
                Text tabLabel = tabs[pageIndex].transform.GetComponentInChildren<Text>();

                title.text = pageTitle;
                tabLabel.text = pageTitle;
                content.text = pageContent;

                if (show)
                {
                    ShowPage(pageIndex);
                }
            }
        }
    }

    /// <summary>
    /// Deletes a page and tab, closes instruction box if no tabs are left
    /// </summary>
    /// <param name="pageIndex">Index of the page to remove</param>
    public void RemovePage(int pageIndex)
    {
        if (pageIndex < 0)
        {
            Debug.LogWarning("Page index is non-positive, not removing any page");
            return;
        }
        if (pageIndex >= pages.Count)
        {
            Debug.LogWarning("Tried to remove nonexistent page, index too high");
            return;
        }
        else
        {
            //Unhook event handlers
            tabs[pageIndex].GetComponent<Toggle>().onValueChanged.RemoveAllListeners();

            //Get references to proper text components
            Text title = pages[pageIndex].transform.Find("Title").GetComponent<Text>();
            Text content = pages[pageIndex].transform.Find("Content").GetComponent<Text>();
            Text tabLabel = tabs[pageIndex].transform.GetComponentInChildren<Text>();

            if (pages.Count == 1) //Only one tab
            {
                //Do not delete the last tab. keep it blank to be used as a template for future instantiations
                ToggleVisibility(0);
                title.text = "Tab1";
                content.text = "No Content Set";
                tabLabel.text = "Tab1";
            }
            else if (tabs[pageIndex].GetComponent<Toggle>().isOn) //Multiple tabs, removing the enabled one
            {
                if (pageIndex == 0) //must increment
                {
                    ShowPage(1);
                }
                else // can decrement
                {
                    ShowPage(pageIndex - 1);
                }
                Destroy(pages[pageIndex]);
                Destroy(tabs[pageIndex]);
            }
            else //Multiple tabs, and removing a non-enabled one
            {
                Destroy(pages[pageIndex]);
                Destroy(tabs[pageIndex]);
            }
        }
    }

    /// <summary>
    /// Displays one of the pages
    /// </summary>
    /// <param name="pageIndex">Identifies which page to show, numbered from L to R</param>
    public void ShowPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= pages.Count)
        {
            Debug.LogWarning("invalid page index to show, not showing any page");
            return;
        }

        for (int i = 0; i < pages.Count; i++)
        {
            if (i != pageIndex)
            {
                pages[i].SetActive(false);
            }
            if (i == pageIndex)
            {
                pages[i].SetActive(true);
            }
        }
    }

    /// <summary>
    /// Returns the page index of a specific page title
    /// </summary>
    /// <param name="title">The page title and tab label to find</param>
    /// <returns>index of the page in the UI</returns>
    public int FindPage(string title)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            if (tabs[i].transform.GetComponentInChildren<Text>().text == title)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Fade the instruction box in or out
    /// </summary>
    /// <param name="fadeTime">Time it takes the fade to complete, default is 1 sec</param>
    public void ToggleVisibility(float fadeTime = 1f)
    {
        if (visible)
        {
            StartCoroutine(FadeOut(fadeTime));
            visible = false;
        }
        if (!visible)
        {
            StartCoroutine(FadeIn(fadeTime));
            visible = true;
        }
    }
    #endregion Public Methods

    #region Coroutines
    IEnumerator FadeOut(float length)
    {
        float step = 1 / length;
        for (float val = 1f; val > 0; val -= step)
        {
            cGroup.alpha = val;
            yield return null;
        }
        gameObject.SetActive(false);
    }

    IEnumerator FadeIn(float length)
    {
        float step = 1 / length;
        for (float val = 0; val < 1; val += step)
        {
            cGroup.alpha = val;
            yield return null;
        }
        gameObject.SetActive(true);
    }
    #endregion Coroutines

    #region Event Handlers
    private void HandleValueChanged(bool value, int index)
    {
        if (value)
        {
            ShowPage(index);
        }
    }

    public void HandleDoubleBumper()
    {
        ToggleVisibility();
    }
    #endregion Event Handlers
}