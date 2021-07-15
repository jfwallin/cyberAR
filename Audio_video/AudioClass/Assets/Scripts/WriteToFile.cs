using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class WrtieToFile : WriteAtEnd
{

    //public string path = "Assets/Resources/test2.txt";
   
    
    public override void WriteToString(string type, string name, string info="NULL" , string test="NULL" )
    {
        string Path = "Assets/Resources/test2.txt";
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(Path, true);
       // Console.WriteLine("write to String has been called");
        writer.WriteLine($"inside WriteToFile {name} is {type}");
        writer.Close();
    }

    /*
 //  [UnityEditor.MenuItem("Tools/Read file")]
   public  void ReadString()
    {
        string path = "Assets/Resources/test.txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }

    */
   
}
