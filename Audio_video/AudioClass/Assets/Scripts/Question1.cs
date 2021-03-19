using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Question1 : MonoBehaviour
{
    // Start is called before the first frame update
    //AudioPlayer myaudio = new AudioPlayer("1");
    public void HandleOnClickEvent()
    {
        // myaudio.Num = 1;
        //AudioPlayer myaudio = new AudioPlayer("1");
        //create array that hold name of the audio/Video pluse type
        Enum AudioType = MCQ.Audio;
        string[] AudioT = { "new_moon_correct", "0" };
        string[] arr = { "1", "Correct", MCQ.Audio.ToString() };
        gameObject.SendMessage("MediaManager", AudioT, (SendMessageOptions)2);
        gameObject.SendMessage("Pause", 1);
        
        

        
        //gameObject.SendMessage("WriteString", arr , (SendMessageOptions)1);
       // gameObject.SendMessage("ReadString");
    }
}
