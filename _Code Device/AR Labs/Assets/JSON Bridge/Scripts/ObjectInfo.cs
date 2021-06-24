using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

/*
 * This class creates a JSON object that can be serialized/deserialized easily with Unities built in capabilities
 * One of the things about JSONs that is nice is that not every field/variable has to be filled. We will take advantage of that.
 */

[System.Serializable]
public class ObjectInfo
{
    public string name; 
    public string parentName;
    public string type; 

    public Vector3 position;
    public Vector3 scale;
    public string material; //Leaving material blank won't cause any problems and just won't render a material. 
    public bool transmittable = false; //transmission isn't working currently so this should be left as false
    public bool enabled = true; //defaults the true. Set to false if you want objects to be instantiated disabled. 

    public string[] componentsToAdd; //This holds a string version of the component script JSONs
}
