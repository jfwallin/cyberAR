using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageButton2 : MonoBehaviour
{
    // Start is called before the first frame update
    public void HandleOnClickEvent()
    {

        gameObject.SendMessage("ImageSwitch", "card_waxing_gibbous_image");

    }
}
