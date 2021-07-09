using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleRotation : MonoBehaviour
{

    public float timeRate = 1.0f;
    public float rotationTime = 5.0f;

    private float rotationRate;
    private float rotationTheta;
    private Vector3 originalPlanetScale;

    // Start is called before the first frame update
    void Start()
    {
        rotationRate = Mathf.PI * 2.0f / rotationTime;
        originalPlanetScale = transform.localScale;

    }

    // Update is called once per frame
    void Update()
    {
        rotationTheta = -rotationRate * Time.time * timeRate;
        transform.eulerAngles = new Vector3(0.0f, rotationTheta * 180.0f / Mathf.PI, 0.0f);

    }
}
