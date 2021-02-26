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

public class Bridge 
{
    //ParseJson can be called from outside the class to trigger the methods included here
    public void ParseJsonFromPath(string path)
    {
        makeScene(getInfo(path));
    }

    public void ParseJsonFromString(string data)
    {
        makeScene(JsonUtility.FromJson<ObjectInfoCollection>(data));
    }

    //getInfo serializes an ObjectInfo object from a json at a path
    private ObjectInfoCollection getInfo(string path)
    {
        StreamReader reader = new StreamReader(path);  
        string line;
        ObjectInfoCollection info = new ObjectInfoCollection();

        line = reader.ReadToEnd();
        info = JsonUtility.FromJson<ObjectInfoCollection>(line);
        return info;
    }

    private void makeScene(ObjectInfoCollection info)
    {
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
        GameObject parent = GameObject.Find(obj.parentName);

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
        myObject.transform.parent = parent.transform;

        //This block is removed in Isaac's code and dealt with in the stringJson
        //I can't quite get that working though
        if (obj.material != "")
        {
            Renderer rend = myObject.GetComponent<Renderer>();
            rend.material = Resources.Load<Material>(obj.material); //material must be in a recources folder.
        }
    }


    private void makeTransmissionObject(ObjectInfo obj)
    {
        if (!GameObject.Find(obj.name)) //to keep it from trying to spawn a seccond version. Can't remember if I actually need this. I suspect I don't.
        {
            TransmissionObject myTransObject;
            GameObject myObject;
            GameObject parent = GameObject.Find(obj.parentName);

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
            myObject.transform.parent = parent.transform;

            //This block is removed in Isaac's code and dealt with in the stringJson
            //I can't quite get that working though
            if (obj.material != "")
            {
                Renderer rend = myObject.GetComponent<Renderer>();
                rend.material = Resources.Load<Material>(obj.material); //material must be in a recources folder.
            }
            //I think the issue is here or when I reposition the thing. Need to look at Issac's pong game.
        }
        
    }


    //dealWithType allows us to instantiate various objects.
    private GameObject dealWithType(string type)
    {
        GameObject myObject;

        switch (type){
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
                myObject=GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case "capsule":
                myObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                break;
            case "cylinder":
                myObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;
            default:
                //myObject = new GameObject(); //This line is for if you want the default to be an empty game object

                myObject = GameObject.Instantiate(Resources.Load(type)) as GameObject; //This line is for if you want the default to be loading a prefab
                //Note that the above line requires your prefab to be located in a resources folder.
                break;
        }

        return myObject;
    }
    //This whole method will likely have to change slightly when we start dealing with Transmission.


}
