using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions; //Include to use null checks

/*Use require component to make sure references to other components on the
  same object can be found.*/
[RequireComponent(typeof(AudioSource))]
public class SampleScript : MonoBehaviour
{
    /*Put all your variables in one place, and be intentional about
      what accessibility and attributes your variables have. Always
      include variable descriptions*/
    #region Public Varibles
    /*Use header attributes to section variables in the inspector. Make sure to
     * put it before a variable that would be visible in the inspector*/
    [Header("This is a header attribute")]
    public int everyonesInspectorInt;  //Var Description, everyone can access and shows up in the inspector
    [HideInInspector]
    public int everyonesInt;           //Everyone can access, not in inspector
    #endregion

    #region Private Variables
    private int myInt;                 //Class accessible, not in inspector
    [Header("Private Variables header attribute")]
    [SerializeField]
    private int myInspectorInt;        //Class accessible, shows up in inspector
    [SerializeField]
    private AudioSource myAudioSource; //Audio component reference to show how to avoid null reference errors
    #endregion

    #region Unity Methods
    private void Awake()
    {
        /*Do all your reference checking and initializations in awake. 
          MLStart() must now be in Start, not Awake*/

        //Check all your references, There are a couple different situations

        /*It is a good idea to check every reference variable set in the inspector
         * as soon as possible Asserts are a good way to do this, as they will throw
         * an error as soon as you click play, which is faster than waiting for you
         * to try and use the null reference in the middle of a play session*/
        Assert.IsNotNull(myAudioSource, "myAudioSource is not set!");
        /*Public fields run the risk of being set to null at runtime, so checking
         * at the beginning isn't bullet proof, be sure to make private where you can*/

        /*If the reference is to a component on the same object, try and
         * get the reference, but make sure to output that it was missing*/
        if (myAudioSource)
        {
            //Initialization
        }
        else
        {
            Debug.LogWarning("Audio Source not set", this);
            myAudioSource = GetComponent<AudioSource>();
            //Always double check GetComponent, it may fail
            Assert.IsNotNull(myAudioSource, "Could not get a reference to the Audio source");
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    #endregion

    #region Private Methods
    //Put all class only functions here
    #endregion

    #region Public Methods
    //Put all functions accessible from outside the class here
    #endregion

    #region Event Handlers
    //For the "On Button Down" functions, etc
    #endregion
}
