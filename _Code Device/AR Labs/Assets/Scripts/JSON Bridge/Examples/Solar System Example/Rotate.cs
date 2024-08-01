using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLeapTools;
using System.Diagnostics;

public class Rotate : MonoBehaviour
{
    public Vector3 rotationAngle; //treat as a const

    void Update()
    {
        transform.Rotate(rotationAngle * Time.deltaTime);
    }
}
