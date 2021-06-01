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
    public class MCExerciseData
    {
        public string name;               //Name of this exercise
        public MCQData[] questions;       //List of data objects describing each question

        public MediaType introMediaType;  //Video, audio, image, etc
        public string[] introMediaNames;  //Filename of the intro media to be played

        public string[] answerPool;       //List of predefined options to use as wrong answers for questions
    }
}