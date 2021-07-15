using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum InputType { info, Media, Debug}
public class TestWrite : MonoBehaviour
{


   // public string path = "Assets/Resources/test2.txt";
    public void start()
    {
        WriteToString( 0, "null");
    }

    // Main write information menthod
    //Take at least two input up to .... input
    // first input is a enum InputType { info, Media, Debug}
    // info for inprmation such as login information
    // Medai for media inofrmation
    // Debug for debug
    //Second input is string for media name, login name or other
    //Third input is any optional string values 
    public void WriteToString(InputType type, string name, params string[] other )
    {
        string Path = "Assets/Resources/test2.txt";

        // StreamWriter writer = new StreamWriter(Path, true);
        StreamWriter myfile = File.AppendText(Path);
        print("write to String has been called");
       // writer.WriteLine($"inside WriteToFile {name} is {type}");
        
        InputType TYPE = (InputType)type;

        switch (TYPE)
        {
            case InputType.info:
                foreach (var ValeD in other)
                    myfile.WriteLine($"Information {name} is {ValeD}");
                    break;
            case InputType.Debug:
                foreach (var ValeD in other)
                {
                   // if (ValeD != "null")
                    myfile.WriteLine($"Debug type {name} is {ValeD}");
                    // writer.Close;
                }
                break;
            case InputType.Media:
                
                foreach (var ValeD in other)
                {
                  //  if(ValeD != "null")
                        myfile.WriteLine($"Media Type {name} is {ValeD}");
                }
                
                break;
        }
        myfile.Close();
    }
}
