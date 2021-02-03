using UnityEngine;
using UnityEngine.UI;

namespace MCQ
{
    public class AnswerChoice : MonoBehaviour
    {
        #region Variables
        private int answerId = -1;          //Unique answer choice id, made from its index in the array of possible answers.
        private IMCQManager manager = null; //Reference to pass id to when selected/unselected.
        #endregion //Variables

        #region Public Functions
        /// <summary>
        /// Sets class variables and toggle group
        /// </summary>
        /// <param name="answerIndex"></param>
        /// <param name="passedManager"></param>
        /// <param name="tg"></param>
        /// <returns></returns>
        public void Initialize(string optionText, int answerIndex, IMCQManager passedManager, ToggleGroup tg)
        {
            answerId = answerIndex;
            manager = passedManager;
            GetComponent<Toggle>().group = tg;
            if (answerIndex == -1 || manager == null || GetComponent<Toggle>()?.group == null)
            {
                Debug.LogWarning("Could some values on an answer choice were not set correctly");
            }
        }

        public void OnSelectChange(bool selected)
        {
            if (selected)
            {
                manager.OnAnswerSelected(answerId);
            }
            else //not selected
            {
                manager.OnAnswerDeselected(answerId);
            }
        }
        #endregion //Public Functions
    }
}