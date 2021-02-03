using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace MCQ
{
    public enum MediaType { Video, Audio, Image, AudioAndImage, None }

    public class MCQManager : MonoBehaviour, IMCQManager
    {
        #region Variables
        [SerializeField]
        private MCQ.MCExerciseData exerciseData;
        [SerializeField]
        private GameObject answerPrefab = null;           //Prefab for an answer option
        [SerializeField]
        private Button submitButton = null;
        [SerializeField]
        private Button continueButton = null;
        [SerializeField]
        private GameObject answers = null;
        [SerializeField]
        private Text questionText = null;

        private int currentQuestionIndex;
        private MCQData currentQuestionData;
        private List<bool> correctOptions;
        private List<bool> selectedOptions;

        //Dummy class, just to illustrate dependency
        private MediaPlayer  mediaPlayer = null;        
        #endregion //Variables


        public void Initialize(MCExerciseData initData, MediaPlayer mPlayer)
        {
            exerciseData = initData;
            mediaPlayer = mPlayer;
            //Null reference checks

            //if (answerPrefab == null)
            //{
            //    LogU.RNFI("answerPrefab");
            //    answerPrefab = (GameObject)Resources.Load("Answer");
            //    Assert.IsNotNull(answerPrefab);
            //}
            Assert.IsNotNull(answerPrefab);
        }

        /// <summary>
        /// Handles an answer being selected. Sets index to true in selectedOptions
        /// </summary>
        /// <param name="answerID">index of the answer option</param>
        public void OnAnswerSelected(int answerID)
        {
            selectedOptions[answerID] = true;
            submitButton.enabled = true;
        }

        /// <summary>
        /// Handles an answer being deselected. Sets index to false in selectedOptions
        /// </summary>
        /// <param name="answerID"></param>
        public void OnAnswerDeselected(int answerID)
        {
            selectedOptions[answerID] = false;
            //If all elements of the list are false, then:
            if (selectedOptions.TrueForAll(x => !x))
            {
                submitButton.enabled = false;
            }
        }

        /// <summary>
        /// Handles when "submit" is pressed. checks answer for correctness, plays media or
        /// goes to the next question
        /// </summary>
        public void OnSubmitPressed()
        {
            //Check answer
            bool correct = true;
            for(int i = 0; i < correctOptions.Count; i++)
            {
                if(selectedOptions[i] != correctOptions[i])
                {
                    correct = false;
                }
            }

            if (correct)
            {
                //They selected correctly
                if(currentQuestionData.answerCorrectMediaNames[0] != "")
                {
                    //If there is media to play on a correct answer, play it and wait for the callback
                    MediaPlayer.PlayMedia(currentQuestionData.answerCorrectMediaNames, OnMediaPlaybackComplete);

                    //Maybe add other feedback initiated here
                }
                else
                {
                    continueButton.enabled = true;
                }
            }
            else
            {
                //They selected incorrectly
                if(currentQuestionData.answerIncorrectMediaNames[0] != "")
                {
                    //If there is media to play on an incorrect answer, then play it and wait for the callback
                    MediaPlayer.PlayMedia(currentQuestionData.answerIncorrectMediaNames, OnMediaPlaybackComplete);
                    
                    //Maybe add other feedback initiated here
                }
                else
                {
                    continueButton.enabled = true;
                }
            }
        }

        public void OnContinuePressed()
        {
            DisplayNextQuestion(exerciseData.questions[currentQuestionIndex + 1]);
        }

        public void DisplayNextQuestion(MCQData questionData)
        {
            ClearOptions();
             
            currentQuestionIndex++;
            currentQuestionData = exerciseData.questions[currentQuestionIndex];
            selectedOptions = new List<bool>(currentQuestionData.TotalNumOptions);
            correctOptions = new List<bool>(currentQuestionData.TotalNumOptions);
            for(int i = 0; i < currentQuestionData.TotalNumOptions; i++)
            {
                selectedOptions[i] = false;
                correctOptions
            }
            for(int j = 0; j < )
        }

        private void DisplayMedia()
        {
            ;
        }

        public void OnMediaPlaybackComplete()
        {
            ;
        }

        /// <summary>
        /// Tests if two lists have the same elements regardless of order
        /// copied from the top answer at:
        /// https://answers.unity.com/questions/1307074/how-do-i-compare-two-lists-for-equality-not-caring.html
        /// </summary>
        /// <typeparam name="T">type of list</typeparam>
        /// <param name="aListA"></param>
        /// <param name="aListB"></param>
        /// <returns>true if they have the same elements, false otherwise.</returns>
        private bool CompareLists<T>(List<T> aListA, List<T> aListB)
        {
            if (aListA == null || aListB == null || aListA.Count != aListB.Count)
                return false;
            if (aListA.Count == 0)
                return true;
            Dictionary<T, int> lookUp = new Dictionary<T, int>();
            // create index for the first list
            for (int i = 0; i < aListA.Count; i++)
            {
                int count = 0;
                if (!lookUp.TryGetValue(aListA[i], out count))
                {
                    lookUp.Add(aListA[i], 1);
                    continue;
                }
                lookUp[aListA[i]] = count + 1;
            }
            for (int i = 0; i < aListB.Count; i++)
            {
                int count = 0;
                if (!lookUp.TryGetValue(aListB[i], out count))
                {
                    // early exit as the current value in B doesn't exist in the lookUp (and not in ListA)
                    return false;
                }
                count--;
                if (count <= 0)
                    lookUp.Remove(aListB[i]);
                else
                    lookUp[aListB[i]] = count;
            }
            // if there are remaining elements in the lookUp, that means ListA contains elements that do not exist in ListB
            return lookUp.Count == 0;
        }

        /// <summary>
        /// Deletes answer options until there are only 4, then disables
        /// the rest. Shrinks the answer container and the MCQ canvas.
        /// </summary>
        private void ClearOptions()
        {
            //Delete extra objects, down to 4
            if (answers.transform.childCount > 4)
            {
                for(int i = answers.transform.childCount; i > 4; i--)
                {
                    //Destroy the first child object
                    GameObject.Destroy(answers.transform.GetChild(0).gameObject);
                }
            }

            //Disable the remaining objects
            foreach(Transform answer in answers.transform)
            {
                answer.gameObject.SetActive(false);
            }

            //Get RectTransform references
            RectTransform answerRectT = answerPrefab.GetComponent<RectTransform>();
            RectTransform answersContainerRectT = answers.GetComponent<RectTransform>();
            RectTransform canvasRectT = GetComponent<RectTransform>();

            //Shirnk the answers conatainer object
            answersContainerRectT.sizeDelta = new Vector2(answerRectT.rect.width, 0);

            //How much to shrink the canvas
            float heightDelta = answerRectT.rect.height * answers.transform.childCount;
            //Shrink the MCQ canvas
            canvasRectT.sizeDelta = new Vector2(canvasRectT.rect.width, canvasRectT.rect.height - heightDelta);
        }

        /// <summary>
        /// Adds random options from a pool to an array
        /// </summary>
        /// <param name="data">MCQExercise object with the pool</param>
        /// <param name="options">all answer options</param>
        /// <param name="numberOfOptionsToAdd">How many options to add. If this
        ///                                    exceeds the number of options
        ///                                    available in the pool, answers
        ///                                    will be duplicated.</param>
        /// <returns>New array with random options from the pool
        ///          at the end.</returns>
        private string[] AddOptionsFromPool(
            MCExerciseData data,
            string[] options,
            int numberOfOptionsToAdd)
        {
            //Create temporary lists for easier modification
            List<string> outOptions = new List<string>(options);
            List<string> tmpPool = new List<string>(data.answerPool);
            //Create Random number stream
            System.Random rng = new System.Random();

            //Add random options to the end of the array from the pool
            for (int i = numberOfOptionsToAdd; i > 0; i--)
            {
                int randIndex = rng.Next(tmpPool.Count);
                outOptions.Add(tmpPool[randIndex]);
                tmpPool.RemoveAt(randIndex);

                //If we use all the options from the pool, start duplicating
                if(tmpPool.Count == 0)
                {
                    tmpPool.AddRange(data.answerPool);
                }
            }

            return outOptions.ToArray();
        }

        /// <summary>
        /// Randomizes the order of the passed array, and returns the index
        /// of the correct answer in the returned array
        /// </summary>
        /// <param name="options">Answer options, including correct</param>
        /// <param name="correctAnswer">The correct answer, must
        ///                             match one string in options</param>
        /// <returns>int index of correct answer in new array</returns>
        private int RandomizeOptionOrder(
            string[] options,
            string correctAnswer)
        {
            System.Random rng = new System.Random();
            options = options.OrderBy(x => rng.Next()).ToArray();
            int indexOfCorrectAnswer = -1;
            for(int i = 0; i < options.Length; i++)
            {
                if (options[i] == correctAnswer)
                    indexOfCorrectAnswer = i;
            }
            return indexOfCorrectAnswer;
        }

        /// <summary>
        /// Displays answer options by enabling or instantiating anser prefabs
        /// and setting the text to match a string from the argument
        /// </summary>
        /// <param name="options">Array of answer options to display</param>
        private void DisplayOptions(string[] options)
        {
            //Get RectTransform references
            RectTransform answerRectT = answerPrefab.GetComponent<RectTransform>();
            RectTransform answersContainerRectT = answers.GetComponent<RectTransform>();
            RectTransform canvasRectT = GetComponent<RectTransform>();

            //How much to expand the canvas
            float heightDelta = answerRectT.rect.height * options.Length;
            //Expand the canvas
            canvasRectT.sizeDelta = new Vector2(canvasRectT.rect.width, canvasRectT.rect.height + heightDelta);

            //Expand the answer container object
            answersContainerRectT.sizeDelta = new Vector2(answerRectT.rect.width, heightDelta);

            //Enable options, set text, instantiate more if necessary
            for(int i = 0; i < options.Length; i++)
            {
                GameObject answerOption;
                //4, because ClearOptions() leaves 4 disabled options as an
                //answer pool.
                if (i < 4)
                {
                    //If we have fewer options than disabled answer objects, just
                    //enable the ones you need
                    answerOption = answers.transform.GetChild(i).gameObject;
                    answerOption.SetActive(true);
                }
                else
                {
                    //If we need more options than are disabled, instantiate more.
                    answerOption = Instantiate(answerPrefab, answers.transform);
                }
                //Set the answer text with the array
                answerOption.GetComponentInChildren<Text>().text = options[i];
            }
        }
    }
}