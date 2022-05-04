using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthTester : MonoBehaviour
{
    [SerializeField]
    private string pin = "";
    [SerializeField]
    private bool start = false;
    [SerializeField]
    private Authenticate auth = null;

    void Update()
    {
        if(start)
        {
            testAuthenticate();
            start = false;
        }
    }

    private void testAuthenticate()
    {
        Debug.Log($"Testing Authenticate script with pin of: {pin}");
        Debug.Log($"Authenticated: {auth.AuthenticatePin(pin)}");
        Debug.Log($"Name Associated with pin: {auth.PinToName(pin)}");
        Debug.Log($"mNumber associated with pin: {auth.PinToMNum(pin)}");
    }
}
