using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Question2 : MonoBehaviour
{
    public void HandleOnClickEvent()
    {
        // myaudio.Num = 1;
        //AudioPlayer myaudio = new AudioPlayer("1");
       // System.Action myCall;
        string[] AudioT = { "new_moon_incorrect", "0" };
        gameObject.SendMessage("MediaManager", AudioT, (SendMessageOptions)2);
      //  gameObject.SendMessage("Pause", 1);
       // string[] arr = { "1", "Wrong", MCQ.Image.ToString()};
        //gameObject.SendMessage("WriteString", arr, (SendMessageOptions)2);
    }
}
