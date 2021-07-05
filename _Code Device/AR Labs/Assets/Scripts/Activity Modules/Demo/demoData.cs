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
        public bool useSunlight = false;

        public float timeToEnd = -1;
        public bool endUsingButton = true;
    }

}
