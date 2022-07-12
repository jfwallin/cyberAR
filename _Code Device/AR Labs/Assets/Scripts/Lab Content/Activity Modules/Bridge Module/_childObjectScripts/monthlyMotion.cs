using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class monthlyMotion : MonoBehaviour
{

    public GameObject theEarth;
    public GameObject theMoon;
    public GameObject earthCenter;
    public GameObject moonCenter;
    public GameObject earthStarBeam;

    public GameObject forward5days;
    public GameObject backward5days;
    public GameObject forward6hours;
    public GameObject backward6hours;
    public GameObject runButton;
    public GameObject stopButton;
    public GameObject endModuleButton;
    public GameObject textObject;

    public float timeRate = 0.05f;

    public float systemRotationTime = 365.25f;
    private float systemRotationRate;
    private float systemRotationTheta;
   

    public float moonRotationTime = 29.5f; 
    private float moonRotationRate;
    private float moonRotationTheta;

    public float earthRotationTime = 1.0f; 
    private float earthRotationRate;
    private float earthRotationTheta;

    private float synodicRotationTime = 27.3f;
    private float synodicRotationRate;
    private float synodicRotationTheta;

    public float theTime;

    private void onEnable()
    {
        forward5days.GetComponent<MagicLeapTools.InputReceiver>().OnClick.AddListener(f5days);
        backward5days.GetComponent<MagicLeapTools.InputReceiver>().OnClick.AddListener(b5days);
        forward6hours.GetComponent<MagicLeapTools.InputReceiver>().OnClick.AddListener(f5hours);
        backward6hours.GetComponent<MagicLeapTools.InputReceiver>().OnClick.AddListener(b6hours);

        runButton.GetComponent<MagicLeapTools.InputReceiver>().OnClick.AddListener(dorun);
        stopButton.GetComponent<MagicLeapTools.InputReceiver>().OnClick.AddListener(stoprun);
        
        endModuleButton.GetComponent<MagicLeapTools.InputReceiver>().OnClick.AddListener(endModule);
        Debug.Log("sdfsdlsdj");
    }

    private void onDisable()
    {
        forward5days.GetComponent<MagicLeapTools.InputReceiver>().OnClick.RemoveListener(f5days);
        backward5days.GetComponent<MagicLeapTools.InputReceiver>().OnClick.RemoveListener(b5days);
        forward6hours.GetComponent<MagicLeapTools.InputReceiver>().OnClick.RemoveListener(f5hours);
        backward6hours.GetComponent<MagicLeapTools.InputReceiver>().OnClick.RemoveListener(b6hours);

        runButton.GetComponent<MagicLeapTools.InputReceiver>().OnClick.RemoveListener(dorun);
        stopButton.GetComponent<MagicLeapTools.InputReceiver>().OnClick.RemoveListener(stoprun);
        
        endModuleButton.GetComponent<MagicLeapTools.InputReceiver>().OnClick.RemoveListener(endModule);
    }
    

    private void f5days(GameObject sender)
    {
        timeRate = 0.0f;
        theTime = theTime + 5.0f;
    }
    private void b5days(GameObject sender)
    {
        timeRate = 0.0f;
        theTime = theTime - 5.0f;
    }
    private void f5hours(GameObject sender)
    {
        timeRate = 0.0f;
        theTime = theTime + 0.25f;
    }
    private void b6hours(GameObject sender)
    {
        timeRate = 0.0f;
        theTime = theTime - 0.25f;
    }

    private void dorun(GameObject sender)
    {
        timeRate = timeRate + 0.25f;
    }

    private void stoprun(GameObject sender)
    {
        timeRate = 0.0f; 
    }

    private void endModule(GameObject sender)
    {
        onDisable();
    }

    
    // Start is called before the first frame update
    void Start()
    {
        systemRotationRate = 360.0f / systemRotationTime;
        moonRotationRate = 360.0f / moonRotationTime;
        earthRotationRate = 360.0f / earthRotationTime;
        synodicRotationRate = 360.0f / synodicRotationTime;
        onEnable();
    }

    // Update is called once per frame
    void Update()
    {
        theTime = theTime + timeRate;
        
        updatePositions();
    }

    private void updateText()
    {
        string theText;

        theText = theTime.ToString() + " days";
//        textObject.GetComponent<TextMeshPro>().SetText(theText);
//        textObject.GetComponent<TextMeshPro>().SetAllDirty();

    }

    private void updatePositions()
    {
        systemRotationTheta = -systemRotationRate * theTime ;
        transform.localEulerAngles = new Vector3(0.0f, systemRotationTheta, 0.0f);
        
        moonRotationTheta = -moonRotationRate * theTime ;
        earthCenter.transform.localEulerAngles = new Vector3(0.0f, moonRotationTheta, 0.0f);

        earthRotationTheta = -earthRotationRate * theTime;
        theEarth.transform.localEulerAngles = new Vector3(0.0f, earthRotationTheta, 0.0f);

        moonCenter.transform.position = theMoon.transform.position;
        synodicRotationTheta = -synodicRotationRate * theTime;
        moonCenter.transform.localEulerAngles = new Vector3(0.0f, -synodicRotationTheta, 0.0f);
    }
}
