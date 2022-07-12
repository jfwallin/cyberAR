using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleOrbit : MonoBehaviour
{


    public Vector3 moonPosition;
    public string nameOfPlanetBeingOrbited = "Earth";
    public float orbitalPeriod = 10.0f;
    public float timeRate = 1.0f;  // this is a way to scale the time of the orbit
    public float orbitScale = 1.0f; // this is a way to scale the orbital size
    public bool synchronousRotation = true;

    private float orbitalDistance;
    private float orbitalRate;
    private float orbitalAngle;
    private GameObject planet;
    
    // Start is called before the first frame update
    void Start()
    {
        // find the object we are orbiting around by name
        planet = GameObject.Find(nameOfPlanetBeingOrbited);
        
        // determine the displacement between the objects and set that as the 
        // orbital distance
        Vector3 displacement = moonPosition - planet.transform.localPosition;
        orbitalDistance = Mathf.Sqrt(Vector3.Dot(displacement, displacement));

        // set the orbital rate and angle
        orbitalRate = Mathf.PI * 2.0f / orbitalPeriod;
        orbitalAngle = Mathf.Atan2(displacement[1], displacement[0]);
    }

    // Update is called once per frame
    void Update()
    {
        animateEarthMoon();
    }


    void animateEarthMoon()
    {
        float orbitTheta;
        float xpos, ypos, zpos;
        float currentDistance;
      
        currentDistance = orbitalDistance * orbitScale;
        orbitTheta = orbitalAngle + orbitalRate * Time.time * timeRate;

        xpos = currentDistance * Mathf.Cos(orbitTheta);
        ypos = 0.0f;
        zpos = currentDistance * Mathf.Sin(orbitTheta);
        //transform.position = new Vector3(xpos, ypos, zpos) + planet.transform.position;
        transform.localPosition = new Vector3(xpos, ypos, zpos) + planet.transform.localPosition;

        // if the body is in synchronous rotation (tidally locked to the planet),
        // the same face of the object always faces the planet.  It makes
        // sense to handle that here rather than in a rotation script separtely
        if (synchronousRotation)
        {
            transform.localEulerAngles = new Vector3(0.0f, -orbitTheta * 180.0f / Mathf.PI + 180.0f, 0.0f);
            //transform.localEulerAngles = new Vector3(0.0f, -orbitTheta * 180.0f / Mathf.PI + 180.0f, 0.0f);
        }


    }
    


}
