using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class creates a JSON object that can be serialized/deserialized easily with Unities built in capabilities
 * One of the things about JSONs that is nice is that not every field/variable has to be filled. We will take advantage of that.
 */

[System.Serializable]
public class ComponentName
{
    public string name;
}

/*This is a helper class we use to add a component without the Bridge knowing what it is.
 * You should never have to do anything with this class.
 */