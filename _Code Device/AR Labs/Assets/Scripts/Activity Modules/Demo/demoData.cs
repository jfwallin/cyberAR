using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace demoRoutines 
{
    [System.Serializable]
    public class demoData : ActivityModuleData
    {
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
