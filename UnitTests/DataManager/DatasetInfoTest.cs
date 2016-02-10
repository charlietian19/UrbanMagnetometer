using System;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SystemWrapper.Configuration;
using Utils.DataManager;
using Moq;

namespace UnitTests.DataManager
{
    [TestClass]
    public class DatasetInfoTest
    {
        private Mock<IConfigurationManagerWrap> ConfigMock;
        private IConfigurationManagerWrap config;
        NameValueCollection settings;

        [TestInitialize]
        public void SetupMocks()
        {
            settings = new NameValueCollection();
            settings["DataCacheFolder"] = @"D:\MagneticFieldData";
            settings["StationName"] = "test";
            settings["SamplingRate"] = "412";
            settings["DataUnits"] = "au";
            settings["ChannelNameX"] = "x";
            settings["ChannelNameY"] = "y";
            settings["ChannelNameZ"] = "z";
            settings["ChannelNameTime"] = "t";
            settings["DataFileNameFormat"] =
                "{0}-{1}-{2}_{3}-xx_{4}_{5}_{6}Hz.bin";
            settings["TimeFileNameFormat"] = "{0}-{1}-{2}_{3}-xx_{4}.bin";
            settings["ZipFileNameFormat"] = "{0}-{1}-{2}_{3}-xx.zip";

            ConfigMock = new Mock<IConfigurationManagerWrap>();
            ConfigMock.Setup(o => o.AppSettings).Returns(settings);
            config = ConfigMock.Object;
        }

        [TestMethod]
        public void CreateFromFileName()
        {
            var name = "2014-05-23_12-xx.zip";
            var data = new DatasetInfo(name, config);
            var time = new DateTime(2014, 5, 23, 12, 0, 0);
            Assert.AreEqual("2014", data.Year);
            Assert.AreEqual("5", data.Month);
            Assert.AreEqual("23", data.Day);
            Assert.AreEqual("12", data.Hour);
            Assert.AreEqual("test", data.StationName);
            Assert.AreEqual(name, data.ArchivePath);
            Assert.AreEqual(time, data.StartDate);
        }

        [TestMethod]
        public void CreateFromFileNameFullPath()
        {
            var name = @"D:\NURI\cache\failed\2014-05-23_12-xx.zip";
            var data = new DatasetInfo(name, config);
            var time = new DateTime(2014, 5, 23, 12, 0, 0);
            Assert.AreEqual("2014", data.Year);
            Assert.AreEqual("5", data.Month);
            Assert.AreEqual("23", data.Day);
            Assert.AreEqual("12", data.Hour);
            Assert.AreEqual("test", data.StationName);
            Assert.AreEqual(name, data.ArchivePath);
            Assert.AreEqual(time, data.StartDate);
        }

        [TestMethod]
        public void CreateAndReadDateTime()
        {
            // year, month, day, hour, minute, second
            var time = new DateTime(2016, 10, 25, 22, 55, 47);
            var data = new DatasetInfo(time, config);
            Assert.AreEqual("2016", data.Year);
            Assert.AreEqual("10", data.Month);
            Assert.AreEqual("25", data.Day);
            Assert.AreEqual("22", data.Hour);
            Assert.AreEqual(time, data.StartDate);
        }

        [TestMethod]
        public void ReadsParamsFromConfig()
        {
            var data = new DatasetInfo(DateTime.Now, config);
            Assert.AreEqual(settings["DataUnits"], data.Units);
            Assert.AreEqual(settings["SamplingRate"], data.SamplingRate);
            Assert.AreEqual(settings["DataCacheFolder"], data.FolderPath);
            Assert.AreEqual(settings["StationName"], data.StationName);
        }

        [TestMethod]
        public void DataFileNameFormat()
        {
            // year, month, day, hour, minute, second
            var time = new DateTime(2016, 10, 25, 22, 55, 47);
            var data = new DatasetInfo(time, config);
            Assert.AreEqual("2016-10-25_22-xx_x_au_412Hz.bin", data.XFileName);
            Assert.AreEqual("2016-10-25_22-xx_y_au_412Hz.bin", data.YFileName);
            Assert.AreEqual("2016-10-25_22-xx_z_au_412Hz.bin", data.ZFileName);
            Assert.AreEqual("2016-10-25_22-xx_t.bin", data.TFileName);
            Assert.AreEqual("2016-10-25_22-xx.zip", data.ZipFileName);
        }

        [TestMethod]
        public void DataFileFullPath()
        {
            var data = new DatasetInfo(DateTime.Now, config);
            Assert.AreEqual(@"D:\MagneticFieldData\SomeDataFile.zip", 
                data.FullPath("SomeDataFile.zip"));
        }

        [TestMethod]
        public void isSameFileOtherDataset()
        {
            // year, month, day, hour, minute, second
            var time1 = new DateTime(2016, 10, 25, 22, 55, 47);
            var time2 = new DateTime(2016, 10, 25, 22, 0, 0);
            var time3 = new DateTime(2015, 10, 25, 22, 0, 0);
            var time4 = new DateTime(2016, 9, 25, 22, 0, 0);
            var time5 = new DateTime(2016, 10, 14, 22, 0, 0);
            var time6 = new DateTime(2016, 10, 25, 17, 0, 0);
            var data1 = new DatasetInfo(time1, config);
            var data2 = new DatasetInfo(time2, config);
            var data3 = new DatasetInfo(time3, config);
            var data4 = new DatasetInfo(time4, config);
            var data5 = new DatasetInfo(time5, config);
            var data6 = new DatasetInfo(time6, config);
            Assert.IsTrue(data1.isSameFile(data2));
            Assert.IsFalse(data1.isSameFile(data3));
            Assert.IsFalse(data1.isSameFile(data4));
            Assert.IsFalse(data1.isSameFile(data5));
            Assert.IsFalse(data1.isSameFile(data6));
        }

        [TestMethod]
        public void isSameFileOtherDateTime()
        {
            // year, month, day, hour, minute, second
            var time1 = new DateTime(2016, 10, 25, 22, 55, 47);
            var time2 = new DateTime(2016, 10, 25, 22, 0, 0);
            var time3 = new DateTime(2015, 10, 25, 22, 0, 0);
            var time4 = new DateTime(2016, 9, 25, 22, 0, 0);
            var time5 = new DateTime(2016, 10, 14, 22, 0, 0);
            var time6 = new DateTime(2016, 10, 25, 17, 0, 0);
            var data1 = new DatasetInfo(time1, config);
            Assert.IsTrue(data1.isSameFile(time2));
            Assert.IsFalse(data1.isSameFile(time3));
            Assert.IsFalse(data1.isSameFile(time4));
            Assert.IsFalse(data1.isSameFile(time5));
            Assert.IsFalse(data1.isSameFile(time6));
        }
    }
}
