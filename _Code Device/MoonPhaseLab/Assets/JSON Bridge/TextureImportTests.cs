using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class TextureImportTests : MonoBehaviour
{
    public string filePath; //= "C:\\Users\\Cody\\Documents\\testImage.jpg"; //Replace with own file path
    string webAddress = "https://www.solarsystemscope.com/textures/download/4k_ceres_fictional.jpg";
    GameObject plane;
    GameObject cube;
    GameObject ceres;
    Texture2D localTexture;
    byte[] localData; //Holds a local file's data
    void Start()
    {
        //Create new plane object at runtime and reset position
        //plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        //plane.transform.position = new Vector3(0, 0, 0);

        //Create cube with rigidbody component (This was mainly just me experimenting)
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0, 1, 2);
        //Rigidbody rb = cube.AddComponent<Rigidbody>();
        //rb.mass = 10;

        //Check if file exists on hard drive
        if(File.Exists(filePath))
        {
            //Load desired image into Unity as texture
            localData = File.ReadAllBytes(filePath);
            localTexture = new Texture2D(2, 2);
            localTexture.LoadImage(localData); //LoadImage automatically adjusts size of texture

            //Apply texture to object (cube)
            cube.GetComponent<Renderer>().material.mainTexture = localTexture;
        }

        //Create sphere object, set object position, 
        ceres = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ceres.transform.position = new Vector3(2, 2.5f, 0);
        StartCoroutine(GetWebTexture(webAddress, ceres));

    }

    //Unity manual page for url retrieval: https://docs.unity3d.com/Manual/UnityWebRequest-RetrievingTexture.html

    //Was unable to return anytthing from the function, but passing in the object proved fruitful. Is slow though.
    //Requirements: UnityEngine.Networking namespace, website URL (string), GameObject, called by StartCoroutine()
    IEnumerator GetWebTexture(string url, GameObject myObject)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D webTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            myObject.GetComponent<Renderer>().material.mainTexture = webTexture;
        }
    }
}
