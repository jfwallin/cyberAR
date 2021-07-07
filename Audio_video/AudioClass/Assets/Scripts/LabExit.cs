using System.Collections;
using UnityEngine.Networking;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LabExit : MonoBehaviour
{
    public string myFile = "Assets/Resources/test2.txt";
    // Start is called before the first frame update
    void OnApplicationQuit()
    {
        print($"Quit called successfuly");
        StartCoroutine(Upload());
        // StartCoroutine(Upload());
        Debug.Log("Application ending after " + Time.time + " seconds");


    }
    IEnumerator Upload()
    {
        print($"upload called successfuly");
        //byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
        byte[] gifByte = File.ReadAllBytes("Assets/Resources/test2.txt");
        WWWForm form = new WWWForm();
        //form.AddField("myField", "myData");
        //Modify the format according to the long-passed file
        //this function to upload files and images to a web server application.
        form.AddBinaryData("file", gifByte, "test2.txt", "txt");
        print($"test for theform {form}");
        using (UnityWebRequest www = UnityWebRequest.Post("http://cyberlearnar.cs.mtsu.edu/upload_file", form))
        {
            print($"Web server called{www.result}");
            yield return www.SendWebRequest();
            print($"Web server Return value");
            if (www.result != UnityWebRequest.Result.Success)
            {
                print($"Web server Error");
                Debug.Log(www.error);
            }
            else
            {
                print($"Web server Complete");
                Debug.Log("Form upload complete!");
            }
            print($"Web server called end of file {www.result}");
        }

        /*
        print($"upload called successfuly");
        // byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
        //making a dummy xml level file
       // XmlDocument map = new XmlDocument();
       // map.LoadXml("<level></level>");
        //converting the xml to bytes to be ready for upload
       // byte[] levelData = System.Text.Encoding.UTF8.GetBytes(map.OuterXml);

        UnityWebRequest www = UnityWebRequest.Post("http://cyberlearnar.cs.mtsu.edu/upload_file", myFile);
        // UnityWebRequest www = UnityWebRequest.Put("https://www.cs.mtsu.edu/~raltobasei/1170/public", myData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            print($"upload success");
        }
        else
        {
            Debug.Log("Upload complete!");
            print($"No upload");
        }
        */
    }
}
