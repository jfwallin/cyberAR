using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class checkAngle : MonoBehaviour
{
    public float azmTarget= 45;
    public float azmTolerance = 40;
    public float altTarget = -30;
    public float altTolerance = 30;
    public string audioClipSuccess = null;
    public string[] callBackObjects;

    private GameObject demoObject;
    private MagicLeapTools.PointerReceiver _pointerReceiver;

    private void Awake()
    {
        _pointerReceiver = GetComponent<MagicLeapTools.PointerReceiver>();
        if (_pointerReceiver == null)
            Debug.Log("input receiver not found");
    }

    private void OnEnable()
    {
        _pointerReceiver.OnDragEnd.AddListener(HandleOnDragEnd);
    }

    private void OnDisable()
    {
        _pointerReceiver.OnSelected.RemoveListener(HandleOnDragEnd);
    }

    private void HandleOnDragEnd(GameObject sender)
    {
        Debug.Log("drag end");
        Vector3 sunPosition = GameObject.Find("Sun").transform.position;
        Vector3 myPosition = GameObject.Find("Main Camera").transform.position;
        Vector3 basketballPosition = this.transform.position;

        Vector3 sunDirection = sunPosition - myPosition;
        Vector3 basketballDirection = basketballPosition - myPosition;

        sunDirection = sunDirection / sunDirection.magnitude;
        basketballDirection = basketballDirection / basketballDirection.magnitude;

        float yAngle = Mathf.Atan2(basketballDirection[0], basketballDirection[2]) * Mathf.Rad2Deg;
        float yAngle2 = Mathf.Atan2(sunDirection[0], sunDirection[2]) * Mathf.Rad2Deg;
        float yAngle3 = 360.0f - ((yAngle - yAngle2) + 360.0f) % 360.0f;

        float basketballXZ = Mathf.Sqrt(basketballDirection[0] * basketballDirection[0] + 
            basketballDirection[2] * basketballDirection[2]);

        float alt = Mathf.Atan2(basketballDirection[1], basketballXZ) * Mathf.Rad2Deg;



        float azmDiff = yAngle3 - azmTarget;
        azmDiff = (azmDiff + 180.0f) % 360.0f - 180.0f;

        float altDiff = alt - altTarget;
        altDiff = (altDiff + 180.0f) % 360.0f - 180.0f;

        Debug.Log("angle = " + yAngle3.ToString() + " alt = " + alt.ToString()
            + " az diff=" + azmDiff.ToString() + "  alt diff=" + altDiff.ToString());

        bool inBounds = false;
        if (Math.Abs(altDiff) < altTolerance && Mathf.Abs(azmDiff) < azmTolerance)
            inBounds = true; 

        if (inBounds)
        {
            Debug.Log("in bounds!!!!!");
            StartCoroutine(WaitForClip(3.0f));
        }
        else
        {
            Debug.Log("not in Bounda!!!");
        }
    }


    IEnumerator WaitForClip(float timeDelay)
    {


        AudioSource  aud = gameObject.GetComponent<AudioSource>();

        aud.clip = MediaCatalogue.Instance.GetAudioClip("mixkit-game-bonus-reached-2065");

        if (aud.clip != null)
            aud.Play();

        Debug.Log("TTIME DELAY " + timeDelay.ToString() + "*********************************************************************************************");

        yield return new WaitForSeconds(timeDelay);
        

        for (int i = 0; i < callBackObjects.Length; i++)
        {
            demoObject = GameObject.Find(callBackObjects[i]);
            if (demoObject != null)
            {
                demoObject.GetComponent<demoSequence>().actionCallBack(gameObject);
            }
            else
                Debug.Log("no callback object ->" + callBackObjects[i]);
        }

    }




}
