using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class monthlyMotionMod : MonoBehaviour
{

    public GameObject theEarth;
    public GameObject theMoon;
    public GameObject earthCenter;
    public GameObject moonCenter;
    public GameObject earthStarBeam;
    public GameObject textObject;

    public float timeRate = 0.25f;

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


    
    // Start is called before the first frame update
    void Start()
    {
        systemRotationRate = 360.0f / systemRotationTime;
        moonRotationRate = 360.0f / moonRotationTime;
        earthRotationRate = 360.0f / earthRotationTime;
        synodicRotationRate = 360.0f / synodicRotationTime;
    }

    // Update is called once per frame
    void Update()
    {
        theTime = theTime + timeRate;
        
        updatePositions();
        updateText();
    }

    private void updateText()
    {
        string theText;

        int tt;
        tt = (int)(theTime * 10.0f);
        int t1 = tt / 10;
        int t2 = tt - t1 * 10;
        theText = t1.ToString() + "." + t2.ToString() + " days";
        textObject.GetComponent<TextMeshPro>().SetText(theText);
        textObject.GetComponent<TextMeshPro>().SetAllDirty();

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
