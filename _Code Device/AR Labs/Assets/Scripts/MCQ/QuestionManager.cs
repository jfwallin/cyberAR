using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionManager : MonoBehaviour, IQuestionManager
{
    #region Variables
    private Text questionText = null;
    #endregion

    #region Public Methods
    public void OnAnswerSelected(int answerID)
    {
        ;
    }

    public void OnAnswerDeselected(int answerID)
    {
        ;
    }
    #endregion
}
