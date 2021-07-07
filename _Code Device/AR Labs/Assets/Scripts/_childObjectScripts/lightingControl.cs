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

    private Vector3 sunlightPosition;
    private Quaternion sunlightAngle;
    private Vector3 sunlightScale;
    private float sunlightIntensity;

    private float intensity;
    private float bounceIntensity;
    private LightShadows shadows;
    LightType lightType;
    Color sceneColor;

    public void start ()
    {
        theLight = GameObject.Find("Directional Light");
        sceneLight = theLight.GetComponent<Light>();
        saveLights();

        sunlightPosition = new Vector3(100.0f, 0.0f, 0.0f);
        sunlightAngle = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        sunlightScale = new Vector3(3.0f, 3.0f, 3.0f);
        sunlightIntensity = 2.5f;
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
        theLight.transform.position = sunlightPosition;
        theLight.transform.rotation = sunlightAngle;
        theLight.transform.localScale = sunlightScale;

        sceneLight.intensity = sunlightIntensity;
        sceneLight.shadows = LightShadows.None;
        sceneLight.type = LightType.Directional;
        sceneLight.color = Color.white;


    }
}



