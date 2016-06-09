using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Utils.DataReader;
using SystemWrapper.IO;
using System.IO;
using SystemWrapper.Configuration;
using System.Collections.Specialized;
using Utils.Fixtures;

namespace UnitTests.DataReader
{
    [TestClass]
    public class DatasetReaderTest
    {
        /* Factory for IBinaryReaderWrap mock objects */
        public class BinaryReaderMockFactory : IBinaryReaderFactory
        {
            public class DatasetDataMock
            {
                public Mock<IBinaryReaderWrap> mock;
                public double[] data;
                public int ptr = 0;

                public DatasetDataMock(Mock<IBinaryReaderWrap> mock,
                    double[] data)
                {
                    mock.Setup(o => o.ReadDouble())
                        .Returns(() => data[ptr])
                        .Callback(() =>
                        {
                            ptr = (ptr + 1 == data.Length) ? 0 : ptr + 1;
                        });
                    this.mock = mock;
                    this.data = data;
                }
            }

            public class DatasetTimeMock
            {
                public Mock<IBinaryReaderWrap> mock;
                public static long timeUtc, index;
                public int length;
                public static double performanceCounter;
                private int count;

                private long ReadInt64()
                {
                    switch (count % 4)
                    {
                        case 0:
                            count++;
                            return index;
                        case 2:
                            count++;
                            return timeUtc;
                        default:
                            throw new InvalidOperationException();                            
                    }
                }

                private int ReadInt32()
                {
                    if (count % 4 == 1)
                    {
                        count++;
                        return length;
                    }
                    throw new InvalidOperationException();
                }

                private double ReadDouble()
                {
                    if (count % 4 == 3)
                    {
                        count++;
                        return performanceCounter;
                    }
                    throw new InvalidOperationException();
                }

                public DatasetTimeMock(Mock<IBinaryReaderWrap> mock, int length)
                {
                    mock.Setup(o => o.ReadInt64())
                        .Returns(() => ReadInt64());
                    mock.Setup(o => o.ReadInt32())
                        .Returns(() => ReadInt32());
                    mock.Setup(o => o.ReadDouble())
                        .Returns(() => ReadDouble());

                    this.mock = mock;
                    this.length = length;
                }
            }

            //public Mock<IBinaryReaderWrap> x, y, z, t;
            public DatasetDataMock x, y, z;
            public DatasetTimeMock t;

            public static double[] xData, yData, zData;
            public int xPtr, yPtr, zPtr;
            private int count = 0;
            private bool readStats = true;

            public IBinaryReaderWrap Create(IFileStreamWrap stream)
            {
                var mock = new Mock<IBinaryReaderWrap>();
                mock.Setup(o => o.Close()).Callback(() =>
                {
                    mock.Setup(o => o.Read())
                        .Throws(new IOException());
                });

                if (readStats)
                {
                    t = new DatasetTimeMock(mock, xData.Length);
                    readStats = false;
                    return t.mock.Object;
                }

                switch (count % 4)
                {
                    case 0:
                        x = new DatasetDataMock(mock, xData);
                        count++;
                        return x.mock.Object;
                    case 1:
                        y = new DatasetDataMock(mock, yData);
                        count++;
                        return y.mock.Object;
                    case 2:
                        z = new DatasetDataMock(mock, zData);
                        count++;
                        return z.mock.Object;
                    case 3:
                        t = new DatasetTimeMock(mock, xData.Length);
                        count++;
                        return t.mock.Object;
                    default:
                        throw new InvalidOperationException();
#pragma warning disable CS0162 // Unreachable code detected
                        return mock.Object;
#pragma warning restore CS0162 // Unreachable code detected
                }
            }
        }

        /* Factory for IFileInfoWrap objects */
        public class FileInfoMockFactory : IFileInfoFactory
        {
            public Mock<IFileInfoWrap> mock;

            public FileInfoMockFactory()
            {
                mock = new Mock<IFileInfoWrap>();
                mock.Setup(o => o.Length).Returns(0);
            }

            public IFileInfoWrap Create(string path)
            {
                return mock.Object;
            }
        }

        private BinaryReaderMockFactory readerFactory;
        private FileInfoMockFactory fileInfoFactory;
        private Mock<IFileWrap> FileMock;
        private Mock<IConfigurationManagerWrap> ConfigMock;
        private IFileWrap file;
        private IConfigurationManagerWrap config;

        [TestInitialize]
        public void SetupMocks()
        {
            var settings = new NameValueCollection();
            settings["LegacyChannelNameX"] = "raw_x.bin";
            settings["LegacyChannelNameY"] = "raw_y.bin";
            settings["LegacyChannelNameZ"] = "raw_z.bin";
            settings["LegacyChannelNameTime"] = "time.bin";

            readerFactory = new BinaryReaderMockFactory();
            fileInfoFactory = new FileInfoMockFactory();
            FileMock = new Mock<IFileWrap>();
            FileMock.Setup(o => o.Open(It.IsAny<string>(),
                It.IsAny<FileMode>(), It.IsAny<FileAccess>()));
            ConfigMock = new Mock<IConfigurationManagerWrap>();
            ConfigMock.Setup(o => o.AppSettings).Returns(settings);

            file = FileMock.Object;
            config = ConfigMock.Object;
        }

        [TestMethod]
        public void ReadSingleChunk()
        {
            double[] x = new double[] { 0.0, 1.0, 2.0, 3.0 };
            double[] y = new double[] { 4.2, 2.7, 1.2, 1.5 };
            double[] z = new double[] { 6.4, 5.3, 5.7, 1.8 };
            var time = new DateTime(2015, 12, 25, 13, 30, 0);

            BinaryReaderMockFactory.DatasetTimeMock.index = 23;
            BinaryReaderMockFactory.DatasetTimeMock.performanceCounter = 12.4;
            BinaryReaderMockFactory.DatasetTimeMock.timeUtc = time.ToBinary();
            BinaryReaderMockFactory.xData = x;
            BinaryReaderMockFactory.yData = y;
            BinaryReaderMockFactory.zData = z;

            var reader = new DatasetReader(file, fileInfoFactory, readerFactory);
            reader.OpenDataFiles("x", "y", "z", "t");
            var chunk = reader.GetNextChunk();
            
            CollectionAssert.AreEqual(x, chunk.XData);
            CollectionAssert.AreEqual(y, chunk.YData);
            CollectionAssert.AreEqual(z, chunk.ZData);
            Assert.AreEqual(23, chunk.Index);
            Assert.AreEqual(12.4, chunk.PerformanceCounter);
            Assert.AreEqual(time, chunk.Time);
        }

        [TestMethod]
        public void ReadSingleLegacyChunk()
        {
            double[] x = new double[] { 0.0, 1.0, 2.0, 3.0 };
            double[] y = new double[] { 4.2, 2.7, 1.2, 1.5 };
            double[] z = new double[] { 6.4, 5.3, 5.7, 1.8 };
            var time = new DateTime(2015, 12, 25, 13, 30, 0, 
                DateTimeKind.Local);

            BinaryReaderMockFactory.DatasetTimeMock.index = 52;
            BinaryReaderMockFactory.DatasetTimeMock.performanceCounter = 7.222;
            // the time is wrong due to a bug in the legacy code
            BinaryReaderMockFactory.DatasetTimeMock.timeUtc = time.ToFileTimeUtc();
            BinaryReaderMockFactory.xData = x;
            BinaryReaderMockFactory.yData = y;
            BinaryReaderMockFactory.zData = z;

            var reader = new LegacyReader(file, fileInfoFactory, readerFactory, 
                config);
            reader.OpenDataFiles("time.bin");
            var chunk = reader.GetNextChunk();

            CollectionAssert.AreEqual(x, chunk.XData);
            CollectionAssert.AreEqual(y, chunk.YData);
            CollectionAssert.AreEqual(z, chunk.ZData);
            Assert.AreEqual(52, chunk.Index);
            Assert.AreEqual(7.222, chunk.PerformanceCounter);
            Assert.AreEqual(time, chunk.Time);
        }
    }
}
