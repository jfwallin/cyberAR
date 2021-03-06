﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

/*
 * This class creates a JSON object that can be serialized/deserialized easily with Unities built in capabilities
 * One of the things about JSONs that is nice is that not every field/variable has to be filled. We will take advantage of that.
 */


public class rigidBodyClass
{
    public bool isKinematic;
    public bool useGravity;
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

public class pointerReceiverClass
{
    //Public Variables:
    //[Tooltip("Can we drag this?")]
    public bool draggable;
    public bool kinematicWhileIdle;
    public bool faceWhileDragging;
    public bool matchWallWhileDragging;
    public bool invertForward;
}



[System.Serializable]
public class ObjectInfo
{
    public string name = "";
    public string parentName = "[_DYNAMIC]";
    public string type = "";
    public string tag = "";

    public Vector3 position = new Vector3(0f, 0f, 0f);
    public Vector3 eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 scale = new Vector3(1.0f, 1.0f, 1.0f);
    public string material; //Leaving material blank won't cause any problems and just won't render a material. 
    public bool transmittable = false; //transmission isn't working currently so this should be left as false
    public bool enabled = true; //defaults the true. Set to false if you want objects to be instantiated disabled. 
    public string texture = "";
    public string textureByURL = "";
    public float[] color ;
    public string RigidBody;
    public string PointerReceiver;

    public string[] componentsToAdd; //This holds a string version of the component script JSONs

    // used to override the positions and scales of existing objects
    public bool newPosition = true;
    public bool newScale = true;
    public bool newEulerAngles = true;
}
