using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


//This script allows you to test the Bridge without a outside source.
public class ExampleSpawner : MonoBehaviour
{
    private Bridge bridge = Bridge.Instance;

    public string[] paths; 

    // Start is called before the first frame update
    void Start()
    {
        string[] json = new string[paths.Length];
        int i = 0;

        foreach(string path in paths)
        {
            json[i] = fromPathToString(path);
            Debug.Log("json file is: " + json[i]);
            i++;
        }

        if (json[0] != null) StartCoroutine(ExampleCoroutine(json));
    }

    //coroutine is used so that the wait funtion works
    IEnumerator ExampleCoroutine(string[] json)
    {

        foreach (string obj in json)
        {
            Debug.Log("obj is = " + obj); //print out json
            bridge.ParseJson(obj); //make the objects in the JSON in the scene

            yield return new WaitForSeconds(15);
            bridge.CleanUp(obj); //remove the objects in the JSON from the scene
        }
    }

    //gets a string of a file at a path
    private string fromPathToString(string path)
    {
        StreamReader reader = new StreamReader(path);
        string line;

        line = reader.ReadToEnd();
        return line;
    }
}
