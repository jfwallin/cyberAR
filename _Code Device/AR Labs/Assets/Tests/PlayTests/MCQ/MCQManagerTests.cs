using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using System.Reflection;

namespace Tests
{
    public class MCQManagerTests
    {
        #region Vairables
        //Reference to class being tested
        MCQ.MCQManager mcqManager = null;

        #endregion //Variables

        #region Test Suite
        //Called before each unit test is run to ensure a clean run
        [SetUp]
        public void SetUp()
        {
            //Initialize the class to test
            mcqManager = new GameObject().AddComponent<MCQ.MCQManager>();
        }

        //Called after each unit test is run to clean up residual state
        [TearDown]
        public void TearDown()
        {
            //Reset the class to test
            GameObject.Destroy(mcqManager.gameObject);

            //Reset logging
            if (LogAssert.ignoreFailingMessages)
                LogAssert.ignoreFailingMessages = false;
        }

        [Test]
        public void Initialize_FindsRef_RefNotSetInInspector(
            [NUnit.Framework.Values("questionText", "answers", "submitButton", "continueButton")] string fieldName)
        {
            //Arrange
            GameObject dummyBackgroundObject = new GameObject();
            dummyBackgroundObject.transform.SetParent(mcqManager.transform);
            GameObject dummyObject = new GameObject();
            dummyObject.transform.SetParent(dummyBackgroundObject.transform);
            GameObject dummyButton = new GameObject();
            dummyButton.AddComponent<Button>();
            dummyButton.transform.SetParent(dummyObject.transform);
            switch (fieldName)
            {
                case "questionText":
                    dummyObject.name = "Question Text";
                    dummyObject.AddComponent<Text>();
                    break;
                case "answers":
                    dummyObject.name = "Answers";
                    break;
                case "submitButton":
                    dummyObject.name = "Buttons";
                    dummyButton.name = "Submit";
                    break;
                case "continueButton":
                    dummyObject.name = "Buttons";
                    dummyButton.name = "Continue";
                    break;
            }
            FieldInfo refCheckFieldInfo = mcqManager.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            LogAssert.ignoreFailingMessages = true;
            //Action
            mcqManager.Initialize(null, null);
            //Assert
            switch(fieldName)
            {
                case "questionText":
                case "answers":
                    Assert.AreEqual(dummyObject, refCheckFieldInfo.GetValue(mcqManager));
                    break;
                case "submitButton":
                case "continueButton":
                    Assert.AreEqual(dummyButton, refCheckFieldInfo.GetValue(mcqManager));
                    break;
            }
        }
        #endregion //Test Suite
    }
}
