using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class demoButtonActions: MonoBehaviour
{
    public string[] callBackObjects;
    private GameObject demoObject;
    private MagicLeapTools.InputReceiver _inputReceiver;

    private void Awake()
    {
        _inputReceiver = GetComponent<MagicLeapTools.InputReceiver>();
        if (_inputReceiver == null)
            Debug.Log("input receiver not found");
    }

    private void OnEnable()
    {
        _inputReceiver.OnSelected.AddListener(HandleOnClick);
    }

    private void OnDisable()
    {
        _inputReceiver.OnSelected.RemoveListener(HandleOnClick);
    }

    private void HandleOnClick(GameObject sender)
    {
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
