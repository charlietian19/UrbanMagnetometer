using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.GPS.Time;

namespace UnitTests.GPS
{
    /// <summary>
    /// Summary description for FifoStorage
    /// </summary>
    [TestClass]
    public class FifoStorage
    {
        public FifoStorage()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ToArray()
        {
            var storage = new FifoStorage<int>(3);
            storage.Add(1);
            storage.Add(2);
            storage.Add(3);
            CollectionAssert.AreEqual(new int[] { 1, 2, 3 },
                storage.ToArray());
        }

        [TestMethod]
        public void FifoBehaviour()
        {
            var storage = new FifoStorage<int>(3);
            storage.Add(1);
            CollectionAssert.AreEqual(new int[] { 1 },
                storage.ToArray());
            storage.Add(2);
            CollectionAssert.AreEqual(new int[] { 1, 2 },
                storage.ToArray());
            storage.Add(3);
            CollectionAssert.AreEqual(new int[] { 1, 2, 3 },
                storage.ToArray());
            storage.Add(4);
            CollectionAssert.AreEqual(new int[] { 2, 3, 4 },
                storage.ToArray());
            storage.Add(5);
            CollectionAssert.AreEqual(new int[] { 3, 4, 5 },
                storage.ToArray());
            storage.Add(6);
            CollectionAssert.AreEqual(new int[] { 4, 5, 6 },
                storage.ToArray());
        }

        [TestMethod]
        public void RaisesOnPop()
        {
            int raisedWith = 0;
            var storage = new FifoStorage<int>(3);
            storage.OnPop += o => raisedWith = o;
            storage.Add(5);
            Assert.AreEqual(0, raisedWith);
            storage.Add(10);
            Assert.AreEqual(0, raisedWith);
            storage.Add(20);
            Assert.AreEqual(0, raisedWith);
            storage.Add(60);
            Assert.AreEqual(5, raisedWith);
            storage.Add(655);
            Assert.AreEqual(10, raisedWith);
        }

        [TestMethod]
        public void IsEnumerable()
        {
            var valuesToAdd = new HashSet<int>(new int[] { 35, 127, 8 });            
            var storage = new FifoStorage<int>(3);
            foreach (var value in valuesToAdd)
            {
                storage.Add(value);
            }
            Assert.IsTrue(valuesToAdd.SetEquals(storage.ToArray()));
        }
    }
}
