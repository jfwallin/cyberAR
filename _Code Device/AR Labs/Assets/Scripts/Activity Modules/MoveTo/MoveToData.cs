using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MoveToRoutines
{
    [System.Serializable]
    public class MoveToActivityData: ActivityModuleData
    {

        bool isSorted;

        public const int maxWrongAnswers = 5;

        public string[] wrongOrderAudio;
        public string correctOrderAudio;

        private bool feedbackEnabled = true;

        public string objects;
        public string introAudio;
        public bool useSunlight = false;

        public float timeToEnd = -1;
        public bool endUsingButton = true;

        public bool createObjects = true;
        public bool destroyObjects = true;
        public bool restoreLights = true;

    }



}


