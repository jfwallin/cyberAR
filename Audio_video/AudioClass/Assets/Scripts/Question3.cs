using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Question3 : MonoBehaviour
{
    public void HandleOnClickEvent()
    {
        // myaudio.Num = 1;
        //AudioPlayer myaudio = new AudioPlayer("1");
        //gameObject.SendMessage("startV", "moonphase-intro.mp4");
        string[] AudioT = { "moonphase-final", "1" };
        
        gameObject.SendMessage("MediaManager", AudioT, (SendMessageOptions.DontRequireReceiver));
        //gameObject.SendMessage("StartV", "moonphase-final");
       // gameObject.SendMessage("Pause", 1);
    }
}
