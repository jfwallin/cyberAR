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
    public string folderPath;

    public ObjectInfoCollection info;

    // Start is called before the first frame update
    void Start()
    {
        string json;
        string path = folderPath + name + ".json";

        StreamWriter writer = new StreamWriter(path);

        json = JsonUtility.ToJson(info, true);
        Debug.Log("json file is: " + json);

        writer.WriteLine(json);

        writer.Close();
    }

}
