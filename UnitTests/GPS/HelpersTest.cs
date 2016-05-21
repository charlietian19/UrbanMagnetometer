using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.GPS;

namespace UnitTests.GPS
{
    /// <summary>
    /// Summary description for Helpers
    /// </summary>
    [TestClass]
    public class HelpersTest
    {
        readonly int yearToAdd = (DateTime.Now.Year / 100) * 100;

        public HelpersTest()
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
        public void DateTimeToUnixTimestamp1()
        {
            DateTime winTime = new DateTime(2015, 5, 25, 14, 35, 7,
                DateTimeKind.Utc);
            double unixTime = 1432564507;
            Assert.AreEqual(unixTime, Helpers
                .DateTimeToUnixTimestamp(winTime));
        }

        [TestMethod]
        public void DateTimeToUnixTimestamp2()
        {
            DateTime winTime = new DateTime(1991, 12, 26, 1, 27, 48,
                DateTimeKind.Utc);
            double unixTime = 693710868;
            Assert.AreEqual(unixTime, Helpers
                .DateTimeToUnixTimestamp(winTime));
        }

        [TestMethod]
        public void DateTimeToUnixTimestamp3()
        {
            DateTime winTime = new DateTime(2103, 1, 10, 18, 15, 42,
                DateTimeKind.Utc);
            double unixTime = 4197896142;
            Assert.AreEqual(unixTime, Helpers
                .DateTimeToUnixTimestamp(winTime));
        }

        [TestMethod]
        public void ParseGpsrmcTimeValid()
        {
            string msg = "$GPRMC,183729,A,3907.356,N,12102.482,W,000.0,360.0,080301,015.5,E*6F";            
            var data = Helpers.ParseGpsrmc(msg);
            Assert.AreNotEqual(0, data.ticks);
            Assert.IsTrue(data.valid);
            Assert.AreEqual(18, data.timestamp.Hour);
            Assert.AreEqual(37, data.timestamp.Minute);
            Assert.AreEqual(29, data.timestamp.Second);
            Assert.AreEqual(8, data.timestamp.Day);
            Assert.AreEqual(3, data.timestamp.Month);
            Assert.AreEqual(1 + yearToAdd, data.timestamp.Year);
            Assert.AreEqual("A", data.active);
            Assert.AreEqual(3907.356, data.latitude);
            Assert.AreEqual("N", data.ns);
            Assert.AreEqual(12102.482, data.longitude);
            Assert.AreEqual("W", data.ew);
            Assert.AreEqual(0.0, data.speedKnots);
            Assert.AreEqual(360.0, data.angleDegrees);
        }

        [TestMethod]
        public void ParseGpsrmcTimeInvalid()
        {
            string msg = "$GPRMC,152926,V,6027.8259,N,02225.6713,E,10.8,0.0,190803,5.9,E,S*22";
            var data = Helpers.ParseGpsrmc(msg);
            Assert.AreNotEqual(0, data.ticks);
            Assert.IsFalse(data.valid);
            Assert.AreEqual(15, data.timestamp.Hour);
            Assert.AreEqual(29, data.timestamp.Minute);
            Assert.AreEqual(26, data.timestamp.Second);
            Assert.AreEqual(19, data.timestamp.Day);
            Assert.AreEqual(8, data.timestamp.Month);
            Assert.AreEqual(3 + yearToAdd, data.timestamp.Year);
            Assert.AreEqual("V", data.active);
            Assert.AreEqual(6027.8259, data.latitude);
            Assert.AreEqual("N", data.ns);
            Assert.AreEqual(02225.6713, data.longitude);
            Assert.AreEqual("E", data.ew);
            Assert.AreEqual(10.8, data.speedKnots);
            Assert.AreEqual(0.0, data.angleDegrees);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseGpsrmcWrongFormat()
        {
            var data = Helpers.ParseGpsrmc("Hello World!");
        }
    }
}
