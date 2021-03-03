using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace MCQ
{
    /// <summary>
    /// Dummy class for starting the MCQ exercise. Initialize the exercise data here
    /// </summary>
    public class DummyMCQStart : MonoBehaviour
    {
        [SerializeField]
        private MCQManager manager = null;
        [SerializeField]
        private MediaPlayer mPlayer = null;
        [SerializeField]
        private AudioPlayer aPlayer = null;
        [SerializeField]
        private MCExerciseData data = null;

        private void Awake()
        {
            //Fail fast assertions
            Assert.IsNotNull(manager);
            Assert.IsNotNull(mPlayer);
            Assert.IsNotNull(data);
        }

        //private void Start()
        //{
        //    Debug.Log("Starting exercise");
        //    manager.Initialize(data, mPlayer);
        //}

        public void OnStartClicked()
        {
            Debug.Log("OnStartClicked called");
            manager.aPlayer = aPlayer;
            manager.Initialize(data, mPlayer);
        }
    }
}