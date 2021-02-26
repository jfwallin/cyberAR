using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Control : MonoBehaviour
{
    // Start is called before the first frame update
    public void NextScreen()
    {
        SceneManager.LoadScene("StartPage");
    }

}
