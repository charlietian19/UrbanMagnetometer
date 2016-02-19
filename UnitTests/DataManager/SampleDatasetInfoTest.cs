using System;
using Utils.DataManager;
using Utils.Configuration;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SystemWrapper.Configuration;
using SystemWrapper.IO;
using System.IO;
using Moq;

namespace UnitTests.DataManager
{

    [TestClass]
    public class SampleDatasetInfoTest
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
            Settings.SampleName = @"Test Sample\test";
            Settings.SampleComment = "Test Comment";

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
                .Returns<string>(x => Path.GetFileName(x));
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
        public void ZipFileName()
        {
            var time = new DateTime(2016, 10, 25, 22, 55, 47);
            var data = new SampleDatasetInfo(time, config, zip,
                file, dir, path);
            Assert.AreEqual(@"Signal Samples\Test Sample\test",
                data.RemotePath);
        }

        [TestMethod]
        public void RemotePath()
        {
            var time = new DateTime(2016, 10, 25, 22, 55, 47);
            var data = new SampleDatasetInfo(time, config, zip,
                file, dir, path);
            Assert.AreEqual("test.zip", data.ZipFileName);
        }

        [TestMethod]
        public void AppendComment()
        {
            var commentPath = @"D:\MagneticFieldData\random0\comment.txt";
            fileMock.Setup(o => o.WriteAllText(commentPath,
                Settings.SampleComment));
            var time = new DateTime(2016, 10, 25, 22, 55, 47);
            var data = new SampleDatasetInfo(time, config, zip,
                file, dir, path);
            data.ArchiveFiles();
            fileMock.Verify(o => o.WriteAllText(commentPath,
                Settings.SampleComment), Times.Once());
        }
    }
}
