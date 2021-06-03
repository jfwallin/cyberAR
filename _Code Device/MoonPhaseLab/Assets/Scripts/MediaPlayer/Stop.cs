using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stop : MonoBehaviour
{
    public void HandleOnClickEvent()
    {
       //Send a signal to stop audio and video
        gameObject.SendMessage("StopAudio", -1);
       
        gameObject.SendMessage("StopV", -1);
    }
}
