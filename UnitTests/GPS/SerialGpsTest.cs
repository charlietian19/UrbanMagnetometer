using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Utils.GPS.SerialGPS;
using Utils.GPS;
using Utils.Fixtures;
using Utils.GPS.Time;

namespace UnitTests.GPS
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SerialGpsTest
    {
        /* Stopwatch mock frequency */
        long frequency = 2000000;

        /* PPS period in seconds */
        double period = 1;

        /* PPS high level phase length, in seconds */
        double high = 0.1;

        /* System.Stopwatch mock */
        Mock<IStopwatch> stopwatch;

        /* Serial port mock */
        Mock<ISerial> serial;

        /* GPS device */
        ITimeSource gps;

        /* PPS and NMEA timings*/
        long[] ticks = new long[3], nmeaTicks = new long[3];
        List<DateTime> date;

        /* PPS and NMEA parameters set by the event handlers */
        long[] returnTicks = new long[3];
        GpsData[] returnData = new GpsData[3];
        int indexTicks = 0, indexData = 0;

        [TestInitialize]
        public void Initialize()
        {
            serial = new Mock<ISerial>();
            serial.Setup(o => o.Open())
                .Callback(() => serial.Raise(o => o.PortOpened += null));
            serial.Setup(o => o.Close());
            serial.Setup(o => o.GetPpsLevel()).Returns(false);

            stopwatch = new Mock<IStopwatch>();
            stopwatch.Setup(o => o.Frequency).Returns(frequency);
            Helpers.stopwatch = stopwatch.Object;

            var date0 = new DateTime(2013, 10, 2, 17, 44, 3);
            long ticks0 = 642876378;
            var dt = Convert.ToInt64(frequency * high * 2);

            date = new List<DateTime> {
                date0,
                date0.AddSeconds(1),
                date0.AddSeconds(2)
            };

            ticks = new long[] {
                642876378,
                ticks0 + Convert.ToInt64(frequency * period),
                ticks0 + 2 * Convert.ToInt64(frequency * period)
            };

            nmeaTicks = new long[]
            {
                ticks[0] + dt,
                ticks[1] + dt,
                ticks[2] + dt
            };

            gps = new SerialGps(serial.Object, stopwatch.Object);
            gps.PpsReceived += ticks =>
            {
                returnTicks[indexTicks] = ticks;
                indexTicks += 1;
            };

            gps.TimestampReceived += data =>
            {
                returnData[indexData] = data;
                indexData += 1;
            };
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
        public void NormalOperationStartLow()
        {
            gps.Open();
            SendPps(ticks[0]);
            SendTimestamp(nmeaTicks[0], date[0]);
            SendPps(ticks[1]);
            SendTimestamp(nmeaTicks[1], date[1]);
            SendPps(ticks[2]);
            SendTimestamp(nmeaTicks[2], date[2]);
            gps.Close();

            AssertNormalOperation();
        }        

        [TestMethod]
        public void NormalOperationStartHigh()
        {
            serial.Setup(o => o.GetPpsLevel()).Returns(true);

            gps.Open();
            ChangePpsLevel(ticks[0] - 261496, false);
            SendPps(ticks[0]);
            SendTimestamp(nmeaTicks[0], date[0]);
            SendPps(ticks[1]);
            SendTimestamp(nmeaTicks[1], date[1]);
            SendPps(ticks[2]);
            SendTimestamp(nmeaTicks[2], date[2]);
            gps.Close();

            AssertNormalOperation();
        }

        private void AssertNormalOperation()
        {
            Assert.AreEqual(ticks[0], returnTicks[0]);
            AssertData(true, ticks[0], date[0], returnData[0]);
            Assert.AreEqual(ticks[1], returnTicks[1]);
            AssertData(true, ticks[1], date[1], returnData[1]);
            Assert.AreEqual(ticks[2], returnTicks[2]);
            AssertData(true, ticks[2], date[2], returnData[2]);
        }

        [TestMethod]
        public void PpsMissedFirstPulse()
        {
            gps.Open();
            indexTicks = 1;
            SendTimestamp(nmeaTicks[0], date[0]);
            SendPps(ticks[1]);
            SendTimestamp(nmeaTicks[1], date[1]);
            SendPps(ticks[2]);
            SendTimestamp(nmeaTicks[2], date[2]);
            gps.Close();
            
            AssertData(false, nmeaTicks[0], date[0], returnData[0]);
            Assert.AreEqual(ticks[1], returnTicks[1]);
            AssertData(true, ticks[1], date[1], returnData[1]);
            Assert.AreEqual(ticks[2], returnTicks[2]);
            AssertData(true, ticks[2], date[2], returnData[2]);
        }

        [TestMethod]
        public void NmeaMissedFirstPulse()
        {
            gps.Open();
            SendPps(ticks[0]);
            indexData = 1;
            SendPps(ticks[1]);
            SendTimestamp(nmeaTicks[1], date[1]);
            SendPps(ticks[2]);
            SendTimestamp(nmeaTicks[2], date[2]);
            gps.Close();

            Assert.AreEqual(ticks[0], returnTicks[0]);
            Assert.AreEqual(ticks[1], returnTicks[1]);
            AssertData(true, ticks[1], date[1], returnData[1]);
            Assert.AreEqual(ticks[2], returnTicks[2]);
            AssertData(true, ticks[2], date[2], returnData[2]);
        }

        [TestMethod]
        public void PpsMissedMiddlePulse()
        {
            gps.Open();
            SendPps(ticks[0]);
            SendTimestamp(nmeaTicks[0], date[0]);
            indexTicks = 2;
            SendTimestamp(nmeaTicks[1], date[1]);
            SendPps(ticks[2]);
            SendTimestamp(nmeaTicks[2], date[2]);
            gps.Close();

            Assert.AreEqual(ticks[0], returnTicks[0]);
            AssertData(true, ticks[0], date[0], returnData[0]);
            AssertData(false, nmeaTicks[1], date[1], returnData[1]);
            Assert.AreEqual(ticks[2], returnTicks[2]);
            AssertData(true, ticks[2], date[2], returnData[2]);
        }

        [TestMethod]
        public void NmeaMissedMiddlePulse()
        {
            gps.Open();
            SendPps(ticks[0]);
            SendTimestamp(nmeaTicks[0], date[0]);
            SendPps(ticks[1]);
            indexData = 2;
            SendPps(ticks[2]);
            SendTimestamp(nmeaTicks[2], date[2]);
            gps.Close();

            Assert.AreEqual(ticks[0], returnTicks[0]);
            AssertData(true, ticks[0], date[0], returnData[0]);
            Assert.AreEqual(ticks[1], returnTicks[1]);
            Assert.AreEqual(ticks[2], returnTicks[2]);
            AssertData(true, ticks[2], date[2], returnData[2]);
        }

        /* Verifies the returned GPS data against the expected values */
        private void AssertData(bool validExpected, long ticksExpected,
            DateTime timestampExpected, GpsData data)
        {
            Assert.AreEqual(ticksExpected, data.ticks);
            Assert.AreEqual(validExpected, data.valid);
            Assert.AreEqual(timestampExpected, data.timestamp);
        }

        /* Emulates receiving a PPS pulse */
        private void SendPps(long ticks)
        {
            ChangePpsLevel(ticks, true);
            var ticksLow = ticks + Convert.ToInt64(frequency * high);
            ChangePpsLevel(ticksLow, false);
        }

        /* Emulates receiving a GPS timestamp */
        private void SendTimestamp(long ticks, DateTime timestamp)
        {
            stopwatch.Setup(o => o.GetTimestamp()).Returns(ticks);
            SendNmeaString(BuildNmea(timestamp));
        }

        /* Emulates changing the PPS level */
        private void ChangePpsLevel(long ticks, bool level)
        {
            stopwatch.Setup(o => o.GetTimestamp()).Returns(ticks);
            serial.Setup(o => o.GetPpsLevel()).Returns(level);
            serial.Raise(o => o.PpsChanged += null, level);
        }

        /* Generates an NMEA GPSRMC string given a DateTime object.
        The checksum is wrong. */
        private string BuildNmea(DateTime date)
        {
            var format = "$GPRMC,{0:00}{1:00}{2:00},A,4916.45,N,12311.12,W," +
                "000.5,054.7,{3:00}{4:00}{5:00},020.3,E*68\r\n";
            var msg = string.Format(format, date.Hour, date.Minute, date.Second,
                date.Day, date.Month, date.Year % 100);
            return msg;
        }

        /* Emulates receiving a string through the serial port*/
        private void SendNmeaString(string data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                SendNmeaChar(data[i]);
            }
        }

        /* Emulates receiving a single character through the serial port */
        private void SendNmeaChar(char data)
        {
            serial.Raise(o => o.DataReceived += null, Convert.ToByte(data));
        }
    }
}
