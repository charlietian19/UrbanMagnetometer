using System;
using Utils.DataManager;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SystemWrapper.Configuration;
using SystemWrapper.IO;
using System.IO;
using Moq;
using Utils.Fixtures;

namespace UnitTests.DataManager
{
    [TestClass]
    public class DatasetInfoTest
    {
        private Mock<IConfigurationManagerWrap> ConfigMock;
        private Mock<IZipFile> zipMock;
        private Mock<IFileWrap> fileMock;
        private Mock<IDirectoryWrap> directoryMock;
        private Mock<IPathWrap> pathMock;
        private IConfigurationManagerWrap config;
        private IZipFile zip;
        private IFileWrap file;
        private IDirectoryWrap dir;
        private IPathWrap path;
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

            // ConfigMock setup
            ConfigMock = new Mock<IConfigurationManagerWrap>();
            ConfigMock.Setup(o => o.AppSettings).Returns(settings);

            // DirectoryMock setup
            directoryMock = new Mock<IDirectoryWrap>();
            directoryMock.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            directoryMock.Setup(o => o.CreateDirectory(It.IsAny<string>()));
            directoryMock.Setup(o => o.Delete(It.IsAny<string>(),
                It.IsAny<bool>()));
            directoryMock.Setup(
                o => o.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((path, pattern) => new string[] { });

            // DirectoryMock setup
            fileMock = new Mock<IFileWrap>();
            fileMock.Setup(o => o.Move(It.IsAny<string>(), It.IsAny<string>()));
            fileMock.Setup(o => o.Delete(It.IsAny<string>()));

            // PathMock setup
            pathMock = new Mock<IPathWrap>();
            pathMock.Setup(o => o.Combine(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((s1, s2) => Path.Combine(s1, s2));
            pathMock.Setup(o => o.GetRandomFileName()).Returns("random0");
            pathMock.Setup(o => o.GetDirectoryName(It.IsAny<string>()))
                .Returns("");
            pathMock.Setup(o => o.GetFileName(It.IsAny<string>()))
                .Returns<string>(x => x);
            pathMock.Setup(o => o.GetFullPath(It.IsAny<string>()))
                .Returns<string>(x => x);

            // ZipMock setup
            zipMock = new Mock<IZipFile>();
            zipMock.Setup(o => o.CreateFromDirectory(It.IsAny<string>(),
                It.IsAny<string>()));

            file = fileMock.Object;
            dir = directoryMock.Object;
            path = pathMock.Object;
            config = ConfigMock.Object;
            zip = zipMock.Object;
        }

        [TestMethod]
        public void CreateFromFileName1()
        {
            var name = "2014-05-23_12-xx.zip";
            var data = new DatasetInfo(name, config, zip, file, dir, path);
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
        public void CreateFromFileName2()
        {
            var name = "2016-2-5_0-xx.zip";
            var data = new DatasetInfo(name, config, zip, file, dir, path);
            var time = new DateTime(2016, 2, 5, 0, 0, 0);
            Assert.AreEqual("2016", data.Year);
            Assert.AreEqual("2", data.Month);
            Assert.AreEqual("5", data.Day);
            Assert.AreEqual("0", data.Hour);
            Assert.AreEqual("test", data.StationName);
            Assert.AreEqual(name, data.ArchivePath);
            Assert.AreEqual(time, data.StartDate);
        }

        [TestMethod]
        public void CreateFromFileNameFullPath()
        {
            var name = @"D:\NURI\cache\failed\2014-05-23_12-xx.zip";
            var data = new DatasetInfo(name, config, zip, file, dir, path);
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
            var data = new DatasetInfo(time, config, zip, file, dir, path);
            Assert.AreEqual("2016", data.Year);
            Assert.AreEqual("10", data.Month);
            Assert.AreEqual("25", data.Day);
            Assert.AreEqual("22", data.Hour);
            Assert.AreEqual(time, data.StartDate);
        }

        [TestMethod]
        public void ReadsParamsFromConfig()
        {
            var data = new DatasetInfo(DateTime.Now, config, zip, file,
                dir, path);
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
            var data = new DatasetInfo(time, config, zip, file, dir, path);
            Assert.AreEqual("2016-10-25_22-xx_x_au_412Hz.bin", data.XFileName);
            Assert.AreEqual("2016-10-25_22-xx_y_au_412Hz.bin", data.YFileName);
            Assert.AreEqual("2016-10-25_22-xx_z_au_412Hz.bin", data.ZFileName);
            Assert.AreEqual("2016-10-25_22-xx_t.bin", data.TFileName);
            Assert.AreEqual("2016-10-25_22-xx.zip", data.ZipFileName);
        }

        [TestMethod]
        public void DataFileFullPath()
        {
            var data = new DatasetInfo(DateTime.Now, config, zip,
                file, dir, path);
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
            var data1 = new DatasetInfo(time1, config, zip, file, dir, path);
            var data2 = new DatasetInfo(time2, config, zip, file, dir, path);
            var data3 = new DatasetInfo(time3, config, zip, file, dir, path);
            var data4 = new DatasetInfo(time4, config, zip, file, dir, path);
            var data5 = new DatasetInfo(time5, config, zip, file, dir, path);
            var data6 = new DatasetInfo(time6, config, zip, file, dir, path);
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
            var blah = time6.ToBinary();
            var data1 = new DatasetInfo(time1, config, zip, file, dir, path);
            Assert.IsTrue(data1.isSameFile(time2));
            Assert.IsFalse(data1.isSameFile(time3));
            Assert.IsFalse(data1.isSameFile(time4));
            Assert.IsFalse(data1.isSameFile(time5));
            Assert.IsFalse(data1.isSameFile(time6));
        }

        [TestMethod]
        public void ArchiveFilesTmpDirectoryDoesntExist()
        {
            var time = new DateTime(2016, 9, 25, 22, 0, 0);
            var originalPath = settings["DataCacheFolder"];
            var randomPath = originalPath + "\\random0";
            pathMock.Setup(o => o.Combine(originalPath, "random0"))
                .Returns(randomPath);
            directoryMock.Setup(o => o.Exists(randomPath)).Returns(false);
            var data = new DatasetInfo(time, config, zip, file, dir, path);
            data.ArchiveFiles();

            pathMock.Verify(o => o.Combine(originalPath, "random0"),
                Times.Once());
            directoryMock.Verify(o => o.Exists(randomPath), Times.Once());
            CheckArchiveCreation(data, randomPath, originalPath);
        }

        [TestMethod]
        public void ArchiveFilesTmpDirectoryExists()
        {
            var time = new DateTime(2016, 9, 25, 22, 0, 0);
            var originalPath = settings["DataCacheFolder"];
            var randomPath = originalPath + "\\random0";
            pathMock.Setup(o => o.Combine(originalPath, "random0"))
                .Returns(randomPath);
            directoryMock.SetupSequence(o => o.Exists(randomPath))
                .Returns(true).Returns(true).Returns(false);
            var data = new DatasetInfo(time, config, zip, file, dir, path);
            data.ArchiveFiles();

            pathMock.Verify(o => o.Combine(originalPath, "random0"),
                Times.Exactly(3));
            directoryMock.Verify(o => o.Exists(randomPath), Times.Exactly(3));
            CheckArchiveCreation(data, randomPath, originalPath);
        }

        private void CheckArchiveCreation(DatasetInfo data, string randomPath,
            string originalPath)
        {
            directoryMock.Verify(o => o.CreateDirectory(randomPath),
                Times.Once());
            pathMock.Verify(o => o.Combine(randomPath, data.XFileName),
                Times.Once());
            pathMock.Verify(o => o.Combine(randomPath, data.YFileName),
                Times.Once());
            pathMock.Verify(o => o.Combine(randomPath, data.ZFileName),
                Times.Once());
            pathMock.Verify(o => o.Combine(randomPath, data.TFileName),
                Times.Once());
            fileMock.Verify(o => o.Move(originalPath + "\\" + data.XFileName,
                randomPath + "\\" + data.XFileName), Times.Once());
            fileMock.Verify(o => o.Move(originalPath + "\\" + data.YFileName,
                randomPath + "\\" + data.YFileName), Times.Once());
            fileMock.Verify(o => o.Move(originalPath + "\\" + data.ZFileName,
                randomPath + "\\" + data.ZFileName), Times.Once());
            fileMock.Verify(o => o.Move(originalPath + "\\" + data.TFileName,
                randomPath + "\\" + data.TFileName), Times.Once());
            zipMock.Verify(o => o.CreateFromDirectory(randomPath,
                originalPath + "\\" + data.ZipFileName), Times.Once());
            directoryMock.Verify(o => o.Delete(randomPath, true), Times.Once());
            Assert.AreEqual(originalPath + "\\" + data.ZipFileName,
                data.ArchivePath);
        }
    }
}
