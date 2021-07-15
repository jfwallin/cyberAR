using System.Collections;
using UnityEngine.Networking;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
//Ubon lab terminted file with login information and results
//will be upload to the server
public class LabExit : MonoBehaviour
{
    public string myFile = "Assets/Resources/test2.txt";
    // thi method will be called upon stopping the lab
    void OnApplicationQuit()
    {
        // upload method is called
        StartCoroutine(Upload());
        // time stamp to the log file 
         Debug.Log("Application ending after " + Time.time + " seconds");


    }
    //method used to upload file to the web
    IEnumerator Upload()
    {
        //Convert the file into binary
        //byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
        byte[] txtByte = File.ReadAllBytes("Assets/Resources/test2.txt");
        //create a webForm object
        WWWForm form = new WWWForm();

        //public void AddBinaryData(string fieldName, byte[] contents, string fileName = null, string mimeType = null);
        //this function to upload files and images to a web server application.
        //Note that the data is read from the contents of byte array and not from a file.
        //The fileName parameter is for telling the server what filename to use when saving the uploaded file.
        form.AddBinaryData("file", txtByte, "test2.txt", "txt");
        
        using (UnityWebRequest www = UnityWebRequest.Post("http://cyberlearnar.cs.mtsu.edu/upload_file", form))
        {
            
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }

        }

    }
}
