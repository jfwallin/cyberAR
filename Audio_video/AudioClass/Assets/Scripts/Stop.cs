using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stop : MonoBehaviour
{
    public void HandleOnClickEvent()
    {
        //Send a signal to stop audio and video

        string[] AudioT = { "moonphase-intro", "1" };

        gameObject.SendMessage("MediaManager", AudioT);
       // gameObject.SendMessage("StopAudio", -1);
       
       // gameObject.SendMessage("StopV", -1);
    }
}
