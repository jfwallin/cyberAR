using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLeapTools;
using System.Collections.Specialized;

public class SolarSystemLogic : MonoBehaviour
{

    private const string GlobalTimeKey = "timeMultiplier";
    private const string GlobalHoldKey = "holdMultiplier";
    private const string GlobalSpawnedKey = "spawned";

    private ControlInput control;// = GameObject.Find("ControlPointer").GetComponent<ControlInput>();
    private GameObject endPoint;
   
    private void Awake()
    {
        Transmission.SetGlobalFloat(GlobalTimeKey, 1);
        Transmission.SetGlobalFloat(GlobalHoldKey, 1);
        //If the variable doesn't exist yet make it and set it to false
        if (!Transmission.HasGlobalBool(GlobalSpawnedKey))
        {
            Transmission.SetGlobalBool(GlobalSpawnedKey, false);
        }

        control = GameObject.Find("ControlPointer").GetComponent<ControlInput>();
        endPoint = GameObject.Find("PointerCursor");

        control.OnTriggerDown.AddListener(HandleTriggerDown);
        control.OnBumperDown.AddListener(HandleBumperDown);
    }
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    private void HandleBumperDown()//Pauses and playes the time.
    {

        if (Transmission.GetGlobalFloat(GlobalTimeKey) == 0)
        {
            Transmission.SetGlobalFloat(GlobalTimeKey, Transmission.GetGlobalFloat(GlobalHoldKey));
            Debug.Log("The multiplier was already 0");
        }
        else
        {
            Transmission.SetGlobalFloat(GlobalHoldKey, Transmission.GetGlobalFloat(GlobalTimeKey));
            Transmission.SetGlobalFloat(GlobalTimeKey, 0);
        }
    }

    private void HandleTriggerDown()
    {
        if (!Transmission.GetGlobalBool(GlobalSpawnedKey))
        {
            
            GameObject solarSystem = GameObject.Find("System");
            Debug.Log("current positon: " + solarSystem.transform.position);
            solarSystem.transform.position = endPoint.transform.position;
            solarSystem.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            
            //Transmission.Spawn("Sun Earth Moon", endPoint.transform.position, Quaternion.Euler(0, 0, 0), new Vector3(0.25f, 0.25f, 0.25f));

            Transmission.SetGlobalBool(GlobalSpawnedKey, true);
            Debug.Log("Spawning system... spawned = " + Transmission.GetGlobalBool(GlobalSpawnedKey));
        }
        else
        {
            Debug.Log("You should already have a system. spawned = " + Transmission.GetGlobalBool(GlobalSpawnedKey));
        }
    }
    
}
