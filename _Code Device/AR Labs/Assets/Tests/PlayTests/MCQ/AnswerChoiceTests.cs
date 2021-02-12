using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using NSubstitute;

namespace Tests
{
    public class AnswerChoiceTests
    {
        #region Vairables
        private MCQ.AnswerChoice answer = null;         //Reference to the class to be tested
        #endregion //Variables

        #region Test Suite
        //Called before each unit test is run to ensure a clean run
        [SetUp]
        public void SetUp()
        {
            //Setup Class to test
            answer = new GameObject().AddComponent<MCQ.AnswerChoice>();
        }

        //Called after each unit test is run to clean up residual state
        [TearDown]
        public void TearDown()
        {
            //Reset the class
            GameObject.Destroy(answer.gameObject);
        }

        [Test]
        public void Initialize_SetsAnswerIndex_WhenPassedInt(
            [NUnit.Framework.Range(0,4,2)] int answerIndex)
        {
            //Arrange
            FieldInfo answerIDFieldInfo = answer.GetType().GetField("answerID", BindingFlags.NonPublic | BindingFlags.Instance);
            //Action
            answer.Initialize("", answerIndex, null, null);
            //Assert
            Assert.AreEqual(answerIndex, answerIDFieldInfo.GetValue(answer));
        }

        [Test]
        public void Initialize_FindsQuestionManager_WhenReferenceNotPassed()
        {
            //Arrange
            GameObject managerGameObject = new GameObject();
            MCQ.MCQManager questionManager = managerGameObject.AddComponent<MCQ.MCQManager>();
            GameObject middleGameObject = new GameObject();
            middleGameObject.transform.SetParent(managerGameObject.transform);
            answer.transform.SetParent(middleGameObject.transform);
            //Reflection to access private field
            FieldInfo questionManagerFieldInfo = answer.GetType().GetField("manager", BindingFlags.NonPublic | BindingFlags.Instance);
            //Action
            answer.Initialize("", 0, null, null);
            //Assert
            Assert.AreEqual(questionManager, questionManagerFieldInfo.GetValue(answer));
        }

        [Test]
        public void Initialize_SetsToggleGroup_WhenPassedToggleGroup()
        {
            //Arrange
            answer.gameObject.AddComponent<Toggle>().group = null;
            ToggleGroup newToggleGroup = answer.gameObject.AddComponent<ToggleGroup>();
            //Action
            answer.Initialize("", 0, null, newToggleGroup);
            //Assert
            Assert.AreEqual(newToggleGroup, answer.GetComponent<Toggle>().group);
        }

        [Test]
        public void OnSelectchange_CallsAnswerSelectedWithCorrectID_WhenToggleSelected(
            [NUnit.Framework.Range(0, 4, 2)] int answerIndex)
        {
            //Arrange
            bool selected = true;
            MCQ.IMCQManager qmMock = Substitute.For<MCQ.IMCQManager>();
            answer.Initialize("", answerIndex, qmMock, null);
            //Action
            qmMock.ClearReceivedCalls();
            answer.OnSelectChange(selected);
            //Assert
            qmMock.Received().OnAnswerSelected(answerIndex);
        }

        [Test]
        public void OnSelectChange_CallsAnswerDeselectedWithCorrectID_WhenToggleDeselected(
            [NUnit.Framework.Range(0, 4, 2)] int answerIndex)
        {
            //Arrange
            bool selected = false;
            MCQ.IMCQManager qmMock = Substitute.For<MCQ.IMCQManager>();
            answer.Initialize("", answerIndex, qmMock, null);
            //Action
            qmMock.ClearReceivedCalls();
            answer.OnSelectChange(selected);
            //Assert
            qmMock.Received().OnAnswerDeselected(answerIndex);
        }
        #endregion //Test Suite
    }
}
