using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
//using UnityEditor;

public class WriteToFile : MonoBehaviour
{
  
   //[UnityEditor.MenuItem("Tools/Write file")]
    public void WriteString(string[] array )
    {
        string path = "Assets/Resources/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine($"Question number {(array[0])} is {array[1]}");
        writer.Close();

        //Re-import the file to update the reference in the editor
       // UnityEditor.AssetDatabase.ImportAsset(path);
       // TextAsset asset = (TextAsset)Resources.Load("test");

        //Print the text from the file
       // Debug.Log(asset.text);
    }

 //  [UnityEditor.MenuItem("Tools/Read file")]
   public  void ReadString()
    {
        string path = "Assets/Resources/test.txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }


   
}
