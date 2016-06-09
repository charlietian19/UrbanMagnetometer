using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.GPS;
using System.Diagnostics;
using Moq;
using Utils.Fixtures;

namespace UnitTests.GPS
{
    /// <summary>
    /// Summary description for Thresholdstorageriminator
    /// </summary>
    [TestClass]
    public class NaiveTimeStorageTest
    {
        private long delta = Stopwatch.Frequency;
        private readonly DateTime baseData = DateTime.Now;

        Mock<ITimer> timer;
        Mock<IStopwatch> stopwatch;

        public NaiveTimeStorageTest()
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

        [TestInitialize]
        public void Initialize()
        {
            timer = new Mock<ITimer>();
            timer.SetupAllProperties();

            stopwatch = new Mock<IStopwatch>();
            stopwatch.Setup(o => o.Frequency).Returns(2500000);
            stopwatch.Setup(o => o.GetTimestamp()).Returns(delta);
        }


        [TestMethod]
        public void Constructor()
        {
            var storage = new NaiveTimeStorage(1e-6, 40, 5, 60);
            Assert.AreEqual(40, storage.maxHistory);
            Assert.AreEqual(delta, storage.frequency);
            Assert.AreEqual(0, storage.pointsReceived);
            Assert.AreEqual(Convert.ToInt64(1e-6 * delta), 
                storage.threshold);
            Assert.AreEqual(5, storage.lookback);
            Assert.AreEqual(60, storage.invalidateAfter);
        }

        [TestMethod]
        public void HistoryUpdates()
        {
            GpsData data1 = buildData(61293), data2 = buildData(2621378),
                data3 = buildData(412365), data4 = buildData(51928137),
                data5 = buildData(316563812), data6 = buildData(175283);
            var storage = new NaiveTimeStorage(1e-6, 3, 3, 5);
            storage.Store(data1);
            assertHistory(data1, storage.history[0]);
            storage.Store(data2);
            assertHistory(data2, storage.history[0]);
            assertHistory(data1, storage.history[1]);
            storage.Store(data3);
            assertHistory(data3, storage.history[0]);
            assertHistory(data2, storage.history[1]);
            assertHistory(data1, storage.history[2]);
            storage.Store(data4);
            assertHistory(data4, storage.history[0]);
            assertHistory(data3, storage.history[1]);
            assertHistory(data2, storage.history[2]);
            storage.Store(data5);
            assertHistory(data5, storage.history[0]);
            assertHistory(data4, storage.history[1]);
            assertHistory(data3, storage.history[2]);
            storage.Store(data6);
            assertHistory(data6, storage.history[0]);
            assertHistory(data5, storage.history[1]);
            assertHistory(data4, storage.history[2]);
        }

        private GpsData buildData(long ticks)
        {
            return new GpsData()
            {
                ticks = ticks,
                timestamp = baseData.AddSeconds(ticks / delta),
                valid = true
            };
        }

        private void assertHistory(GpsData dataExpected, GpsData data)
        {
            Assert.AreEqual(dataExpected.ticks, data.ticks);
            Assert.AreEqual(dataExpected.timestamp, data.timestamp);
        }

        [TestMethod]
        public void SamplesBeforeLookbackAreInvalid()
        {
            GpsData data = buildData(7129835162);
            var storage = new NaiveTimeStorage(1e-6, 4, 2, 4);
            storage.Store(data);
            storage.Store(data);
            Assert.IsFalse(storage.history[0].valid);
            Assert.IsFalse(storage.history[1].valid);
        }

        [TestMethod]
        public void SamplesPastLookbackAreValid()
        {
            GpsData data = buildData(7129835162);
            var storage = new NaiveTimeStorage(1e-6, 4, 2, 4);
            storage.Store(data);
            storage.Store(data);
            storage.Store(data);
            storage.Store(data);
            Assert.IsFalse(storage.history[2].valid);
            Assert.IsFalse(storage.history[3].valid);
        }

        [TestMethod]
        public void DataMarkedInvalidByGpsRemainsInvalid()
        {
            GpsData data = buildData(7129835162);
            data.valid = false;
            var storage = new NaiveTimeStorage(1e-6, 3, 1, 3);
            storage.Store(data);
            storage.Store(data);
            storage.Store(data);
            Assert.IsFalse(storage.history[0].valid);
            Assert.IsFalse(storage.history[1].valid);
            Assert.IsFalse(storage.history[2].valid);
        }

        [TestMethod]
        public void SampleWithingThreshold1()
        {
            GpsData data = buildData(174234);
            var storage = new NaiveTimeStorage(1e-6, 2, 2, 3);
            storage.Store(data);
            Assert.IsFalse(storage.history[0].valid);
            storage.Store(data);
            Assert.IsFalse(storage.history[0].valid);
            Assert.IsTrue(storage.Store(data));
            Assert.IsTrue(storage.history[0].valid);
        }

        [TestMethod]
        public void SampleWithingThreshold2()
        {
            double threshold = 100e-6;
            long ticks = 174234;
            GpsData data1 = buildData(ticks);
            GpsData data2 = buildData(ticks + 2 * delta);
            GpsData data3 = buildData(ticks + delta * 3 - 
                Convert.ToInt64(threshold * 4 * delta));
            var storage = new NaiveTimeStorage(threshold, 2, 2, 3);
            storage.Store(data1);
            Assert.IsFalse(storage.history[0].valid);
            storage.Store(data2);
            Assert.IsFalse(storage.history[0].valid);
            Assert.IsTrue(storage.Store(data3));
            Assert.IsTrue(storage.history[0].valid);
        }

        [TestMethod]
        public void SampleAboveThreshold1()
        {
            double threshold = 100e-6;
            long ticks = 5174232614;
            GpsData data1 = buildData(ticks);
            GpsData data2 = buildData(ticks + delta);
            GpsData data3 = buildData(ticks + delta * 2 +
                Convert.ToInt64(threshold * 4 * delta));
            var storage = new NaiveTimeStorage(threshold, 2, 2, 3);
            storage.Store(data1);
            Assert.IsFalse(storage.history[0].valid);
            storage.Store(data2);
            Assert.IsFalse(storage.history[0].valid);
            Assert.IsFalse(storage.Store(data3));
            Assert.IsFalse(storage.history[0].valid);
        }

        [TestMethod]
        public void SampleAboveThreshold2()
        {
            double threshold = 100e-6;
            long ticks = 5174232614;
            GpsData data1 = buildData(ticks);
            GpsData data2 = buildData(ticks + delta);
            GpsData data3 = buildData(ticks + delta * 2 +
                Convert.ToInt64(threshold * 2 / 3 * delta));
            GpsData data4 = buildData(ticks + delta * 3 +
                Convert.ToInt64(threshold * 4 / 3 * delta));
            var storage = new NaiveTimeStorage(threshold, 2, 2, 2);
            storage.Store(data1);
            Assert.IsFalse(storage.history[0].valid);
            storage.Store(data2);
            Assert.IsFalse(storage.history[0].valid);
            Assert.IsTrue(storage.Store(data3));
            Assert.IsTrue(storage.history[0].valid);
            Assert.IsFalse(storage.Store(data4));
            Assert.IsFalse(storage.history[0].valid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThresholdCantBeNegative()
        {
            var store = new NaiveTimeStorage(-5, 23, 5, 23);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HistoryHasToBeAtLeastTwo()
        {
            var store = new NaiveTimeStorage(1, 1, 1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void LookbackHasToBeAtLeastOne()
        {
            var store = new NaiveTimeStorage(1, 4, 0, 4);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HistoryCantBeLessThanLookback()
        {
            var store = new NaiveTimeStorage(1, 10, 15, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidateAfterCantBeNegative()
        {
            var store = new NaiveTimeStorage(1, 10, 15, -1);
        }

        [TestMethod]
        public void StalePointsInvalidate()
        {
            delta = 1000;
            stopwatch.Setup(o => o.Frequency).Returns(delta);

            var data1 = buildData(1000);
            var data2 = buildData(2000);
            var data3 = buildData(3000);

            /* Invalidate stored points after 3 seconds */
            var storage = new NaiveTimeStorage(100e-6, 3, 1, 3,
                stopwatch.Object, timer.Object);
            storage.Store(data1);
            storage.Store(data2);
            storage.Store(data3);

            Assert.AreEqual(true, storage.history[0].valid);
            Assert.AreEqual(true, storage.history[1].valid);
            Assert.AreEqual(false, storage.history[2].valid);

            stopwatch.Setup(o => o.GetTimestamp()).Returns(5500);
            timer.Raise(o => o.Elapsed += null, null, null);

            Assert.AreEqual(true, storage.history[0].valid);
            Assert.AreEqual(false, storage.history[1].valid);
            Assert.AreEqual(false, storage.history[2].valid);
        }
    }
}
