using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.GPS;
using System.Diagnostics;

namespace UnitTests.GPS
{
    /// <summary>
    /// Summary description for ThresholdDiscriminator
    /// </summary>
    [TestClass]
    public class NaiveTimeSourceTest
    {
        private readonly long delta = Stopwatch.Frequency;

        public NaiveTimeSourceTest()
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
        public void Constructor()
        {
            var disc = new NaiveTimeStorage(1e-6, 4, 4);
            Assert.AreEqual(4, disc.maxHistory);
            Assert.AreEqual(delta, disc.frequency);
            Assert.AreEqual(0, disc.pointsReceived);
            Assert.AreEqual(Convert.ToInt64(1e-6 * delta), 
                disc.threshold);
        }

        [TestMethod]
        public void HistoryUpdates()
        {
            GpsData data1 = buildData(61293), data2 = buildData(2621378),
                data3 = buildData(412365), data4 = buildData(51928137),
                data5 = buildData(316563812), data6 = buildData(175283);
            var disc = new NaiveTimeStorage(1e-6, 3, 3);
            disc.Store(data1);
            assertHistory(data1, disc.history[0]);
            disc.Store(data2);
            assertHistory(data2, disc.history[0]);
            assertHistory(data1, disc.history[1]);
            disc.Store(data3);
            assertHistory(data3, disc.history[0]);
            assertHistory(data2, disc.history[1]);
            assertHistory(data1, disc.history[2]);
            disc.Store(data4);
            assertHistory(data4, disc.history[0]);
            assertHistory(data3, disc.history[1]);
            assertHistory(data2, disc.history[2]);
            disc.Store(data5);
            assertHistory(data5, disc.history[0]);
            assertHistory(data4, disc.history[1]);
            assertHistory(data3, disc.history[2]);
            disc.Store(data6);
            assertHistory(data6, disc.history[0]);
            assertHistory(data5, disc.history[1]);
            assertHistory(data4, disc.history[2]);
        }

        private GpsData buildData(long ticks)
        {
            return new GpsData() { ticks = ticks, timestamp = new DateTime(ticks) };
        }

        private void assertHistory(GpsData dataExpected, GpsData data)
        {
            Assert.AreEqual(dataExpected.ticks, data.ticks);
            Assert.AreEqual(dataExpected.timestamp, data.timestamp);
        }

        [TestMethod]
        public void FirstSamplesAreAlwaysInvalid()
        {
            GpsData data = buildData(7129835162);
            int n = 4;
            var disc = new NaiveTimeStorage(1e-6, n, n);
            for (int i = 0; i < n; i++)
            {
                Assert.IsFalse(disc.Store(data));
            }
        }

        [TestMethod]
        public void SampleWithingThreshold1()
        {
            GpsData data = buildData(174234);
            var disc = new NaiveTimeStorage(1e-6, 2, 2);
            disc.Store(data);
            Assert.IsFalse(disc.history[0].valid);
            disc.Store(data);
            Assert.IsFalse(disc.history[0].valid);
            Assert.IsTrue(disc.Store(data));
            Assert.IsTrue(disc.history[0].valid);
        }

        [TestMethod]
        public void SampleWithingThreshold2()
        {
            double threshold = 100e-6;
            long ticks = 174234;
            GpsData data1 = buildData(ticks);
            GpsData data2 = buildData(ticks + delta);
            GpsData data3 = buildData(ticks + delta * 2 - 
                Convert.ToInt64(threshold * 4 * delta));
            var disc = new NaiveTimeStorage(threshold, 2, 2);
            disc.Store(data1);
            Assert.IsFalse(disc.history[0].valid);
            disc.Store(data2);
            Assert.IsFalse(disc.history[0].valid);
            Assert.IsTrue(disc.Store(data3));
            Assert.IsTrue(disc.history[0].valid);
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
            var disc = new NaiveTimeStorage(threshold, 2, 2);
            disc.Store(data1);
            Assert.IsFalse(disc.history[0].valid);
            disc.Store(data2);
            Assert.IsFalse(disc.history[0].valid);
            Assert.IsFalse(disc.Store(data3));
            Assert.IsFalse(disc.history[0].valid);
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
            var disc = new NaiveTimeStorage(threshold, 2, 2);
            disc.Store(data1);
            Assert.IsFalse(disc.history[0].valid);
            disc.Store(data2);
            Assert.IsFalse(disc.history[0].valid);
            Assert.IsTrue(disc.Store(data3));
            Assert.IsTrue(disc.history[0].valid);
            Assert.IsFalse(disc.Store(data4));
            Assert.IsFalse(disc.history[0].valid);
        }
    }
}
