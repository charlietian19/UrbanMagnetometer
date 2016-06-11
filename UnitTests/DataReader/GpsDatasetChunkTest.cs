using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.DataReader;
using Utils.GPS;
using System.Diagnostics;
using Moq;
using Utils.GPS.Time;

namespace UnitTests.DataReader
{
    /// <summary>
    /// Summary description for GpsDatasetChunk
    /// </summary>
    [TestClass]
    public class GpsDatasetChunkTest
    {
        public GpsDatasetChunkTest()
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
            var time = DateTime.Now;
            long index = 8712638;
            double counter = 221367.33;
            var estimatorMock = new Mock<ITimeEstimator>();
            var gps = new GpsData
            {
                timestamp = time,
                ticks = Convert.ToInt64(counter * Stopwatch.Frequency),
            };
            var xdata = new double[] { 1.2, 5.0, 2.1 };
            var ydata = new double[] { 3.2, 12.31, 532.1 };
            var zdata = new double[] { 62.3, 532.3, 123.2 };
            var chunk = new GpsDatasetChunk(gps, estimatorMock.Object, index, 
                xdata, ydata, zdata);
            Assert.AreEqual(time, chunk.Time);
            Assert.AreEqual(index, chunk.Index);
            Assert.AreEqual(counter, chunk.PerformanceCounter, 0.01);
            Assert.AreEqual(xdata, chunk.XData);
            Assert.AreEqual(ydata, chunk.YData);
            Assert.AreEqual(zdata, chunk.ZData);
            Assert.AreEqual(gps, chunk.Gps);

            var gps2 = new GpsData
            {
                timestamp = time,
                ticks = Convert.ToInt64(counter * Stopwatch.Frequency),
            };
            chunk.Gps = gps2;
            Assert.AreEqual(gps2, chunk.Gps);
        }
    }
}
