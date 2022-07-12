using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generalOrbit : MonoBehaviour
{


    public Vector3 moonPosition;
    public string nameOfPlanetBeingOrbited = "Earth";
    public float orbitalPeriod = 10.0f;
    public float orbitScale = 1.0f; // this is a way to scale the orbital size
    public bool synchronousRotation = true;

    public float orbitStartTime = 0.0f;
    public float orbitEndTime = 1e6f;
    public float thetaEnd = 1e6f;
    public float timeRate = 1.0f;


    private float simulationTime = 0.0f;

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
        Vector3 displacement = moonPosition - planet.transform.position;
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
        float xpos, ypos, zpos;
        float currentDistance;
      
        currentDistance = orbitalDistance * orbitScale;
        if (Time.time > orbitStartTime && Time.time < orbitEndTime)
        {


            simulationTime = simulationTime + Time.deltaTime;
            orbitalAngle = orbitalAngle + orbitalRate * Time.deltaTime * timeRate;
            orbitalAngle = orbitalAngle % (2.0f * Mathf.PI);



            xpos = currentDistance * Mathf.Cos(orbitalAngle);
            ypos = 0.0f;
            zpos = currentDistance * Mathf.Sin(orbitalAngle);
            transform.localPosition = new Vector3(xpos, ypos, zpos) + planet.transform.position;

            // if the body is in synchronous rotation (tidally locked to the planet),
            // the same face of the object always faces the planet.  It makes
            // sense to handle that here rather than in a rotation script separtely
            if (synchronousRotation)
            {
                transform.eulerAngles = new Vector3(0.0f, - orbitalAngle * 180.0f / Mathf.PI + 180.0f, 0.0f);
            }
        }

    }

    public void runForSpecifiedTime(float timeInterval, float timeDelay)
    {
        // move the system to a specified angle
        // motion starts after a time delay
        
        // stop the motion
        orbitStartTime = 1e6f;

        // set up the animation
        float tcurrent = Time.time;
        orbitStartTime = tcurrent + timeDelay;
        orbitEndTime = orbitStartTime + timeInterval;
    } 

    public void moveToAngle(float thetaFinal, float timeDelay)
    {
        // move the system to a specified angle
        // motion starts after a time delay
        
        // stop the motion
        orbitStartTime = 1e6f;

        //  find the time to the angle
        float moveTime = findTimeToAngle(thetaFinal);

        runForSpecifiedTime(moveTime, timeDelay);
    }

    public float findTimeToAngle(float thetaFinal)
    {
        // set up the system so it moves to a specific angle

        // make sure we are rotating in the postive direction
        if (thetaFinal < orbitalAngle)
        {
            thetaFinal = thetaFinal + 2.0f * Mathf.PI;
        }

        // find the angle we need to rotate through
        float deltaAngle = thetaFinal - orbitalAngle;

        float timeToAngle = deltaAngle / (orbitalRate * timeRate);
        return timeToAngle;

    }
    
    public void updateSimulation( float tstart, float tend, float newTimeRate = 1.0f)
    {
        orbitStartTime = tstart;
        orbitEndTime = tend;
        timeRate = newTimeRate;
    }


}
