using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;

public class RunTestsFromMenu : ScriptableObject, ICallbacks
{
    private string testTitle;    //Stores name of test collection executed

    /// <summary>
    /// This creates the scriptable object to then perform the tests
    /// </summary>
    [MenuItem("Tools/Run Project Setup Tests")]
    public static void RunProjectsetupTests()
    {
        CreateInstance<RunTestsFromMenu>().StartTestRun("ProjectSetup");
    }

    /// <summary>
    /// This initiates the test run
    /// </summary>
    /// <param name="testSuite">Name of the test collection/class to run</param>
    private void StartTestRun(string testSuite)
    {
        //Store name of test group to display later
        testTitle = testSuite;
        //This makes the Scriptable Object not appear in the hierarchy and not persist in project data
        hideFlags = HideFlags.HideAndDontSave;

        //Create an Api instance, and construct the necessary arguments
        CreateInstance<TestRunnerApi>().Execute(new ExecutionSettings
        {
            //when set to true, The runner is garunteed to be done testing when Execute returns
            runSynchronously = false,
            //Filters are applied to narrow down what tests are performed
            filters = new[]
            {
                new Filter
                {
                    //pass in the name of the group of tests
                    groupNames = new [] {testSuite},
                    testMode = TestMode.EditMode
                }
            }
        });
    }

    #region Unity Methods
    //Register and unregister Api execution callbacks when this object is created or destroyed
    public void OnEnable()
    {
        CreateInstance<TestRunnerApi>().RegisterCallbacks(this);
    }

    public void OnDisable()
    {
        CreateInstance<TestRunnerApi>().UnregisterCallbacks(this);
    }
    #endregion //Unity Methods

    #region TestRunner Callbacks
    //All the callbacks that must be implemented to satisfy the ICallbacks interface, which links to the TestRunnerApi
    //Called before any tests have run
    public void RunStarted(ITestAdaptor testsToRun)
    {
        ;
    }

    //Run before each node in the tree of tests executes
    public void TestFinished(ITestResultAdaptor result)
    {
        ;
    }

    //Runright after each node in the tree of tests executes
    public void TestStarted(ITestAdaptor test)
    {
        ;
    }

    /// <summary>
    /// Recieves the result of the entire test run and handles it
    /// </summary>
    /// <param name="result">Contains various datum about the test results</param>
    public void RunFinished(ITestResultAdaptor result)
    {
        //We passed
        if(result.FailCount == 0)
        {
            EditorUtility.DisplayDialog($"{testTitle} Test Result", "All Project Setup Tests have passed", "sweet");
        }
        else //A test failed
        {
            EditorUtility.DisplayDialog($"{testTitle} Test Result", $"{result.FailCount} tests have failed", "ok");
            EditorApplication.ExecuteMenuItem("Window/General/Test Runner");
        }
        //Clean up
        DestroyImmediate(this);
    }
    #endregion //TestRunner Callbacks
}
