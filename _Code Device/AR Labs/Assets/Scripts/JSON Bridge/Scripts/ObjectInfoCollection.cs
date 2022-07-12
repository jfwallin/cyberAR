using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class creates a JSON object that can be serialized/deserialized easily with Unities built in capabilities
 * One of the things about JSONs that is nice is that not every field/variable has to be filled. We will take advantage of that.
 */

[System.Serializable]
public class ObjectInfoCollection
{
    public ObjectInfo[] objects;
}

/*
 * I'm not convinced this is neccisary
 * Might be better nested inside the big JSON
 */