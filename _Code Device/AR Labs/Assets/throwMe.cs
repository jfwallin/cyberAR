using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

// Purpose is to familiarize user with controller by having them do something
public class throwMe : MonoBehaviour
{
    //public GameObject controller;
    private MLInput.Controller controller;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.TriggerValue > .2 && this.GetComponent<BoxCollider>().bounds.Contains(controller.Position))
        {
            this.transform.position = controller.Position;
            this.transform.rotation = controller.Orientation;
            this.transform.eulerAngles += new Vector3(0, 90, 0);
        }
    }
}
