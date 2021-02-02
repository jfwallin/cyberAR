using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    // A demonstration of a Play test of a monobehaviiour. When this executes an empty test scene is created, so
    // any objcts or scripts you wish to reference and test must be instantiated. Increasing the time scale can
    // help make simulation go quicker
    public class MonobehaviorTestExample
    {
        #region Variables
        //References for the test methods to use
        GameObject go;
        MonobehaviourTest monoScript;
        #endregion //Variables

        #region Setup / Teardown
        //This executes once for the entire test suite/class before the tests run
        [OneTimeSetUp]
        public void Begin()
        {
            //Increase time flow so simulation occurs more quickly
            Time.timeScale = 5f;
        }

        //This executes once for the entire test suite/class after the tests have run
        [OneTimeTearDown]
        public void End()
        {
            Time.timeScale = 1f;
        }

        //Executed before every test
        [SetUp]
        public void SetUp()
        {
            //create gameobject and reference to script we wish to test
            go = new GameObject();
            monoScript = go.AddComponent<MonobehaviourTest>();
        }

        //Executed after every test
        [TearDown]
        public void TearDown()
        {
            //delete gameobject and references. It is always recommended to
            //start fresh with every test to ensure no cross contamination
            //between tests
            GameObject.Destroy(go);
            go = null;
            monoScript = null;
        }
        #endregion //Setup / Teardown

        #region Tests
        [Test]
        public void list_length_is_zero_when_first_created()
        {
            //Arrange
                //Was already done in Setup()

            //Action
            int listLength = monoScript.GetListLength();

            //Assert
            Assert.AreEqual(0, listLength);
        }

        [Test]
        public void list_length_increses_by_one_when_one_item_is_added()
        {
            //Arrange
            monoScript.AddItemToList(2);
            monoScript.AddItemToList(5);
            int initialLength = monoScript.GetListLength();

            //Action
            monoScript.AddItemToList(4);
            int listLength = monoScript.GetListLength();

            //Assert
            Assert.AreEqual(initialLength + 1, listLength);
        }
        #endregion //Tests

        #region Unity Tests
        [UnityTest]
        public IEnumerator list_decreases_over_time_when_decay_is_true()
        {
            //Arrange
            monoScript.AddItemToList(1);
            monoScript.AddItemToList(2);
            monoScript.AddItemToList(3);
            int initialLength = monoScript.GetListLength();

            //Action
            monoScript.MakeListDecay();

            yield return new WaitForSeconds(2f);

            //Assert
            Assert.Less(monoScript.GetListLength(), initialLength);
        }
        #endregion //Unity Tests
    }
}
