using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class finalScreen : MonoBehaviour
{

    public Texture finalSlide;
    public AudioClip finalAudio;
    private GameObject endbutton;




    // Start is called before the first frame update
    void Start()
    {
        //float oscale = 0.2f;

        this.GetComponent<Renderer>().material.mainTexture = finalSlide;

        endbutton = GameObject.Find("endApp");
        endbutton.GetComponent<Renderer>().material.color = Color.red;
        GameObject.Find("endbutton").GetComponent<Renderer>().material.color = Color.red;
        endbutton.AddComponent<finalieButtonCallback>();

        AudioSource aud = GetComponent<AudioSource>();
        aud.clip = finalAudio;
        aud.Play();


    }

   
}
