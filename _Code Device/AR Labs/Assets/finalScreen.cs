using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class finalScreen : MonoBehaviour
{

    public Texture finalSlide;
    public AudioClip finalAudio;
    private GameObject screen;
    private GameObject endbutton;




    // Start is called before the first frame update
    void Start()
    {
        float oscale = 0.2f;
        endbutton = GameObject.Find("endbutton");
        screen = GameObject.Find("display");
        screen.transform.eulerAngles = new Vector3(90.0f, 180.0f, 0.0f);
        screen.transform.localScale = new Vector3(2.0f * oscale, 1.0f * oscale, 1.0f * oscale);
        //gameObjects[i].tag = "sortable";

        screen.GetComponent<Renderer>().material.mainTexture = finalSlide;

        endbutton.GetComponent<Renderer>().material.color = Color.red;
        GameObject.Find("button").GetComponent<Renderer>().material.color = Color.red;



        AudioSource aud = GetComponent<AudioSource>();
        aud.clip = finalAudio;
        aud.Play();


    }

   
}
