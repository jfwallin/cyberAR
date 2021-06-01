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

    /// <summary>
    /// The MCQManager is the tope level controller of the MCQ module
    /// It is responsible for managing all of the UI elements for the 
    /// question. It creates answer options, randomizes them, displays them,
    /// and monitors their selection states. It also manages requesting
    /// media playback from the media player. Lastly, It manages question
    /// grading and answer feedback.
    /// </summary>
    public class MCQManager : MonoBehaviour, IMCQManager
    {
        #region Variables
        private AudioPlayer aPlayer = null;         //Reference to the object that plays all media
        //[SerializeField]
        private MCExerciseData exerciseData;        //Conatians answer choices, correct answers, and question text
        [SerializeField]
        private GameObject answerPrefab = null;     //Prefab for an answer option
        [SerializeField]
        private Button submitButton = null;         //Button that submits the currently selected answer
        [SerializeField]
        private Button continueButton = null;       //Button that moves to the next question
        [SerializeField]
        private Button finishButton = null;         //Reference to the button pressed to conclude the module
        [SerializeField]
        private GameObject answers = null;          //Gameobject that holds all the answers as child objects
        [SerializeField]
        private Text questionText = null;           //Title of the question, what is being asked

        private int currentQuestionIndex;           //Keeps track of what question from the exerciseData we are on
        private MCQData currentQuestionData;        //Has all the data needed for the current question
        private string[] currentAnswerPool;         //Answer pool for current exercise, can be null
        private List<bool> correctOptions;          //Flag for each answer choice, if it is correct or not
        private List<bool> selectedOptions;         //Flag for each answer choice, if it is selected or not
        #endregion //Variables

        #region Public Functions
        /// <summary>
        /// Sets up the MCQ exercise, checks references, and begins the questions
        /// </summary>
        /// <param name="initData">The data object containing all the information for the exercise</param>
        /// <param name="mPlayer">Reference to the media player that will play media for the question</param>
        public void Initialize(MCExerciseData initData, AudioPlayer player)
        {
            Debug.Log("Initialize called on the MCQManager");
            //Set necessary references
            exerciseData = initData;
            aPlayer = player;

            //Initilize some internal state
            currentQuestionIndex = 0;
            currentQuestionData = exerciseData.questions[currentQuestionIndex];
            currentAnswerPool = exerciseData.answerPool;

            //Fail fast assertions
            Assert.IsNotNull(exerciseData);
            Assert.IsNotNull(answerPrefab);
            Assert.IsNotNull(submitButton);
            Assert.IsNotNull(continueButton);
            Assert.IsNotNull(finishButton);
            Assert.IsNotNull(answers);
            Assert.IsNotNull(questionText);

            //Set state of some dependencies.
            submitButton.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(false);
            finishButton.gameObject.SetActive(false);
            questionText.text = "";

            //Start exercise, play intro media if there is any
            if (exerciseData.introMediaNames.Length > 0 && exerciseData.introMediaNames[0] != "")
            {
                Debug.Log("Display media called");
                //Set up the data for the media manager call
                string[] mediaCallInfo = new string[] { exerciseData.introMediaNames[0], MediaType.Video.ToString() };
                //Pass data, pass lambda expression for the callback
                aPlayer.MediaManager(mediaCallInfo, OnIntroMediaPlaybackComplete);
            }
            else //No intro media
            {
                Debug.Log("Starting question setup");
                SetupNextQuestion(currentQuestionData, currentAnswerPool);
            }
        }

        /// <summary>
        /// Handles calling all the functions needed to remove the last question from the UI
        /// and setup the next, including playing reference media is there is any.
        /// </summary>
        /// <param name="questionData">Question Data object to display</param>
        /// <param name="answerPool">array of possible extra options</param>
        public void SetupNextQuestion(MCQData questionData, string[] answerPool)
        {
            //Remove last question only if not the first question
            if(currentQuestionIndex != 0)
            {
                ClearOptions();
                questionText.text = "";
            }

            //Reset answer tracking for new question
            Debug.Log($"Total num options is: {questionData.TotalNumOptions}");
            selectedOptions = Enumerable.Repeat<bool>(false, questionData.TotalNumOptions).ToList();
            Debug.Log($"selectedOptions initialized, length is : {selectedOptions.Count}");
            correctOptions = Enumerable.Repeat<bool>(false, questionData.TotalNumOptions).ToList();

            //Generate answer options
            List<string> answers = new List<string>();
            if (questionData.numberOfOptionsFromPool > 0)
            {
                //Create list with answer options + extra options from pool
                answers.AddRange(AddOptionsFromPool(answerPool, questionData.answerOptions, questionData.numberOfOptionsFromPool));
            }
            else //No extra choices from the answer pool
            {
                answers.AddRange(questionData.answerOptions);
            }

            //Randomize option order
            List<int> correctIndices = new List<int>();
            if(questionData.randomizeOrder)
            {
                //Convert list to array
                string[] answerArray = answers.ToArray();
                string[] shuffledArray;
                //Pass the array to be shuffled, collect indices of correct answers
                correctIndices.AddRange(RandomizeOptionOrder(answerArray, out shuffledArray, questionData.correctOptionsIndices));
                //Collect the shuffled answers back into the original list
                answers.Clear();
                answers.AddRange(shuffledArray);
            }
            else //Do not randomize the order
            {
                correctIndices.AddRange(questionData.correctOptionsIndices);
            }

            //Setup array that tracks correct answers
            foreach (int index in correctIndices)
            {
                correctOptions[index] = true;
            }

            //Play reference media if there is any (limited to 1 currently)
            if (questionData.referenceMediaNames[0] != "")
            {
                //Indicate that the user should look at the Media Player
                questionText.text = "The question will be displayed after the media finishes playing.";

                //Set up the data for the media manager call
                //Create data to tell the media player to show an image
                string[] mediaCallInfo = new string[] { currentQuestionData.referenceMediaNames[1], 2.ToString()/*MediaType.Image.ToString()*/ };
                //Pass data, pass empty lambda expression for the callback so it does nothing on completion
                aPlayer.MediaManager(mediaCallInfo, () => { });

                //Start audio
                mediaCallInfo = new string[] { currentQuestionData.referenceMediaNames[0], 0.ToString() };
                aPlayer.MediaManager(mediaCallInfo, () => OnRefMediaPlaybackComplete(answers.ToArray()));
            }
            else //No reference media to play
            {
                DisplayNextQuestion(questionData, answers.ToArray());
            }
        }
        #endregion //Public Functions

        #region Event Handlers
        public void OnIntroMediaPlaybackComplete()
        {
            SetupNextQuestion(exerciseData.questions[0], currentAnswerPool);
        }

        public void OnRefMediaPlaybackComplete(string[] answers)
        {
            DisplayNextQuestion(currentQuestionData, answers);
        }

        public void OnFeedbackMediaPlaybackComplete()
        {
            continueButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Handles an answer being selected. Sets index to true in selectedOptions
        /// </summary>
        /// <param name="answerIndex">index of the answer option</param>
        public void OnAnswerSelected(int answerIndex)
        {
            Debug.Log($"OnAnswerSelected called, index = {answerIndex}, List Length: {selectedOptions.Count}");
            selectedOptions[answerIndex] = true;
            submitButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Handles an answer being deselected. Sets index to false in selectedOptions
        /// </summary>
        /// <param name="answerID"></param>
        public void OnAnswerDeselected(int answerID)
        {
            selectedOptions[answerID] = false;
            //If all elements of the list are false, then no options are selected
            if (selectedOptions.TrueForAll(x => !x))
            {
                submitButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Handles when "submit" is pressed. checks answer for correctness, plays media or
        /// goes to the next question
        /// </summary>
        public void OnSubmitPressed()
        {
            //Disable the submit button
            submitButton.gameObject.SetActive(false);

            //Check answer
            bool correct = true;
            for (int i = 0; i < correctOptions.Count; i++)
            {
                if (selectedOptions[i] != correctOptions[i])
                {
                    correct = false;
                }
            }

            if (correct)
            {
                //They selected correctly
                if (currentQuestionData.answerCorrectMediaNames[0] != "")
                {
                    //If there is media to play on a correct answer, play it and wait for the callback
                    string[] mediaCallInfo = new string[] { currentQuestionData.answerCorrectMediaNames[0], "0" };
                    aPlayer.MediaManager(mediaCallInfo, OnFeedbackMediaPlaybackComplete);
                }
                else //No feedback media to play, simply let them continue
                {
                    continueButton.enabled = true;
                }
            }
            else
            {
                //They selected incorrectly
                if (currentQuestionData.answerIncorrectMediaNames[0] != "")
                {
                    //If there is media to play on an incorrect answer, then play it and wait for the callback
                    string[] mediaCallInfo = new string[] { currentQuestionData.answerIncorrectMediaNames[0], "0" };
                    aPlayer.MediaManager(mediaCallInfo, OnFeedbackMediaPlaybackComplete);
                }
                else //No Feedback media to play, simply let them continue
                {
                    continueButton.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Callback for the continue button.
        /// </summary>
        public void OnContinuePressed()
        {
            //Disable continue button
            continueButton.gameObject.SetActive(false);

            currentQuestionIndex++;
            Debug.Log($"CurrentQuestionindex: {currentQuestionIndex}, questions.Length: {exerciseData.questions.Length}");
            if(currentQuestionIndex < exerciseData.questions.Length)
            {
                Debug.Log("Continuing to next question");
                //Increment forward a question internally
                currentQuestionData = exerciseData.questions[currentQuestionIndex];

                //Start displaying the next question
                SetupNextQuestion(currentQuestionData, currentAnswerPool);
            }
            else //No more questions to show
            {
                Debug.Log("Ending MCQ");
                //Clear questions and show finished text
                ShowFinishedScreen();
            }
        }

        /// <summary>
        /// Notifies the lab manager that the MCQ module has completed executing
        /// </summary>
        public void OnFinishButtonPressed()
        {
            Debug.Log("MC finished!");
            GameObject go = GameObject.Find("Lab Control");
            go.GetComponent<LabControl>().MCCompleted();
        }
        #endregion //Event Handlers

        #region Private Methods
        /// <summary>
        /// Generates and displays the answer text and answer options
        /// </summary>
        /// <param name="questionData">Question data object to display</param>
        /// <param name="answers">Array of answer options to dispaly</param>
        private void DisplayNextQuestion(MCQData questionData, string[] answers)
        {
            //Display question text
            questionText.text = questionData.question;

            //Display answers
            DisplayOptions(answers);
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
                for (int i = answers.transform.childCount; i > 4; i--)
                {
                    //Destroy the first child object
                    GameObject.Destroy(answers.transform.GetChild(0).gameObject);
                }
            }

            //Disable the remaining objects
            foreach (Transform answer in answers.transform)
            {
                if(answer.GetComponent<Toggle>().isOn)
                {
                    answer.GetComponent<Toggle>().SetIsOnWithoutNotify(false);
                }
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
        /// <param name="answerPool">Answer Pool array to add options from</param>
        /// <param name="options">existing array of options to add to</param>
        /// <param name="numberOfOptionsToAdd">How many options to add. If this
        ///                                    exceeds the number of options
        ///                                    available in the pool, then less 
        ///                                    will be added.</param>
        /// <returns>New array with random options from the pool
        ///          at the end.</returns>
        private string[] AddOptionsFromPool(
            String[] answerPool,
            string[] options,
            int numberOfOptionsToAdd)
        {
            //Create temporary lists for easier modification
            List<string> outOptions = new List<string>(options);
            List<string> tmpPool = new List<string>(answerPool);

            //If some options from the pool are already present, remove them from the pool
            foreach (string option in outOptions)
            {
                if (tmpPool.Contains(option))
                {
                    tmpPool.Remove(option);
                }
            }

            //Create Random number stream
            System.Random rng = new System.Random();

            //Add random options to the end of the array from the pool
            for (int i = numberOfOptionsToAdd; i > 0; i--)
            {
                int randIndex = rng.Next(tmpPool.Count);
                outOptions.Add(tmpPool[randIndex]);
                tmpPool.RemoveAt(randIndex);

                //If we use all the options from the pool, stop adding
                if (tmpPool.Count == 0)
                {
                    break;
                }
            }

            return outOptions.ToArray();
        }

        /// <summary>
        /// Randomizes the order of the passed array, and returns the index
        /// of the correct answer in the shuffled array
        /// </summary>
        /// <param name="options">Answer options, including correct</param>
        /// <param name="correctAnswerIndices">Correct answers, must match
        ///                                    one string in options</param>
        /// <returns>int array of correct answer indices in new array</returns>
        private int[] RandomizeOptionOrder(
            string[] options,
            out string[] outOptions,
            int[] correctAnswerIndices)
        {
            //Get correct answers before shuffle
            List<string> correctAnswers = new List<string>();
            for (int i = 0; i < correctAnswerIndices.Length; i++)
            {
                correctAnswers.Add(options[correctAnswerIndices[i]]);
            }

            //Do the shuffle, done by reference
            System.Random rng = new System.Random();
            outOptions = options.OrderBy(x => rng.Next()).ToArray();

            //Generate the new array of correct answer indices
            List<int> newCorrectIndices = new List<int>(correctAnswerIndices.Length);// Not correct init *************
            for (int j = 0; j < correctAnswers.Count; j++)
            {
                //Get the new indices of a correct answer in the shuffled list
                newCorrectIndices.Add(Array.IndexOf(outOptions, correctAnswers[j]));
            }

            return newCorrectIndices.ToArray();
        }

        /// <summary>
        /// Displays answer options by enabling or instantiating answer prefabs
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
            for (int i = 0; i < options.Length; i++)
            {
                GameObject answerOption;
                //4, because ClearOptions() leaves 4 disabled options as an answer prefab pool.
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
                //Initialize the answer option
                answerOption.GetComponent<AnswerChoice>().Initialize(options[i], i, this, answers.GetComponent<ToggleGroup>());
            }
        }

        private void ShowFinishedScreen()
        {
            //Clear the answer options
            ClearOptions();

            //Show temporary final text and enable the finishButton
            questionText.text = "You have completed the all questions. Press finish to Exit.";
            finishButton.gameObject.SetActive(true);
        }
        #endregion //Private Methods
    }
}