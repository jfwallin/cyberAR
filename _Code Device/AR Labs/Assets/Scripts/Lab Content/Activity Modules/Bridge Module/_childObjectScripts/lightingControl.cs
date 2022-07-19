using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightingControl : MonoBehaviour 
{


    //Instance field
    private static lightingControl _instance;
    public static lightingControl Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<lightingControl>();
            }

            return _instance;
        }
    }


    //Componenet References
    private GameObject theLight;
    private Light sceneLight;

    private Vector3 position;
    private Quaternion angle;
    private Vector3 scale;

    private Vector3 sunlightPosition = new Vector3(100.0f, 0.0f, 0.0f);
    private Quaternion sunlightAngle = Quaternion.Euler(0.0f, 90.0f, 0.0f);
    private Vector3 sunlightScale = new Vector3(3.0f, 3.0f, 3.0f);
    private float sunlightIntensity = 2.5f;

    private float intensity;
    private float bounceIntensity;
    private LightShadows shadows;
    LightType lightType;
    Color sceneColor;

    public void Awake ()
    {
        //Singleton Management, delete self if another media player exists.
        if(_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else
        {
            _instance = this;
        }

        theLight = GameObject.Find("Directional Light");
        sceneLight = theLight.GetComponent<Light>();
        saveLights();
    }

    public void setSunlight(Vector3 sunP, Quaternion sunQ, Vector3 sunS, float sunI)
    {
        sunlightPosition = sunP;
        sunlightAngle = sunQ;
        sunlightScale = sunS;
        sunlightIntensity = sunI;
    }

    public void saveLights()
    {
        LabLogger.Instance.InfoLog(
            this.GetType().ToString(),
            "Trace",
            "saveLights()");
        //sceneLight.color = Color.white;
        position = theLight.transform.position;
        angle = theLight.transform.rotation;
        scale = theLight.transform.localScale;

        intensity = sceneLight.intensity;
        bounceIntensity = sceneLight.bounceIntensity;
        shadows = sceneLight.shadows;
        lightType = sceneLight.type;
        sceneColor = sceneLight.color;

    }

    public void restoreLights()
    {
        LabLogger.Instance.InfoLog(
            this.GetType().ToString(),
            "Trace",
            "restoreLights()");
        theLight.transform.position = position;
        theLight.transform.rotation = angle;
        theLight.transform.localScale = scale;

        sceneLight.intensity = intensity;
        sceneLight.shadows = shadows;
        sceneLight.type = lightType;
        sceneLight.color = sceneColor;

    }


    public void sunlight()
    {
        LabLogger.Instance.InfoLog(
            this.GetType().ToString(),
            "Trace",
            "sunlight()");
        theLight.transform.localPosition = sunlightPosition;
        theLight.transform.localRotation = sunlightAngle;
        theLight.transform.localScale = sunlightScale;

        sceneLight.intensity = sunlightIntensity;
        sceneLight.shadows = LightShadows.None;
        sceneLight.type = LightType.Directional;
        sceneLight.color = Color.white;


    }
}



