using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Question2 : MonoBehaviour
{
    public void HandleOnClickEvent()
    {
        // myaudio.Num = 1;
        //AudioPlayer myaudio = new AudioPlayer("1");
        gameObject.SendMessage("getNum", "22");
        gameObject.SendMessage("Pause", 1);
    }
}
