using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Utils.GPS;

namespace UnitTests.GPS
{
    [TestClass]
    public class NaiveTimeEstimatorTest
    {
        

        public NaiveTimeEstimatorTest()
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
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MinPointsToFitSetError()
        {
            var estimator = new NaiveTimeEstimator();
            estimator.MinPointsToFit = 1; // can't fit a line with 1 point
        }

        
        [TestMethod]
        public void MinPointsToFitSetSuccess()
        {
            var estimator = new NaiveTimeEstimator();
            Assert.AreNotEqual(8, estimator.MinPointsToFit);
            estimator.MinPointsToFit = 8;
            Assert.AreEqual(8, estimator.MinPointsToFit);
        }

        [TestMethod]
        public void DataPointsArePassedToStorage()
        {            
            var storage = new Mock<ITimeStorage>();            
            storage.Setup(o => o.Store(It.IsAny<GpsData>()));
            var estimator = new NaiveTimeEstimator(storage.Object);
            var data1 = new GpsData()
            {
                timestamp = new DateTime(1234),
                ticks = 1234,
                valid = true
            };
            var data2 = new GpsData()
            {
                ticks = 75923,
                timestamp = new DateTime(12637812),
                valid = false
            };
            estimator.PutTimestamp(data1);
            estimator.PutTimestamp(data2);
            storage.Verify(o => o.Store(data1), Times.Once());
            storage.Verify(o => o.Store(data2), Times.Once());
        }

        [TestMethod]
        public void FitModel1()
        {
            var data1 = new GpsData()
            {
                ticks = 10,
                timestamp = new DateTime(10),
                valid = true
            };
            var data2 = new GpsData()
            {
                ticks = 20,
                timestamp = new DateTime(20),
                valid = true
            };

            var validPoints = new GpsData[] { data1, data2 };
            var storage = new Mock<ITimeStorage>();
            storage.Setup(o => o.ValidPointsCount).Returns(2);
            storage.Setup(o => o.GetValidPoints()).Returns(validPoints);
            var estimator = new NaiveTimeEstimator(storage.Object);
            estimator.Update();
            Assert.AreEqual(0, estimator.GetTimeStamp(0).timestamp.Ticks);
            Assert.AreEqual(30, estimator.GetTimeStamp(30).timestamp.Ticks);
        }

        [TestMethod]
        public void FitModel2()
        {
            var data1 = new GpsData()
            {
                ticks = 10,
                timestamp = new DateTime(10),
                valid = true
            };
            var data2 = new GpsData()
            {
                ticks = 20,
                timestamp = new DateTime(10),
                valid = true
            };

            var validPoints = new GpsData[] { data1, data2 };
            var storage = new Mock<ITimeStorage>();
            storage.Setup(o => o.ValidPointsCount).Returns(2);
            storage.Setup(o => o.GetValidPoints()).Returns(validPoints);
            var estimator = new NaiveTimeEstimator(storage.Object);
            estimator.Update();
            Assert.AreEqual(10, estimator.GetTimeStamp(0).timestamp.Ticks);
            Assert.AreEqual(10, estimator.GetTimeStamp(30).timestamp.Ticks);
        }

        [TestMethod]
        public void FitModel3()
        {
            var data1 = new GpsData()
            {
                ticks = 100,
                timestamp = new DateTime(50 + 10),
                valid = true
            };
            var data2 = new GpsData()
            {
                ticks = 200,
                timestamp = new DateTime(100 + 10),
                valid = true
            };

            var validPoints = new GpsData[] { data1, data2 };
            var storage = new Mock<ITimeStorage>();
            storage.Setup(o => o.ValidPointsCount).Returns(2);
            storage.Setup(o => o.GetValidPoints()).Returns(validPoints);
            var estimator = new NaiveTimeEstimator(storage.Object);
            estimator.Update();
            Assert.AreEqual(10, estimator.GetTimeStamp(0).timestamp.Ticks);
            Assert.AreEqual(160, estimator.GetTimeStamp(300).timestamp.Ticks);
        }        
    }
}
