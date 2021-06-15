using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageButton1 : MonoBehaviour
{
    public void HandleOnClickEvent()
    {
        
        gameObject.SendMessage("ImageSwitch", "card_first_quarter_image");
        
    }
}
