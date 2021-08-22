/***
 * Author: Yunhan Li
 * Any issue please contact yunhn.lee@gmail.com
 ***/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace VRKeyboard.Utils
{
    public class KeyboardManager : MonoBehaviour
    {
        #region Public Variables
        [Header("User defined")]
        [Tooltip("If the character is uppercase at the initialization")]
        public bool isUppercase = false;
        public int maxInputLength;

        [Header("Text Boxes")]
        public Text std_text_box;

        [Header("Essentials")]
        public Transform keys;
        #endregion

        #region Private Variables
        private Text currText;
        private GameObject currPlaceholder;
        private string Input 
        {
            get { return currText.text; }
            set { currText.text = value; } 
        }
        private Key[] keyList;
        private bool capslockFlag;
        #endregion

        #region Monobehaviour Callbacks
        void Awake()
        {
            keyList = keys.GetComponentsInChildren<Key>();
            resetText();
        }

        void Start()
        {
            foreach (var key in keyList)
            {
                key.OnKeyClicked += GenerateInput;
            }
            capslockFlag = isUppercase;
            CapsLock();
        }
        #endregion

        #region Public Methods
        public void Backspace()
        {
            if (Input.Length > 0)
            {
                Input = Input.Remove(Input.Length - 1);
            }
            else
            {
                return;
            }
        }

        public void Clear()
        {
            Input = "";
        }

        public void CapsLock()
        {
            foreach (var key in keyList)
            {
                if (key is Alphabet)
                {
                    key.CapsLock(capslockFlag);
                }
            }
            capslockFlag = !capslockFlag;
        }

        public void Shift()
        {
            foreach (var key in keyList)
            {
                if (key is Shift)
                {
                    key.ShiftKey();
                }
            }
        }

        public void GenerateInput(string s)
        {
            // Disable current placeholder text
            currPlaceholder.SetActive(false);

            // Add letters to current string
            if (Input.Length > maxInputLength) { return; }
            Input += s;
            
        }

        // Set which text the keyboard is writing to. Also sets the placeholder. 
        public void setText(Text txtbox) 
        {
            currText = txtbox;
            currPlaceholder = currText.gameObject.transform.parent.gameObject.transform.GetChild(1).gameObject;
        }

        // Reset Textbox to the User entry 
        public void resetText()
        {
            setText(std_text_box);
        }
        #endregion
    }
}