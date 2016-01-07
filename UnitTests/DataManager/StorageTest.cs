using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.DataManager;
using Moq;
using SystemWrapper.IO;
using System.IO;
using SystemWrapper.Configuration;
using System.Collections.Specialized;


namespace UnitTests.DataManager
{
    [TestClass]
    public class StorageTest
    {
        /* Factory for IBinaryWriterWrap mock objects */
        public class BinaryWriterMockFactory : IBinaryWriterFactory
        {
            public Mock<IBinaryWriterWrap> x, y, z, t;
            private int count = 0;

            public IBinaryWriterWrap Create(IFileStreamWrap stream)
            {
                var mock = new Mock<IBinaryWriterWrap>();
                mock.Setup(o => o.Write(It.IsAny<long>()));
                mock.Setup(o => o.Write(It.IsAny<double>()));
                mock.Setup(o => o.Write(It.IsAny<int>()));
                mock.Setup(o => o.Close())
                    .Callback(() =>
                    {
                        mock.Setup(o => o.Write(It.IsAny<long>()))
                            .Throws(new IOException());
                        mock.Setup(o => o.Write(It.IsAny<double>()))
                            .Throws(new IOException());
                        mock.Setup(o => o.Write(It.IsAny<int>()))
                            .Throws(new IOException());
                    });

                switch (count % 4)
                {
                    case 0:
                        x = mock;
                        break;
                    case 1:
                        y = mock;
                        break;
                    case 2:
                        z = mock;
                        break;
                    case 3:
                        t = mock;
                        break;
                }

                count++;
                return mock.Object;
            }
        }

        private BinaryWriterMockFactory factory;
        private Mock<IFileWrap> FileMock;
        private Mock<IConfigurationManagerWrap> ConfigMock;
        private Mock<IUploadScheduler> schedulerMock;
        private Mock<IDirectoryWrap> directoryMock;
        private IFileWrap file;
        private IConfigurationManagerWrap config;
        private IUploadScheduler scheduler;
        private IDirectoryWrap dir;

        [TestInitialize]
        public void SetupMocks()
        {
            var settings = new NameValueCollection();
            settings["DataCacheFolder"] = ".";
            settings["StationName"] = "test";
            settings["SamplingRate"] = "1";
            settings["DataUnits"] = "au";
            settings["ChannelNameX"] = "x";
            settings["ChannelNameY"] = "y";
            settings["ChannelNameZ"] = "z";
            settings["ChannelNameTime"] = "t";
            settings["DataFileNameFormat"] =
                "{0}-{1}-{2}_{3}:xx_{4}_{5}_{6}Hz.bin";
            settings["TimeFileNameFormat"] = "{0}-{1}-{2}_{3}:xx_{4}.bin";
            settings["ZipFileNameFormat"] = "{0}-{1}-{2}_{3}:xx.zip";

            factory = new BinaryWriterMockFactory();
            FileMock = new Mock<IFileWrap>();
            ConfigMock = new Mock<IConfigurationManagerWrap>();
            schedulerMock = new Mock<IUploadScheduler>();
            directoryMock = new Mock<IDirectoryWrap>();
            FileMock.Setup(o => o.Open(It.IsAny<string>(),
                It.IsAny<FileMode>(), It.IsAny<FileAccess>()));
            ConfigMock.Setup(o => o.AppSettings).Returns(settings);
            schedulerMock.Setup(o => o.UploadMagneticData(It.IsAny<IDatasetInfo>()));
            directoryMock.Setup(o => o.Exists(It.IsAny<string>()))
                .Returns(true);
            directoryMock.Setup(o => o.CreateDirectory(It.IsAny<string>()))
                .Throws(new NotSupportedException());
            file = FileMock.Object;
            config = ConfigMock.Object;
            scheduler = schedulerMock.Object;
            dir = directoryMock.Object;
        }

        [TestMethod]
        public void WriteSinglePoint()
        {
            var storage = new Storage(scheduler, file, dir, factory, config);
            double[] x = { 0.0 }, y = { 2.0 }, z = { 3.0 };
            DateTime time = DateTime.Now;
            double seconds = 1245.0;

            storage.Store(x, y, z, seconds, time);
            factory.x.Verify(o => o.Write(0.0), Times.Once());
            factory.y.Verify(o => o.Write(2.0), Times.Once());
            factory.z.Verify(o => o.Write(3.0), Times.Once());
            factory.t.Verify(o => o.Write((long)0), Times.Once());
            factory.t.Verify(o => o.Write(1), Times.Once());
            factory.t.Verify(o => o.Write(time.ToBinary()), Times.Once());
            factory.t.Verify(o => o.Write(seconds), Times.Once());
        }

        [TestMethod]
        public void WriteSingleSample()
        {
            var storage = new Storage(scheduler, file, dir, factory, config);
            double[] x = { 0.0, 1.42, 213, 23, 12, 234.12, 56 },
                y = { 2.0, 21, 543.1, 23, 54, 22, 2.6 },
                z = { 3.0, 1.2324, 2.3, 21.2, 21.3, 4.0, 5.44 };
            DateTime time = DateTime.Now;
            double seconds = 23324.345;

            storage.Store(x, y, z, seconds, time);
            for (int i = 0; i < x.Length; i++)
            {
                var xval = x[i];
                var yval = y[i];
                var zval = z[i];
                factory.x.Verify(o => o.Write(xval), Times.Once());
                factory.y.Verify(o => o.Write(yval), Times.Once());
                factory.z.Verify(o => o.Write(zval), Times.Once());
            }

            factory.t.Verify(o => o.Write((long)0), Times.Once());
            factory.t.Verify(o => o.Write((int)x.Length), Times.Once());
            factory.t.Verify(o => o.Write(time.ToBinary()), Times.Once());
            factory.t.Verify(o => o.Write(seconds), Times.Once());
        }

        [TestMethod]
        public void WriteMultipleSamples()
        {
            var storage = new Storage(scheduler, file, dir, factory, config);
            double[] x1 = { 0, 0, 0 }, y1 = { 0, 0, 0 }, z1 = { 0, 0, 0 };
            double[] x2 = { 0, 0 }, y2 = { 0, 0 }, z2 = { 0, 0 };
            double[] x3 = { 0, 0, 0, 0 }, y3 = { 0, 0, 0, 0 }, 
                z3 = { 0, 0, 0, 0 };
            DateTime time = DateTime.Now;
            double seconds = 43;

            storage.Store(x1, y1, z1, seconds, time);
            factory.t.Verify(o => o.Write((long)0), Times.Once());
            factory.t.Verify(o => o.Write(x1.Length), Times.Once());

            storage.Store(x2, y2, z2, seconds, time);
            factory.t.Verify(o => o.Write((long)(x1.Length)), Times.Once());
            factory.t.Verify(o => o.Write(x2.Length), Times.Once());

            storage.Store(x3, y3, z3, seconds, time);
            factory.t.Verify(o => o.Write((long)(x1.Length + x2.Length)), 
                Times.Once());
            factory.t.Verify(o => o.Write(x3.Length), Times.Once());
        }

        [TestMethod]
        public void UploadWhenHourChanges()
        {
            var storage = new Storage(scheduler, file, dir, factory, config);
            double[] x = { 123.532 }, y = { 21.2 }, z = { 2.0 };
            DateTime time1, time2, time3;
            time1 = new DateTime(2016, 1, 1, 0, 0, 0);
            time2 = new DateTime(2016, 1, 1, 0, 15, 0); // same hour
            time3 = new DateTime(2016, 1, 1, 1, 0, 0);  // different hour
            double seconds = 125.54323;

            storage.Store(x, y, z, seconds, time1);
            var xWriter = factory.x;
            var yWriter = factory.y;
            var zWriter = factory.z;
            var tWriter = factory.z;
            storage.Store(x, y, z, seconds, time2);
            schedulerMock.Verify(o => o.UploadMagneticData(
                It.IsAny<IDatasetInfo>()), Times.Never());
            xWriter.Verify(o => o.Close(), Times.Never());
            yWriter.Verify(o => o.Close(), Times.Never());
            zWriter.Verify(o => o.Close(), Times.Never());
            tWriter.Verify(o => o.Close(), Times.Never());

            storage.Store(x, y, z, seconds, time3);
            schedulerMock.Verify(o => o.UploadMagneticData(
                It.IsAny<IDatasetInfo>()), Times.Once());
            xWriter.Verify(o => o.Close(), Times.Once());
            yWriter.Verify(o => o.Close(), Times.Once());
            zWriter.Verify(o => o.Close(), Times.Once());
            tWriter.Verify(o => o.Close(), Times.Once());
        }

        [TestMethod]
        public void WriteToNextFileWhenHourChanges()
        {
            var storage = new Storage(scheduler, file, dir, factory, config);
            double[] x = { 123.532 }, y = { 21.2 }, z = { 2.0 };
            DateTime time1, time2;
            time1 = new DateTime(2016, 1, 1, 0, 0, 0);
            time2 = new DateTime(2016, 1, 1, 1, 0, 0);  // different hour
            double seconds = 125.54323;

            storage.Store(x, y, z, seconds, time1);
            storage.Store(x, y, z, seconds, time2);
            factory.x.Verify(o => o.Write(123.532), Times.Once());
            factory.y.Verify(o => o.Write(21.2), Times.Once());
            factory.z.Verify(o => o.Write(2.0), Times.Once());
            factory.t.Verify(o => o.Write((long)0), Times.Once());
            factory.t.Verify(o => o.Write(1), Times.Once());
            factory.t.Verify(o => o.Write(time2.ToBinary()), Times.Once());
            factory.t.Verify(o => o.Write(seconds), Times.Once());
        }

        [TestMethod]
        public void ReopenFileAfterCloseCalled()
        {
            var storage = new Storage(scheduler, file, dir, factory, config);
            double[] x1 = { 123.532 }, y1 = { 21.2 }, z1 = { 2.0 };
            double[] x2 = { 52.2 }, y2 = { 54.2 }, z2 = { 213.6 };
            DateTime time = new DateTime(2016, 1, 1, 0, 0, 0);
            double seconds = 125.54323;

            storage.Store(x1, y1, z1, seconds, time);
            storage.Close();
            storage.Store(x2, y2, z2, seconds, time);

            factory.x.Verify(o => o.Write(52.2), Times.Once());
            factory.y.Verify(o => o.Write(54.2), Times.Once());
            factory.z.Verify(o => o.Write(213.6), Times.Once());
            factory.t.Verify(o => o.Write((long)0), Times.Once());
            factory.t.Verify(o => o.Write(1), Times.Once());
            factory.t.Verify(o => o.Write(time.ToBinary()), Times.Once());
            factory.t.Verify(o => o.Write(seconds), Times.Once());
        }
    }
}
