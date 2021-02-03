using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MCQ
{
    [System.Serializable]
    public class MCExerciseData
    {
        public string name;
        public MCQData[] questions;

        public MediaType introMediaType;
        public string[] introMediaNames;

        public string[] answerPool;
    }
}