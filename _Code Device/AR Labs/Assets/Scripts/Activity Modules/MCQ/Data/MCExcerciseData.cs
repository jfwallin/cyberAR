using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MCQ
{
    /// <summary>
    /// Contains all the information needed to construct the 
    /// MCQ exercise. 
    /// </summary>
    [System.Serializable]
    public class MCExerciseData : ActivityModuleData
    {
        public MCQData[] questions;       //List of data objects describing each question

        public string[] answerPool;       //List of predefined options to use as wrong answers for questions
    }
}