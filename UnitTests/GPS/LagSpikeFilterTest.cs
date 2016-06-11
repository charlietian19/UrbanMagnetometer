using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Utils.GPS.Time;
using Utils.DataReader;
using Utils.Fixtures;
using Utils.GPS;

namespace UnitTests.GPS
{
    /// <summary>
    /// Summary description for DataChunkJitterFilterTest
    /// </summary>
    [TestClass]
    public class LagSpikeFilterTest
    {
        public LagSpikeFilterTest()
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

        Mock<IStopwatch> stopwatchMock;
        GpsData updatedGps = new GpsData
        {
            timestamp = new DateTime(1988, 12, 4),
            ticks = 6328176381
        };
        long frequency = 1000;

        [TestInitialize]
        public void Initialize()
        {
            stopwatchMock = new Mock<IStopwatch>();
            stopwatchMock.Setup(o => o.Frequency).Returns(frequency);
        }

        [TestMethod]
        public void ConstructorNormal()
        {
            var filter = new LagSpikeFilter(20);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorSizeTooSmall()
        {
            var filter = new LagSpikeFilter(1);
        }

        [TestMethod]
        public void FlushCausesStorageClear()
        {
            var storageMock = new Mock<IStorage<GpsDatasetChunk>>();

            storageMock.SetupAllProperties();
            storageMock.Setup(o => o.Clear());
            storageMock.Setup(o => o.Flush());
            var filter = new LagSpikeFilter(storageMock.Object,
                stopwatchMock.Object);
            filter.Flush();
            storageMock.Verify(o => o.Clear(), Times.Once());
            storageMock.Verify(o => o.Flush(), Times.Never());
        }

        [TestMethod]
        public void ClearCausesStorageClear()
        {
            var storageMock = new Mock<IStorage<GpsDatasetChunk>>();

            storageMock.SetupAllProperties();
            storageMock.Setup(o => o.Clear());
            var filter = new LagSpikeFilter(storageMock.Object,
                stopwatchMock.Object);
            filter.Clear();
            storageMock.Verify(o => o.Clear(), Times.Once());
        }

        [TestMethod]
        public void PushCausesStorageAdd()
        {
            var storageMock = new Mock<IStorage<GpsDatasetChunk>>();
            var chunk = MakeChunk(123);

            storageMock.SetupAllProperties();
            storageMock.Setup(o => o.Add(It.IsAny<GpsDatasetChunk>()));
            var filter = new LagSpikeFilter(storageMock.Object,
                stopwatchMock.Object);
            filter.InputData(chunk);
            storageMock.Verify(o => o.Add(It.IsAny<GpsDatasetChunk>()),
                Times.Once());
        }

        [TestMethod]
        public void StorageOverflowRaisesOnPop()
        {
            var storage = new FifoStorage<GpsDatasetChunk>(2);
            var chunk = MakeChunk(123);
            var filter = new LagSpikeFilter(storage, stopwatchMock.Object);
            GpsDatasetChunk calledWith = null;
            filter.OnPop += (data) => calledWith = data;
            storage.Add(chunk);
            storage.Add(chunk);
            storage.Add(chunk);
            Assert.AreEqual(chunk, calledWith);
        }

        [TestMethod]
        public void SampleDataNoLag()
        {
            var data = new long[] { 1000, 2003, 3001, 3998, 5000 };
            var storage = MakeStorage(data);
            var filter = new LagSpikeFilter(storage, stopwatchMock.Object);
            GpsDatasetChunk returnChunk = null;
            filter.ToleranceLow = 10 * 1e-3;
            filter.OnPop += o => returnChunk = o;
            filter.InputData(MakeChunk(6000));            
            Assert.AreEqual(data[0], returnChunk.Gps.ticks);
        }

        [TestMethod]
        public void SampleDataMiddleLag()
        {
            var data = new long[] { 1000, 2003, 3501, 3998, 5000 };
            var storage = MakeStorage(data);
            var filter = new LagSpikeFilter(storage, stopwatchMock.Object);
            GpsDatasetChunk returnChunk = null;
            filter.ToleranceLow = 10 * 1e-3;
            filter.OnPop += o => returnChunk = o;
            filter.InputData(MakeChunk(6000));
            Assert.AreEqual(data[0], returnChunk.Gps.ticks);
        }

        [TestMethod]
        public void SampleDataNewestLag()
        {
            var data = new long[] { 1000, 2003, 3501, 3998, 5000 };
            var storage = MakeStorage(data);
            var filter = new LagSpikeFilter(storage, stopwatchMock.Object);
            GpsDatasetChunk returnChunk = null;
            filter.ToleranceLow = 10 * 1e-3;
            filter.OnPop += o => returnChunk = o;
            filter.InputData(MakeChunk(8000));
            Assert.AreEqual(data[0], returnChunk.Gps.ticks);
        }

        [TestMethod]
        public void SampleDataOldestLag()
        {
            var data = new long[] { 1030, 2003, 3001, 3998, 5000 };
            var storage = MakeStorage(data);
            var filter = new LagSpikeFilter(storage, stopwatchMock.Object);
            GpsDatasetChunk returnChunk = null;
            filter.ToleranceLow = 10 * 1e-3;
            filter.OnPop += o => returnChunk = o;
            filter.InputData(MakeChunk(6000));
            Assert.AreEqual(1003, returnChunk.Gps.ticks);
            Assert.AreEqual(updatedGps.timestamp, returnChunk.Gps.timestamp);
        }


        /* Returns a GpsDatasetChunk given ticks */
        private GpsDatasetChunk MakeChunk(long ticks)
        {
            var estimatorMock = new Mock<ITimeEstimator>();
            estimatorMock.Setup(o => o.GetTimeStamp(It.IsAny<long>()))
                .Returns(updatedGps);
            return MakeChunk(ticks, estimatorMock);
        }

        /* Returns a GpsDatasetChunk given ticks and an estimator mock */
        private GpsDatasetChunk MakeChunk(long ticks,
            Mock<ITimeEstimator> estimatorMock)
        {
            var time = DateTime.Now;
            long index = 8712638;
            var gps = new GpsData
            {
                timestamp = time,
                ticks = ticks,
            };
            var xdata = new double[] { 1.2, 5.0, 2.1 };
            var ydata = new double[] { 3.2, 12.31, 532.1 };
            var zdata = new double[] { 62.3, 532.3, 123.2 };
            return new GpsDatasetChunk(gps, estimatorMock.Object, index,
                xdata, ydata, zdata);
        }

        /* Prepares a data storage given data arrival ticks. 
        The smaller is the index, the earlier the data arrived. */
        private FifoStorage<GpsDatasetChunk> MakeStorage(long[] ticks)
        {
            var storage = new FifoStorage<GpsDatasetChunk>(ticks.Length);
            for (int i = 0; i < ticks.Length; i++)
            {
                storage.Add(MakeChunk(ticks[i]));
            }
            return storage;
        }

    }
}
