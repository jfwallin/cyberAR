using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageChanger : MonoBehaviour
{
   // create a sprite object
    public Sprite img1;
    public string Name { get; set; }

    // method recieved and image name as string from controle
   public void ImageSwitch(string name)
   {
        //print($"I recieve a message from button1 {name}");
        //concate teh name of the image to the file name in the resource file
        Name = "Image/" + name;
     TestI();
    }
    void TestI()
    {
       // print($"image path and name: {Name}");
       //open image from resource file 
        img1 = Resources.Load<Sprite>(Name);
       //display image to the component "UI image"  that connect to this script
        GetComponent<Image>().sprite = img1;
       // Debug.Log("image script statrted");
    }
    
}
