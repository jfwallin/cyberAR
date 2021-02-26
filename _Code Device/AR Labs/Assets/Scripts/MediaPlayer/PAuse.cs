using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PAuse : MonoBehaviour
{
    public static int Flage = 1;
    public void HandleOnClickEvent()
    {
        // Send a signal to pause or unpause the audio
       
        if (Flage == 0)
        {
            gameObject.SendMessage("Pause", -2);
            Flage = 1;
        }
        else
        {
            gameObject.SendMessage("Pause", -1);
            Flage = 0;
        }
    }
}
