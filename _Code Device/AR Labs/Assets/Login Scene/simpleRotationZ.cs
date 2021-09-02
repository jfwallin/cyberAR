using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleRotationZ : MonoBehaviour
{
    public float rotationPerFrame = 10.0f;

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0.0f, 0.0f, rotationPerFrame);
    }
}
