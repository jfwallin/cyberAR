using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class objectModifications : ObjectInfo
{
    public int activationConditions;
    public bool reactivateObject = false;
    // -1 = initial
    // 0 = final
}

