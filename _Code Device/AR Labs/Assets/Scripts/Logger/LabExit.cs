using System.Collections;
using UnityEngine.Networking;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
//Ubon lab terminted file with login information and results
//will be upload to the server
//We could use the same could but using a submit botton
public class LabExit : MonoBehaviour
{
    public string myFile = "Assets/Resources/test2.txt";// this need to be cj=hange to be the same file name as the write 
    [HideInInspector]
    public bool uploading = false;
    // thi method will be called upon stopping the lab
    //void OnApplicationQuit()
    //{
    //    // upload method is called
    //    StartCoroutine(Upload());
    //    // time stamp to the log file 
    //     Debug.Log("Application ending after " + Time.time + " seconds and time is" + DateTime.Now.ToString());


    //}

    public void SubmitLog(Action onDoneUploading)
    {
        uploading = true;
        // upload method is called
        StartCoroutine(Upload(onDoneUploading));
        // time stamp to the log file 
        Debug.Log("Application ending after " + Time.time + " seconds and time is" + DateTime.Now.ToString());
    }


    //method used to upload file to the web
    IEnumerator Upload(Action doneUploading)
    {

        //Convert the file into binary
        byte[] txtByte = File.ReadAllBytes(myFile); //("Assets/Resources/test2.txt");
        //create a webForm object
        WWWForm form = new WWWForm();

        //public void AddBinaryData(string fieldName, byte[] contents, string fileName = null, string mimeType = null);
        //this function to upload files and images to a web server application.
        //Note that the data is read from the contents of byte array and not from a file.
        //The fileName parameter is for telling the server what filename to use when saving the uploaded file.
        form.AddBinaryData("file", txtByte, "test"+11+".txt", "txt");

        //// submit file to server
        UnityWebRequest www = UnityWebRequest.Post("http://cyberlearnar.cs.mtsu.edu/upload_file", form);
        yield return www.SendWebRequest();
        uploading = false;
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }

        doneUploading.Invoke();

       
    }
}
