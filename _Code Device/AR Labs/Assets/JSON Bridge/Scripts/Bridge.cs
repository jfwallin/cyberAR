using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using MagicLeapTools;
//using System.Runtime.CompilerServices;
//using System.CodeDom;
using System.Collections.Specialized;
using System.Runtime.Versioning;
using System.Security.Policy;
using System.Runtime.InteropServices;
using UnityEditor;
using TMPro;
public class Bridge
{
    //ParseJson can be called from outside the class to trigger the methods included here
    public void ParseJson(string data)
    {
        makeScene(JsonUtility.FromJson<ObjectInfoCollection>(data));
    }

    public void makeObjects(ObjectInfo[] objectList)
    {
        foreach (ObjectInfo obj in objectList)
        {
            if (obj.transmittable == false) makeObject(obj);
            else if (obj.transmittable == true) makeTransmissionObject(obj);
        }
    }

    private void makeScene(ObjectInfoCollection info)
    {
        //In the event that transmission objects need to be created this will send each to thier apropriet category
        foreach (ObjectInfo obj in info.objects)
        {
            if (obj.transmittable == false) makeObject(obj);
            else if (obj.transmittable == true) makeTransmissionObject(obj);
        }
    }

    //makeObject goes through the json and creates the scene and all conected scripts from it.
    //We are assuming that the scene is set up with the camera, default lighting, and controller already present.
    public void makeObject(ObjectInfo obj)
    {
        GameObject myObject;
        GameObject parent = null;
        TextMeshPro textBox= null;

        // this allow use to modify existing objects in the scene
        myObject = GameObject.Find(obj.name);
        if (myObject != null)
        {
            Debug.Log("found object " + obj.name);
        }
        else
        {
            Debug.Log("creating " + obj.name + " " + obj.type);
            myObject = dealWithType(obj.type); //possibly fixed
            myObject.name = obj.name;

            obj.newPosition = true;
            obj.newScale = true;
            obj.newEulerAngles = true;
            if (obj.parentName != "")
            {
                parent = GameObject.Find(obj.parentName);
                myObject.transform.SetParent(parent.transform);
            }
        }

       /* if (obj.parentName != "")
        {
            parent = GameObject.Find(obj.parentName);
            myObject.transform.SetParent(parent.transform);
        }*/
        //if (obj.parentName != "")
        //{
        //    myObject.transform.parent = parent.transform;
        //}

        // three new key words have been added to the objectInfo class.
        // The keywords allow use to not override the postions, scales, and
        // orientation of an existing object if we don't want to do that.
        if (obj.newPosition)
            myObject.transform.localPosition = obj.position;
        if (obj.newScale)
            myObject.transform.localScale = obj.scale;
        if (obj.newEulerAngles)
            myObject.transform.localEulerAngles = obj.eulerAngles;

        if (obj.componentsToAdd != null)
        {
            //Debug.Log("components to add " + obj.componentsToAdd.Length.ToString());
            for (int i = 0; i < obj.componentsToAdd.Length; i++)
            {
                //Parse once to get the name of the component
                ComponentName cName = JsonUtility.FromJson<ComponentName>(obj.componentsToAdd[i]);
                //Check if the component already exists (ie, the mesh renderer on aprimitive)
                //Debug.Log(i.ToString() + "   " + obj.componentsToAdd[i]);
                //Debug.Log("cname " + cName.name); 
                Component myComp = myObject.GetComponent(Type.GetType(cName.name));
                if (myComp == null)
                {
                    JsonUtility.FromJsonOverwrite(obj.componentsToAdd[i], myObject.AddComponent(Type.GetType(cName.name)));
                }
                else
                {
                    JsonUtility.FromJsonOverwrite(obj.componentsToAdd[i], myComp);
                }
            }
        }

        if (obj.tag != "")
        {
            myObject.tag = obj.tag;
            // allowable options are limited to the Unity defaults and other attributes
            // manually added to the base scene
        }

        

        //myObject.GetComponent<PointerReceiver>().Clicked)


        //This block is removed in Isaac's code and dealt with in the stringJson
        //I can't quite get that working though
        if (obj.material != "" && obj.material != null)
        {
            Debug.Log("length " + obj.material.Length.ToString());
            Debug.Log("in Render   -X" + obj.material + "X");
            Renderer rend = myObject.GetComponent<Renderer>();
            rend.material = Resources.Load<Material>(obj.material); //material must be in a recources folder.
        }

        myObject.SetActive(obj.enabled); //sets the enabled/disabled state of the object


        if (obj.texture != "" )
        {
            Renderer rend = myObject.GetComponent<Renderer>();
            rend.material.mainTexture = Resources.Load<Texture2D>(obj.texture);
        }

        if (obj.textureByURL != "")
        {
            Renderer rend = myObject.GetComponent<Renderer>();
            rend.material.mainTexture = Resources.Load<Texture2D>(obj.texture);
        }

        if (obj.color != null)
        {
            if (obj.color.Length == 4)
            {
                Renderer rend = myObject.GetComponent<Renderer>();
                rend.material.color = new Color( obj.color[0], obj.color[1], obj.color[2], obj.color[3]);
            }
        }



        if (obj.RigidBody!= null)
        {
            Rigidbody mycomp = myObject.GetComponent<Rigidbody>();
            if (mycomp != null)
            {
                // create a new helper class for rigidbody components
                rigidBodyClass rb = new rigidBodyClass();

                // copy the current values from the rigid body component to the helper
                rb.useGravity = mycomp.useGravity;
                rb.isKinematic = mycomp.isKinematic;
                rb.mass = mycomp.mass;
                rb.drag = mycomp.drag;
                rb.angularDrag = mycomp.angularDrag;

                // the constraints for Rigid body use a bit flag system
                // 2 = x constraint
                // 4 = y constraint
                // 8 = z constraint
                // 16 = x rotation constraint
                // 32 = y rotation constraint
                // 64 = z rotation constraint
                // the sum of the variables gives you the constraint, so 2+4 = 6 
                // would constrain x and y
                //rb.xConstraint = ((byte)mycomp.constraints & (1 << pos)) != 0;
                rb.xConstraint = ((byte)mycomp.constraints & (1 << 1)) != 0;
                rb.yConstraint = ((byte)mycomp.constraints & (1 << 2)) != 0;
                rb.zConstraint = ((byte)mycomp.constraints & (1 << 3)) != 0;

                rb.xRotationConstraint = ((byte)mycomp.constraints & (1 << 4)) != 0;
                rb.yRotationConstraint = ((byte)mycomp.constraints & (1 << 5)) != 0;
                rb.zRotationConstraint = ((byte)mycomp.constraints & (1 << 6)) != 0;

                // assign components to the helper

                JsonUtility.FromJsonOverwrite(obj.RigidBody, rb);
                
                // transfer the helper class back to the real rigid body 
                mycomp.useGravity = rb.useGravity;
                mycomp.isKinematic = rb.isKinematic;
                mycomp.mass = rb.mass;
                mycomp.drag = rb.drag;
                mycomp.angularDrag = rb.angularDrag;

                // this is a bit tedeous, but it zeems to work
                mycomp.constraints = RigidbodyConstraints.None;
                if (rb.xConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezePositionX;
                if (rb.yConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezePositionY;
                if (rb.zConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezePositionZ;
                if (rb.xRotationConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezeRotationX;
                if (rb.yRotationConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezeRotationY;
                if (rb.zRotationConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezeRotationZ;


            }


        }
        


        if (obj.PointerReceiver != null) 
        {

            MagicLeapTools.PointerReceiver mycomp = myObject.GetComponent<PointerReceiver>();
            if (mycomp == null)
            {
                Debug.Log("no Pointer Receiver");
            }
            else
            {
                // create a new helper class for rigidbody components
                pointerReceiverClass pr = new pointerReceiverClass();
                pr.draggable = mycomp.draggable;
                pr.kinematicWhileIdle= mycomp.kinematicWhileIdle;
                pr.faceWhileDragging = mycomp.faceWhileDragging;
                pr.matchWallWhileDragging= mycomp.matchWallWhileDragging;
                pr.invertForward= mycomp.invertForward;

                JsonUtility.FromJsonOverwrite(obj.PointerReceiver, pr);

                mycomp.draggable = pr.draggable;
                mycomp.kinematicWhileIdle= pr.kinematicWhileIdle;
                mycomp.faceWhileDragging = pr.faceWhileDragging;
                mycomp.matchWallWhileDragging= pr.matchWallWhileDragging;
                mycomp.invertForward= pr.invertForward;
            }

        }


        if (obj.tmp != null)
        {
            textBox = myObject.GetComponent<TextMeshPro>();
            if (textBox == null)
            {
                Debug.Log("no TextMeshPro");
            }
            else
            {
                textProClass tpc = new textProClass();
                tpc.textField = textBox.text;
                tpc.color = textBox.color;
                tpc.fontSize = textBox.fontSize;
                tpc.wrapText = textBox.enableWordWrapping;
                //Debug.Log(JsonUtility.ToJson(obj));
                //Debug.Log("TTMP  " + obj.tmp);
                JsonUtility.FromJsonOverwrite(obj.tmp, tpc);

                //Debug.Log("2TTMP  " + obj.tmp);
                //Debug.Log("dddddd  " + tpc.textField);

                textBox.SetText(tpc.textField);
                textBox.fontSize = tpc.fontSize;
                textBox.color = tpc.color;
                textBox.enableWordWrapping = tpc.wrapText;
                textBox.SetAllDirty();

            }


        }
        else
        {
            Debug.Log("no meshpro");
        }

        
       textBox = myObject.GetComponent<TextMeshPro>();
       if (textBox != null)
        {
            textBox.SetAllDirty();
        }

    }

    private void makeTransmissionObject(ObjectInfo obj)
    {
        if (!GameObject.Find(obj.name)) //to keep it from trying to spawn a seccond version. Can't remember if I actually need this. I suspect I don't.
        {
            TransmissionObject myTransObject;
            GameObject myObject;
            GameObject parent = null;
            if (obj.parentName != "")
            {
                parent = GameObject.Find(obj.parentName);
            }

            myTransObject = Transmission.Spawn(obj.type, obj.position, Quaternion.Euler(0, 0, 0), obj.scale);
            myTransObject.name = obj.name;
            myObject = myTransObject.gameObject;

            for (int i = 0; i < obj.componentsToAdd.Length; i++)
            {
                //Parse once to get the name of the component
                ComponentName cName = JsonUtility.FromJson<ComponentName>(obj.componentsToAdd[i]);
                //Check if the component already exists (ie, the mesh renderer on aprimitive)
                Component myComp = myObject.GetComponent(Type.GetType(cName.name));
                if (myComp == null)
                {
                    JsonUtility.FromJsonOverwrite(obj.componentsToAdd[i], myObject.AddComponent(Type.GetType(cName.name)));
                    //This is causing problems becuase of AddCompnent
                }
                else
                {
                    JsonUtility.FromJsonOverwrite(obj.componentsToAdd[i], myComp);
                }
            }


            //myObject.transform.position = obj.position;
            //myObject.transform.localScale = obj.scale;
            if (obj.parentName != "")
            {
                myObject.transform.parent = parent.transform;
            }


            //This block is removed in Isaac's code and dealt with in the stringJson
            //I can't quite get that working though
            if (obj.material != "")
            {
                Renderer rend = myObject.GetComponent<Renderer>();
                rend.material = Resources.Load<Material>(obj.material); //material must be in a recources folder.
            }

            if (obj.texture != "")
            {
                Renderer rend = myObject.GetComponent<Renderer>();
                rend.material.mainTexture = Resources.Load<Texture2D>(obj.texture);
            }
            //I think the issue is here or when I reposition the thing. Need to look at Issac's pong game.
        }

    }


    //dealWithType allows us to instantiate various objects.
    private GameObject dealWithType(string type)
    {
        GameObject myObject;

        switch (type)
        {
            case "":
                myObject = new GameObject();
                break;
            case "empty":
                myObject = new GameObject();
                break;
            case "plane":
                myObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                break;
            case "cube":
                myObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case "sphere":
                myObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case "capsule":
                myObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                break;
            case "cylinder":
                myObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;
            default:
                myObject = GameObject.Instantiate(Resources.Load(type, typeof(GameObject)) as GameObject);

                //myObject = (GameObject)Resources.Load(s1) as GameObject;
                //myObject = GameObject.Instantiate(Resources.Load(type)) as GameObject; //This line is for if you want the default to be loading a prefab
                //Note that the above line requires your prefab to be located in a resources folder.
                break;
        }

        return myObject;
    }
    //This whole method will likely have to change slightly when we start dealing with Transmission.

    //a cleanup meathod....hmmm
    public void CleanUp(string data)
    {

        Debug.Log("cleaning up in the bridge");
        //parse string
        ObjectInfoCollection info = JsonUtility.FromJson<ObjectInfoCollection>(data);

        foreach (ObjectInfo obj in info.objects)
        {
            GameObject.Destroy(GameObject.Find(obj.name));
        }
    }


}
