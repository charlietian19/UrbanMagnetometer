using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.GPS.Time;

namespace UnitTests.GPS
{
    /// <summary>
    /// Summary description for FifoStorage
    /// </summary>
    [TestClass]
    public class FifoStorageTest
    {
        public FifoStorageTest()
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
            var res = storage.ToArray();
            CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, res);
        }

        [TestMethod]
        public void FifoBehaviour()
        {
            var storage = new FifoStorage<int>(3);
            Assert.AreEqual(0, storage.Count);
            storage.Add(1);
            Assert.AreEqual(1, storage.Count);
            CollectionAssert.AreEqual(new int[] { 1 },
                storage.ToArray());
            storage.Add(2);
            Assert.AreEqual(2, storage.Count);
            CollectionAssert.AreEqual(new int[] { 1, 2 },
                storage.ToArray());
            storage.Add(3);
            Assert.AreEqual(3, storage.Count);
            CollectionAssert.AreEqual(new int[] { 1, 2, 3 },
                storage.ToArray());
            storage.Add(4);
            Assert.AreEqual(3, storage.Count);
            CollectionAssert.AreEqual(new int[] { 2, 3, 4 },
                storage.ToArray());
            storage.Add(5);
            Assert.AreEqual(3, storage.Count);
            CollectionAssert.AreEqual(new int[] { 3, 4, 5 },
                storage.ToArray());
            storage.Add(6);
            Assert.AreEqual(3, storage.Count);
            CollectionAssert.AreEqual(new int[] { 4, 5, 6 },
                storage.ToArray());
        }

        [TestMethod]
        public void RaiseOnPop()
        {
            int raisedWith = 0;
            var storage = new FifoStorage<int>(3);
            storage.AfterPop += o => raisedWith = o;
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
        public void RaiseOnPush()
        {
            int raisedWith = 0;
            var storage = new FifoStorage<int>(3);
            storage.BeforePush += o => raisedWith = o;
            storage.Add(5);
            Assert.AreEqual(5, raisedWith);
            storage.Add(10);
            Assert.AreEqual(10, raisedWith);
            storage.Add(20);
            Assert.AreEqual(20, raisedWith);
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

        [TestMethod]
        public void ReadByIndex()
        {
            var storage = new FifoStorage<int>(3);
            storage.Add(5);
            storage.Add(265);
            storage.Add(321);
            Assert.AreEqual(5, storage[0]);
            Assert.AreEqual(265, storage[1]);
            Assert.AreEqual(321, storage[2]);
        }

        [TestMethod]
        public void ModifyByIndex()
        {
            var storage = new FifoStorage<int>(3);
            storage.Add(0);
            storage.Add(0);
            storage.Add(0);
            storage[0] = 543;
            storage[1] = 62;
            storage[2] = 9;
            Assert.AreEqual(543, storage[0]);
            Assert.AreEqual(62, storage[1]);
            Assert.AreEqual(9, storage[2]);
        }

        [TestMethod]
        public void FifoAfterModifyByIndex()
        {
            var storage = new FifoStorage<int>(3);
            storage.Add(0);
            storage.Add(0);
            storage.Add(0);
            storage[0] = 543;
            storage[1] = 62;
            storage[2] = 9;
            CollectionAssert.AreEqual(new int[] { 543, 62, 9 },
                storage.ToArray());
            storage.Add(11);
            CollectionAssert.AreEqual(new int[] { 62, 9, 11 },
                storage.ToArray());
            storage.Add(16);
            CollectionAssert.AreEqual(new int[] { 9, 11, 16 },
                storage.ToArray());
        }
    }
}
