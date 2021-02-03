using System.Collections;
using System.Collections.Generic;

namespace MCQ
{
    [System.Serializable]
    public class MCQData
    {
        //Media dependency information
        public MediaType referenceMediaType;
        public string[] referenceMediaNames;
        public string[] answerCorrectMediaNames;
        public string[] answerIncorrectMediaNames;

        public string question;                //Question to display

        public bool allowMultiSelection;       //Whether to allow multiple choices to be selected
        public bool randomizeOrder;            //Whether to randomize the order of the options

        public int numberOfOptionsFromPool;    //How many options to add from the pool

        public string[] answerOptions;         //Answer options
        public int[] correctOptionsIndex;      //index of the correct answer options

        public int TotalNumOptions { get => answerOptions.Length + numberOfOptionsFromPool; }
        //make multiselect a derived flag from the number of correct answer indices
    }
}