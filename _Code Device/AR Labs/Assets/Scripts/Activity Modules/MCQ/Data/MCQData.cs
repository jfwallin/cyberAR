using System.Collections;
using System.Collections.Generic;

namespace MCQ
{
    [System.Serializable]
    public class MCQData
    {
        //Media dependency information
        public MediaInfo[] referenceMedia;
        public MediaInfo[] answerCorrectMedia;
        public MediaInfo[] answerIncorrectMedia;

        public string question;                //Question to display

        public bool randomizeOrder;            //Whether to randomize the order of the options
        public bool allowMultiSelect;          //Whether to allow multiple choices to be selected

        public int numberOfOptionsFromPool;    //How many options to add from the pool

        public string[] answerOptions;         //Answer options
        public int[] correctOptionsIndices;      //index of the correct answer options

        //Returns the total number of answer options for this question
        public int TotalNumOptions { get => answerOptions.Length + numberOfOptionsFromPool; }
    }
}