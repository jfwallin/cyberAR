using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


//This script is temporary!!!!!

//This allows an "easier" way of making the JSON 
//until something better can me created
public class MakeJSON : MonoBehaviour
{

    public string name;
    public string path;
    public bool newJson;

    public ObjectInfoCollection info;

    private string json;

    // Start is called before the first frame update
    void Start()
    {
        
        if (newJson)
        {
            path = path + name + ".json";
        }
        else
        {
            StreamReader reader = new StreamReader(path);
            json = reader.ReadToEnd(); //read into from doc into string

            info = JsonUtility.FromJson<ObjectInfoCollection>(json);
        }
        
    }


    void OnDestroy()
    {
        Debug.Log("in destroy");

        StreamWriter writer = new StreamWriter(path);

        json = JsonUtility.ToJson(info, true);
        Debug.Log("json file is: " + json);

        writer.WriteLine(json);

        writer.Close();
    }
}
