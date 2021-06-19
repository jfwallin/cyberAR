using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace sorting
{
    public class buttonCallback : MonoBehaviour
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

            GameObject sorter = GameObject.Find("sortingManager");
            //sorter.GetComponent<sortingData>().hhh();
        }

        private void OnDisable()
        {
            if (enableOnClick)
                _inputReceiver.OnSelected.RemoveListener(HandleOnClick);
            _inputReceiver.OnDragEnd.RemoveListener(HandleOnClick);
        }

        private void HandleOnClick(GameObject sender)
        {
            Debug.Log("clock");
            GameObject sorter = GameObject.Find("sortingManager");
            if (sorter != null)
            {
                if (enableOnClick)
                    sorter.GetComponent<sortingManager>().feedbackOnOrder();
                /*
                 * string typeString = "sortingActivity";
            System.Type type = System.Type.GetType(typeString);
            FindObjectOfType(type).onFeedback();
                 */
            }
            else
                Debug.Log("no sorting object");
        }


    }
}
