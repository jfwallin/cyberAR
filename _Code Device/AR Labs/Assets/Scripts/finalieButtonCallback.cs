using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class finalieButtonCallback : MonoBehaviour
{

    private MagicLeapTools.InputReceiver _inputReceiver;
    public bool enableOnClick = true;

    private void Awake()
    {
        _inputReceiver = GetComponent<MagicLeapTools.InputReceiver>();
        if (_inputReceiver == null)
            Debug.Log("input receiver not found");

    }

    private void OnEnable()
    {
        if (enableOnClick)
            _inputReceiver.OnSelected.AddListener(HandleOnClick);

        //if (enableDragEnd)
        _inputReceiver.OnDragEnd.AddListener(HandleOnClick);

    }

    private void OnDisable()
    {
        if (enableOnClick)
            _inputReceiver.OnSelected.RemoveListener(HandleOnClick);
        _inputReceiver.OnDragEnd.RemoveListener(HandleOnClick);
    }

    private void HandleOnClick(GameObject sender)
    {

        GameObject labmanager = GameObject.Find("Lab Control");
        if (labmanager != null)
        {
            labmanager.GetComponent<LabControl>().finalieDone();
        }
        else
            Debug.Log("no labmanager object");
    }


}
