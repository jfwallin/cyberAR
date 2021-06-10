using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class earthCallback : MonoBehaviour
{

    private MagicLeapTools.InputReceiver _inputReceiver;
    public bool enableOnClick = true;
    public bool enableDragEnd = true;
    public GameObject callbackObject;
    public string callBackMessage;

    private void Awake()
    {
        _inputReceiver = GetComponent<MagicLeapTools.InputReceiver>();
        if (_inputReceiver == null)
            Debug.Log("input receiver not found");
        Debug.Log("earthcallback awake ");

    }



    private void OnEnable()
    {
        if (enableOnClick)
            _inputReceiver.OnSelected.AddListener(HandleOnClick);
        if (enableDragEnd)
            _inputReceiver.OnDragEnd.AddListener(HandleDragEnd);

    }

    private void OnDisable()
    {
        if (enableOnClick)
            _inputReceiver.OnSelected.RemoveListener(HandleOnClick);
        if (enableDragEnd)
            _inputReceiver.OnDragEnd.RemoveListener(HandleDragEnd);
    }

    private void HandleOnClick(GameObject sender)
    {


        if (callbackObject != null)
            callbackObject.GetComponent<earthMoon>().HandleOnClick(callBackMessage);
     }

    private void HandleDragEnd(GameObject sender)
    {
        if (callbackObject != null)
            callbackObject.GetComponent<earthMoon>().HandleDragEnd(callBackMessage);

    }



}
