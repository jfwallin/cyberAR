using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLeapTools;

public class Orbit : MonoBehaviour
{
    //This will control the orbit of a celestial body
    

    //Public Variable
    public string center; //This is what the orbit is centered on.
    public float rotateDegree; //Treat as const
    
    //Private Variable
    private Vector3 offset;
    private GameObject centerObject;

    void Awake()
    {
        centerObject = GameObject.Find(center);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (centerObject == null) //allows for no center
        {
            centerObject = gameObject;
            rotateDegree = 0.0f;
        } 
    }

    private void FixedUpdate() //physics
    {
        offset = transform.position - centerObject.transform.position; //radius of orbit

        Vector3 centerVector = centerObject.transform.position; //get position relative to world 
        transform.position = offset + centerVector;

        transform.RotateAround(centerVector, Vector3.up, rotateDegree * Transmission.GetGlobalFloat("timeMultiplier") * Time.deltaTime);
    }
    
}
