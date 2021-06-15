using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


//This script allows you to test the Bridge without a outside source.
//You can create the JSON in the inspector and then test if it is working corrrectly in one run in the Unity Editor.
public class MakeObjectInfo : MonoBehaviour
{
    private Bridge bridge = new Bridge();

    public string name;
    public string[] paths;
    
    //public ObjectInfo info;
    //public ObjectInfoCollection info;

    // Start is called before the first frame update
    void Start()
    {
        string[] json = new string[paths.Length];
        int i = 0;
        //path = path + name+ ".json";

        //StreamWriter writer = new StreamWriter(path);
        
        //json = JsonUtility.ToJson(info, true);
        //Debug.Log("json file is: " + json);

        //writer.WriteLine(json);

        //writer.Close();

        //bridge.ParseJsonFromPath(path);


        foreach(string path in paths)
        {
            json[i] = fromPathToString(path);
            Debug.Log("json file is: " + json[i]);
            i++;
        }

        if (json[0] != null) StartCoroutine(ExampleCoroutine(json));
    }

    IEnumerator ExampleCoroutine(string[] json)
    {

        foreach (string obj in json)
        {
            Debug.Log("obj is = " + obj);
            bridge.ParseJson(obj);

            yield return new WaitForSeconds(15);
            bridge.CleanUp(obj);
        }
    }

    private string fromPathToString(string path)
    {
        StreamReader reader = new StreamReader(path);
        string line;

        line = reader.ReadToEnd();
        return line;
    }
}
