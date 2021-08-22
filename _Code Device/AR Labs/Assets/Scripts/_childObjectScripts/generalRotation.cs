using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generalRotation : MonoBehaviour
{

    public float rotationTime = 5.0f;
    public float rotationStartTime = 0.0f;
    public float rotationEndTime = 1e6f;
    public float timeRate = 1.0f;

    private float simulationTime = 0.0f;
    private float rotationRate;
    private float rotationAngle;

    // Start is called before the first frame update
    void Start()
    {
        rotationRate = Mathf.PI * 2.0f / rotationTime;
        simulationTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > rotationStartTime && Time.time < rotationEndTime)
        {
            simulationTime = simulationTime + Time.deltaTime;
            rotationAngle = rotationAngle - rotationRate * Time.time * timeRate;
            rotationAngle = rotationAngle % (2.0f * Mathf.PI);
            transform.eulerAngles = new Vector3(0.0f, rotationAngle * 180.0f / Mathf.PI, 0.0f);
        }
    }

    public void rotateForSpecifiedTime(float timeInterval, float timeDelay)
    {
        // move the system for a specified time 
        // motion starts after a time delay
        
        // stop the motion
        rotationStartTime = 1e6f;

        // set up the animation
        float tcurrent = Time.time;
        rotationStartTime = tcurrent + timeDelay;
        rotationEndTime = rotationStartTime + timeInterval;


    }

    public void moveToAngle(float thetaFinal, float timeDelay)
    {
        // move the system to a specficed angle
        // motion starts after a time delay
        
        // stop the motion
        rotationStartTime = 1e6f;

        //  find the time to the angle
        float moveTime = findTimeToAngle(thetaFinal);

        // set up the animation
        float tcurrent = Time.time;
        rotationStartTime = tcurrent + timeDelay;
        rotationEndTime = rotationStartTime + moveTime;


    }

    public float findTimeToAngle(float thetaFinal)
    {
        // set up the system so it moves to a specific angle

        // make sure we are rotating in the postive direction
        if (thetaFinal < rotationAngle)
        {
            thetaFinal = thetaFinal + 2.0f * Mathf.PI;
        }

        // find the angle we need to rotate through
        float deltaAngle = thetaFinal - rotationAngle;

        float timeToAngle = deltaAngle / (rotationRate * timeRate);
        return timeToAngle;

    }
    
    public void updateSimulation( float tstart, float tend, float newTimeRate = 1.0f)
    {
        rotationStartTime = tstart;
        rotationEndTime = tend;
        timeRate = newTimeRate;
    }
}
