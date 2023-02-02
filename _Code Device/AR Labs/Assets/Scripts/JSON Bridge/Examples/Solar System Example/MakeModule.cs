using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MakeModule : MonoBehaviour
{
    private Bridge bridge = Bridge.Instance;

    public string path;

    // Start is called before the first frame update
    void Start()
    {
        //get json from file at path
        StreamReader reader = new StreamReader(path);
        string line;
        line = reader.ReadToEnd();

        Debug.Log(line); //print out the json
        bridge.ParseJson(line); //make the objects in the JSON in the scene
    }
}
