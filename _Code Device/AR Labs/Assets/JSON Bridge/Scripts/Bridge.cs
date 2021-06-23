using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MagicLeapTools;
using System.Runtime.CompilerServices;
using System.CodeDom;
using System;
using System.Collections.Specialized;
using System.Runtime.Versioning;
using System.Security.Policy;
using System.Runtime.InteropServices;
using UnityEditor;

public class rbClass
{
    public bool isKinematic = false;
    public bool useGravity = false;
    public float mass;
    public float drag;
    public float angularDrag;

    public bool xConstraint;
    public bool yConstraint;
    public bool zConstraint;
    public bool xRotationConstraint;
    public bool yRotationConstraint;
    public bool zRotationConstraint;

}
public class Bridge
{
    //ParseJson can be called from outside the class to trigger the methods included here
    public void ParseJson(string data)
    {
        makeScene(JsonUtility.FromJson<ObjectInfoCollection>(data));
    }


    private void makeScene(ObjectInfoCollection info)
    {
        //In the event that transmission obects need to be created this will send each to thier apropriet category
        foreach (ObjectInfo obj in info.objects)
        {
            if (obj.transmittable == false) makeObject(obj);
            else if (obj.transmittable == true) makeTransmissionObject(obj);
        }
    }

    //makeObject goes through the json and creates the scene and all conected scripts from it.
    //We are assuming that the scene is set up with the camera, default lighting, and controller already present.
    private void makeObject(ObjectInfo obj)
    {
        GameObject myObject;
        GameObject parent = null;
        if (obj.parentName != "")
        {
            parent = GameObject.Find(obj.parentName);
        }

        myObject = dealWithType(obj.type); //possibly fixed
        myObject.name = obj.name;

        for (int i = 0; i < obj.componentsToAdd.Length; i++)
        {
            //Parse once to get the name of the component
            ComponentName cName = JsonUtility.FromJson<ComponentName>(obj.componentsToAdd[i]);
            //Check if the component already exists (ie, the mesh renderer on aprimitive)
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


        myObject.transform.position = obj.position;
        myObject.transform.localScale = obj.scale;
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
            Debug.Log("texture " + obj.texture);
            Renderer rend = myObject.GetComponent<Renderer>();
            rend.material.mainTexture = Resources.Load<Texture2D>(obj.texture);
        }

        if (obj.textureByURL != "")
        {
            Debug.Log("textureByURL " + obj.textureByURL);
            Renderer rend = myObject.GetComponent<Renderer>();
            rend.material.mainTexture = Resources.Load<Texture2D>(obj.texture);
        }

        Debug.Log("rb length =" + obj.RigidBody.Length.ToString());

        if (obj.RigidBody.Length > 0)
        {

            Debug.Log("rb string = " + obj.RigidBody);
            Rigidbody mycomp = myObject.GetComponent<Rigidbody>();
            if (mycomp == null)
            {
                Debug.Log("no rigidbody");
            }
            else
            {
                // create a new helper class for rigidbody components
                rbClass rbTest = new rbClass();

                // copy the current values from the rigid body component to the helper
                rbTest.useGravity = mycomp.useGravity;
                rbTest.isKinematic = mycomp.isKinematic;
                rbTest.mass = mycomp.mass;
                rbTest.drag = mycomp.drag;
                rbTest.angularDrag = mycomp.angularDrag;

                // the constraints for Rigid body use a bit flag system
                // 2 = x constraint
                // 4 = y constraint
                // 8 = z constraint
                // 16 = x rotation constraint
                // 32 = y rotation constraint
                // 64 = z rotation constraint
                // the sum of the variables gives you the constraint, so 2+4 = 6 
                // would constrain x and y
                //rbTest.xConstraint = ((byte)mycomp.constraints & (1 << pos)) != 0;
                rbTest.xConstraint = ((byte)mycomp.constraints & (1 << 1)) != 0;
                rbTest.yConstraint = ((byte)mycomp.constraints & (1 << 2)) != 0;
                rbTest.zConstraint = ((byte)mycomp.constraints & (1 << 3)) != 0;
                
                rbTest.xRotationConstraint = ((byte)mycomp.constraints & (1 << 4)) != 0;
                rbTest.yRotationConstraint = ((byte)mycomp.constraints & (1 << 5)) != 0;
                rbTest.zRotationConstraint = ((byte)mycomp.constraints & (1 << 6)) != 0;

                mycomp.constraints = RigidbodyConstraints.FreezePosition;

                // loop over the components in the json image and assign them to the helper
                for (int i = 0; i < obj.RigidBody.Length; i++)
                {
                    Debug.Log(">>>>   " + i.ToString() + " | " + obj.RigidBody[i]);
                    JsonUtility.FromJsonOverwrite(obj.RigidBody[i], rbTest);
                }

                // transfer the helper class back to the real rigid body 
                mycomp.useGravity = rbTest.useGravity;
                mycomp.isKinematic = rbTest.isKinematic;
                mycomp.mass = rbTest.mass;
                mycomp.drag = rbTest.drag;
                mycomp.angularDrag = rbTest.angularDrag;


                mycomp.constraints = RigidbodyConstraints.None;
                if (rbTest.xConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezePositionX;
                if (rbTest.yConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezePositionY;
                if (rbTest.zConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezePositionZ;
                if (rbTest.xRotationConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezeRotationX;
                if (rbTest.yRotationConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezeRotationY;
                if (rbTest.zRotationConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezeRotationZ;

            }
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
                Debug.Log("texture " + obj.texture);
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
                Debug.Log("prefab name " + type);
                type = "Prefabs/" + type;
                Debug.Log("2  prefab name " + type);
                string s1;
                s1 = "Prefabs/moveableSphere.prefab";
                s1 = "Prefabs/moveableSphere";
                s1 = "Assets/Prefabs/demoPrefab";
                s1 = "C:/Users/jfwal/OneDrive/Documents/GitHub/cyberAR/_Code Device/AR Labs/Assets/" +
                    "Assets/Prefabs/demoPrefab";
                s1 = "C:/Users/jfwal/OneDrive/Documents/GitHub/cyberAR/_Code Device/AR Labs/Assets/" +
                    "Prefabs/moveableSphere";
                s1 = "moveableSphere";

                Debug.Log("2  prefab name " + s1);
                myObject = GameObject.Instantiate(Resources.Load(s1) as GameObject);
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
        //parse string
        ObjectInfoCollection info = JsonUtility.FromJson<ObjectInfoCollection>(data);

        foreach (ObjectInfo obj in info.objects)
        {
            GameObject.Destroy(GameObject.Find(obj.name));
        }
    }


}
