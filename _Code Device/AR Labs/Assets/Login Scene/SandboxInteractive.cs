using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class SandboxInteractive : MonoBehaviour
{
    //#if PLATFORM_LUMIN
    [Tooltip("adds random force to objects on start")]
    public float var = 1;
    [Tooltip("Rotation speed multiplier")]
    public float rSpeed = 60;

    private Rigidbody rig;
    private BoxCollider box; 
    
    private MLInput.Controller controller;

    private Vector3 lastPos; 
    private Vector3 lastRot;

    private bool flag = false;
    private bool inside = false;

    // Start is called before the first frame update
    void Start()
    {
        rig = gameObject.GetComponent<Rigidbody>();
        box = gameObject.GetComponent<BoxCollider>();
        controller = MLInput.GetController(MLInput.Hand.Left);

        float flotex = Random.Range(-var, var) + Camera.main.transform.forward.x * var;
        float flotez = Random.Range(-var, var) + Camera.main.transform.forward.z * var;
        // this.transform.position = new Vector3(flotex, Camera.main.transform.position.y, flotez);

    }

    // Update is called once per frame
    void Update()
    {
        var delta = (this.transform.position - lastPos);
        if (flag && !delta.Equals(new Vector3())) {
            print(this.transform.position + " - " + lastPos + " = " + (delta / Time.deltaTime).ToString("G4"));
            rig.velocity = delta / Time.deltaTime;
            flag = true;
        } 
    }

    void LateUpdate()
    {
        lastPos = this.transform.position;
        lastRot = this.transform.eulerAngles;
    }

    void OnTriggerEnter(Collider other)
    {
        inside = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (inside && controller.TriggerValue > 0.2f)
        {
            rig.isKinematic = true;
            this.transform.position = controller.Position;
            float touchx, touchy, touchz;

            touchx = controller.Touch1PosAndForce.x * rSpeed % 360;
            touchy = controller.Touch1PosAndForce.y * rSpeed % 360;
            touchz = controller.Touch1PosAndForce.z * rSpeed * controller.Touch1PosAndForce.z > .5? -1:1 % 360;
            this.transform.eulerAngles += new Vector3(touchz, touchx, touchy) * Time.deltaTime;
            flag = true;
        }
        else  { rig.isKinematic = false; }
    }

    void OnTriggerExit(Collider other)
    {
        inside = false;
    }
    //#endif
}
