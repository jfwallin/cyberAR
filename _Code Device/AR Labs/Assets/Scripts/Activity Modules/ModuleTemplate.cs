using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// How to use this template:
/// Copy this class to a new file.
/// Edit the class name to be the name you want, and make sure it matches the filename.
/// Do the same thing for the ModuleTemplateData Data object template.
/// Change all instances of "ModuleTemplateData" to the new data object name
/// </summary>

public class ModuleTemplate : ActivityModule
{
    #region Variables
    //Put all variables for the module here
    private ModuleTemplateData moduleData;
    private MediaPlayer mPlayer;
    #endregion Variables

    #region Public Functions
    public override void Initialize(string initData)
    {
        moduleData = new ModuleTemplateData();
        JsonUtility.FromJsonOverwrite(initData, moduleData);

        //Get reference to the media player
        mPlayer = MediaPlayer.Instance;

        //Check for null references
        Assert.IsNotNull(moduleData);
        Assert.IsNotNull(mPlayer);
        //(Check other variables here------)

        //Initialize state & values of objects here:



        //

        //Play introductory media if there is any


        //

        //Start the module (call some function here)

        //
    }

    public override void EndOfModule()
    {
        Debug.Log("Module Finished");
        //Handle any end of module tasks here



        //
        //Tell Lab control the section is done
        //GameObject.FindObjectOfType<LabControl>().SomeFunction()
    }

    public override ActivityModuleData SaveState()
    {
        //Perform any tasks needed and update the data object


        //
        //Return the updated data object
        return moduleData;
    }
    #endregion Public Functions
}
