using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Question1 : MonoBehaviour
{
    // Start is called before the first frame update
    //AudioPlayer myaudio = new AudioPlayer("1");
    public void HandleOnClickEvent()
    {
        // myaudio.Num = 1;
        //AudioPlayer myaudio = new AudioPlayer("1");
        
        gameObject.SendMessage("getNum", "55");
        gameObject.SendMessage("Pause", 1);

    }
}
