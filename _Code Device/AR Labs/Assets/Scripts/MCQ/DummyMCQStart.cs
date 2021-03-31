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
        private AudioPlayer aPlayer = null;
        [SerializeField]
        public MCExerciseData data = null;

        private void Awake()
        {
            //Fail fast assertions
            Assert.IsNotNull(manager);
            Assert.IsNotNull(aPlayer);
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
            manager.Initialize(data, aPlayer);
        }
    }
}