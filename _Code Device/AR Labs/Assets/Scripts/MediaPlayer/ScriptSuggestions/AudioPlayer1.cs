using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Audio Player 
// Recieve inputs as a string fro audio wave that reside in 
// Audio file in Assets/Resources file 
public class AudioPlayer1 : MonoBehaviour
{
    public string Num { get; set; }
    public int PAUSE { get; set; }
    public int STOP { get; set; }
    public AudioSource[] allAudios;
    public AudioPlayer1(string playMe)
    {
        Num = (playMe);
    }
    // This will get a number should change to get the file name
    public void getNum (string number)
    {
        Num = number;
       
       //This will stop all audio before starting other audio
        AudioSource[] allAudios = Camera.main.gameObject.GetComponents<AudioSource>();
        foreach (AudioSource audioS in allAudios)
        {
            audioS.Stop();
        }
        // cal the audio player method
        FileAudioPlayer();
    }
    public void StopAudio(int STOPme)
    {
        // -1 for stop
        STOP = STOPme;
        AudioSource[] allAudios = Camera.main.gameObject.GetComponents<AudioSource>();
        if (STOP == -1)
        {
            foreach (AudioSource audioS in allAudios)
            {
                audioS.Stop();
            }
        }
       

    }
    // This method used to puse and unpause the audio 
    public void Pause(int number)
    {
        // -1 for pause and -2 for un pause
        PAUSE = number;
        AudioSource[] allAudios = Camera.main.gameObject.GetComponents<AudioSource>();
        if (PAUSE == -1)
        {
            foreach (AudioSource audioS in allAudios)
            {
                audioS.Pause();
            }
        }
        else
        {
           
            foreach (AudioSource audioS in allAudios)
            {
                audioS.UnPause();
            }
        }
        //SUGGESTION
        //If you implement what was suggested below, this becomes:
        /// AudioSource audio = GetComponent<AudioSource>();
        /// id(PAUSE == -1)
        ///     audio.Pause();
        /// else
        ///     audio.UnPause();
    }


    //Audio player method
    public void FileAudioPlayer()
    {
        //create audioSource game object
        AudioSource audio = gameObject.AddComponent<AudioSource>();

        //SUGGESTION
        ///AudioSource audio = GetComponent<AudioSource>();
        ///if(!audio)
        ///{
        ///    audio = AddComponent<AudioSource>();
        ///}
        //This doesn't add more than 1 audio source, which performs better, and removes the need to find multiple earlier


        // use switch statment to paly different video
        // This need to be change to recieve the name of the audio
        switch (Num)
        {

            case ("55"):
                
                  
                    audio.PlayOneShot((AudioClip)Resources.Load("Audio/new_moon_correct"));
                   // audio.PlayOneShot((AudioClip)Resources.Load("Audio/"+Num)); // Num could be the name of the Audio


                break;
            case ("22"):
                
                    audio.PlayOneShot((AudioClip)Resources.Load("Audio/new_moon_incorrect"));
                break;
            case ("2"):
               
                    audio.PlayOneShot((AudioClip)Resources.Load("Audio/new_moon_correct"));
                break;
            case ("5"):
                
                    audio.PlayOneShot((AudioClip)Resources.Load("Audio/Test55"));
                break;
            case ("-1"):// This used to stop all audio
                AudioSource[] allAudios = Camera.main.gameObject.GetComponents<AudioSource>();
                foreach (AudioSource audioS in allAudios)
                {
                    audioS.Stop();
                }
                break;

        }
      
    }

   
}
