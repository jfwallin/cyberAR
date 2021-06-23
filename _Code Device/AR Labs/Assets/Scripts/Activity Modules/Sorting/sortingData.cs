using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace sorting
{
    [System.Serializable]
    public class sortingData : ActivityModuleData
    {
        public static int nObjects;

        // declare the sort info array

        public string objectTag = "sortable";
        public float xstart, ystart, zstart;
        public float xend, yend, zend;
        public GameObject[] gameObjects;
        public Vector3[] sortPts;

        bool isSorted;

        // default values for the delay, move time, and flourish of the movements
        public float tdelay = 2.0f;
        public float tmove = 5.0f;
        public int pretty = 1;

        public const int maxObjects = 10;
        public string sortingShape;
        public GameObject myPrefab;
        public Texture[] myTexture = new Texture[maxObjects];
        public String[] tnames = new String[maxObjects];
        public GameObject[] markers = new GameObject[maxObjects];
        public float mscale = 0.1f;
        public GameObject markerPrefab;

        //public AudioClip grab;
        public const int maxWrongAnswers = 5;
        public AudioClip audioInstructions;
        public AudioClip[] wrongOrder = new AudioClip[maxWrongAnswers];
        public AudioClip correctOrder;
        public int wrongAnswerCount = 0;
        public int totalWrongAnswer = 0;

        //public GameObject theButton;
        public GameObject myButton;

        private bool feedbackEnabled = true;



    }
}
