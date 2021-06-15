using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class defining the common functionality of an Activity Module
/// </summary>
public abstract class ActivityModule : MonoBehaviour
{
    public ActivityModuleData data;

    public abstract void Initialize(ActivityModuleData initData);
    //Have to define one function with one parameter. Problem is,
    //different modules will have different data objects. If we 
    //pass the unparsed string, that could work. and then the module
    // could parse it itself and construct the data object it needs.
    //this would require some restructuring of existing code.

    public abstract void EndOfModule();

    public abstract ActivityModuleData SaveState();
}