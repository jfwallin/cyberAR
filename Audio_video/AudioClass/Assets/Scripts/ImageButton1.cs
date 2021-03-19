using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageButton1 : MonoBehaviour
{
    public void HandleOnClickEvent()
    {
        
       // gameObject.SendMessage("ImageSwitch", "card_first_quarter_image");
        string[] AudioT = { "card_first_quarter_image", "2" };
        gameObject.SendMessage("MediaManager", AudioT);

    }
}
