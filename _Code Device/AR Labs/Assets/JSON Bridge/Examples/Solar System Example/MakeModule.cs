using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MakeModule : MonoBehaviour
{
    private Bridge bridge = new Bridge();

    public string path;

    // Start is called before the first frame update
    void Start()
    {
        StreamReader reader = new StreamReader(path);
        string line;

        line = reader.ReadToEnd();

        Debug.Log(line);
        bridge.ParseJson(line);
    }
}
