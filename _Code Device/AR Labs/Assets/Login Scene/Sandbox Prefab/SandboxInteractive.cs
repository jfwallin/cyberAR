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

    private Vector3 currRot;
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
       
    }

    void OnTriggerStay(Collider other)
    {
        if (controller.TriggerValue > 0.2f)
        {
            rig.isKinematic = true;
            this.transform.position = controller.Position;
            float touchx, touchy, touchz, 
                x = controller.Touch1PosAndForce.x, 
                y = controller.Touch1PosAndForce.y, 
                z = controller.Touch1PosAndForce.z;
            

            touchx = x * rSpeed % 360;
            touchy = y * rSpeed % 360;
            // if touchforce > 30%, add or subtract z force based on where applied on the y axis
            touchz = (z > .3f ? y >= 0 ? z : -z : 0) * rSpeed % 360;

            this.transform.eulerAngles += new Vector3(touchz, touchx, touchy) * Time.deltaTime;
            flag = true;
        }
        else  { rig.isKinematic = false; }
    }

    void OnTriggerExit(Collider other)
    {
        
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        print(collisionInfo.collider.name);
    }
    //#endif
}
