/* Based on a post on a Unity question thread 
 * https://answers.unity.com/questions/1164022/move-a-2d-item-in-a-circle-around-a-fixed-point.html
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class revolve : MonoBehaviour
{

    public float RotateSpeed = 5f;
    public float Radius = 0.1f;

    private float _angle;
    private Vector3 scale;

    private void Start() { scale = transform.parent.localScale; }
    private void Update()
    {

        _angle += RotateSpeed * Time.deltaTime;

        var offset = new Vector3(Mathf.Cos(_angle) * scale.x, 0, Mathf.Sin(_angle) * scale.z) * Radius;
        transform.position = transform.parent.gameObject.transform.position + offset;
    }



}