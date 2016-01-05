using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SystemWrapper.Configuration;
using SystemWrapper.IO;
using System.Threading;
using System.IO;
using System.Collections.Specialized;
using Utils.DataManager;
using Utils.GDrive;
using Moq;

namespace UnitTests.DataManager
{
    [TestClass]
    public class UploadSchedulerTest
    {
        private Mock<IConfigurationManagerWrap> configMock;
        private Mock<IUploader> uploaderMock;
        private Mock<IFileWrap> fileMock;
        private Mock<IDirectoryWrap> directoryMock;
        private Mock<IZipFile> zipMock;
        private Mock<IDatasetInfo> infoMock;
        private Mock<IPathWrap> pathMock;

        private IConfigurationManagerWrap config;
        private IUploader uploader;
        private IFileWrap file;
        private IDirectoryWrap dir;
        private IZipFile zip;
        private IDatasetInfo info;
        private IPathWrap path;

        private Semaphore finished;
        int timeout;

        private int count;

        NameValueCollection settings;

        /* Signals the main test thread that the worker is done. */
        private void SignalFinished(IDatasetInfo info,
            bool success, string msg)
        {
            finished.Release();
        }

        [TestInitialize]
        public void SetupMocks()
        {
            settings = new NameValueCollection();
            settings["MaxActiveUploads"] = "5";
            settings["MaxRetryCount"] = "3";
            settings["WaitBetweenRetriesSeconds"] = "0";
            settings["RemoteFileNameFormat"] = @"\{0}\{1}\{2}\{3}\{4}";
            count = 0;
            timeout = 10000;
            finished = new Semaphore(0, 1);

            configMock = new Mock<IConfigurationManagerWrap>();
            uploaderMock = new Mock<IUploader>();
            fileMock = new Mock<IFileWrap>();
            directoryMock = new Mock<IDirectoryWrap>();
            zipMock = new Mock<IZipFile>();
            infoMock = new Mock<IDatasetInfo>();
            pathMock = new Mock<IPathWrap>();
            uploaderMock.Setup(o => o.Upload(It.IsAny<string>(),
                It.IsAny<string>()));
            fileMock.Setup(o => o.Move(It.IsAny<string>(), It.IsAny<string>()));
            fileMock.Setup(o => o.Delete(It.IsAny<string>()));
            directoryMock.Setup(o => o.Exists(It.IsAny<string>())).Returns(false);
            directoryMock.Setup(o => o.CreateDirectory(It.IsAny<string>()));
            directoryMock.Setup(o => o.Delete(It.IsAny<string>(),
                It.IsAny<bool>())).Callback(() => count++);
            zipMock.Setup(o => o.CreateFromDirectory(It.IsAny<string>(), 
                It.IsAny<string>()));
            configMock.Setup(o => o.AppSettings).Returns(settings);
            infoMock.Setup(o => o.Year).Returns("2015");
            infoMock.Setup(o => o.Month).Returns("5");
            infoMock.Setup(o => o.Day).Returns("30");
            infoMock.Setup(o => o.Hour).Returns("15");
            infoMock.Setup(o => o.StationName).Returns("TestStation");
            infoMock.Setup(o => o.FullPath(It.IsAny<string>()))
                .Returns<string>(x => x);
            infoMock.Setup(o => o.XFileName)
                .Returns(string.Format("x{0}.bin", count));
            infoMock.Setup(o => o.YFileName)
                .Returns(string.Format("y{0}.bin", count));
            infoMock.Setup(o => o.ZFileName)
                .Returns(string.Format("z{0}.bin", count));
            infoMock.Setup(o => o.TFileName)
                .Returns(string.Format("t{0}.bin", count));
            infoMock.Setup(o => o.ZipFileName)
                .Returns(string.Format("data{0}.zip", count));
            infoMock.Setup(o => o.FolderPath).Returns("");
            pathMock.Setup(o => o.Combine(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((s1, s2) => Path.Combine(s1, s2));
            pathMock.Setup(o => o.GetRandomFileName())
                .Returns(string.Format("random{0}", count));

            config = configMock.Object;
            uploader = uploaderMock.Object;
            file = fileMock.Object;
            dir = directoryMock.Object;
            zip = zipMock.Object;
            info = infoMock.Object;
            path = pathMock.Object;
        }

        [TestMethod]
        public void UploadSingleFileSuccess()
        {
            var scheduler = new UploadScheduler(uploader, config, file,
                dir, zip, path);
            scheduler.FinishedEvent += 
                new UploadFinishedEventHandler(SignalFinished);
            scheduler.UploadMagneticData(info);
            finished.WaitOne(timeout);

            directoryMock.Verify(o => o.Exists("random0"), Times.Once());
            directoryMock.Verify(o => o.CreateDirectory("random0"),
                Times.Once());
            fileMock.Verify(o => o.Move("x0.bin", @"random0\x0.bin"), 
                Times.Once());
            fileMock.Verify(o => o.Move("y0.bin", @"random0\y0.bin"),
                Times.Once());
            fileMock.Verify(o => o.Move("z0.bin", @"random0\z0.bin"),
                Times.Once());
            fileMock.Verify(o => o.Move("t0.bin", @"random0\t0.bin"),
                Times.Once());
            zipMock.Verify(o => o.CreateFromDirectory("random0", "data0.zip"),
                Times.Once());
            directoryMock.Verify(o => o.Delete("random0", true), Times.Once());
            uploaderMock.Verify(o => o.Upload("data0.zip",
                @"\2015\5\30\15\TestStation"), Times.Once());
            fileMock.Verify(o => o.Delete("data0.zip"), Times.Once());
        }


        [TestMethod]
        public void UploadSingleFileCompressionFailed()
        {
            zipMock.Setup(o => o.CreateFromDirectory(It.IsAny<string>(),
                It.IsAny<string>())).Throws(new FileNotFoundException());
            var scheduler = new UploadScheduler(uploader, config, file,
                dir, zip, path);
            scheduler.FinishedEvent +=
                new UploadFinishedEventHandler(SignalFinished);
            scheduler.UploadMagneticData(info);
            finished.WaitOne(timeout);

            directoryMock.Verify(o => o.Exists("random0"), Times.Once());
            directoryMock.Verify(o => o.CreateDirectory("random0"),
                Times.Once());
            fileMock.Verify(o => o.Move("x0.bin", @"random0\x0.bin"),
                Times.Once());
            fileMock.Verify(o => o.Move("y0.bin", @"random0\y0.bin"),
                Times.Once());
            fileMock.Verify(o => o.Move("z0.bin", @"random0\z0.bin"),
                Times.Once());
            fileMock.Verify(o => o.Move("t0.bin", @"random0\t0.bin"),
                Times.Once());
            zipMock.Verify(o => o.CreateFromDirectory("random0", "data0.zip"),
                Times.Once());
            directoryMock.Verify(o => o.Delete("random0", true), Times.Never());
            uploaderMock.Verify(o => o.Upload("data0.zip",
                @"\2015\5\30\15\TestStation"), Times.Never());
            fileMock.Verify(o => o.Delete("data0.zip"), Times.Never());
        }

        [TestMethod]
        public void UploadSingleFileUploadFailed()
        {
            uploaderMock.Setup(o => o.Upload(It.IsAny<string>(),
                It.IsAny<string>())).Throws(new FileUploadException());
            var scheduler = new UploadScheduler(uploader, config, file,
                dir, zip, path);
            scheduler.FinishedEvent +=
                new UploadFinishedEventHandler(SignalFinished);
            scheduler.UploadMagneticData(info);
            finished.WaitOne(timeout);

            directoryMock.Verify(o => o.Exists("random0"), Times.Once());
            directoryMock.Verify(o => o.CreateDirectory("random0"),
                Times.Once());
            fileMock.Verify(o => o.Move("x0.bin", @"random0\x0.bin"),
                Times.Once());
            fileMock.Verify(o => o.Move("y0.bin", @"random0\y0.bin"),
                Times.Once());
            fileMock.Verify(o => o.Move("z0.bin", @"random0\z0.bin"),
                Times.Once());
            fileMock.Verify(o => o.Move("t0.bin", @"random0\t0.bin"),
                Times.Once());
            zipMock.Verify(o => o.CreateFromDirectory("random0", "data0.zip"),
                Times.Once());
            directoryMock.Verify(o => o.Delete("random0", true), Times.Once());
            uploaderMock.Verify(o => o.Upload("data0.zip",
                @"\2015\5\30\15\TestStation"), Times.Exactly(3));
            fileMock.Verify(o => o.Delete("data0.zip"), Times.Never());
        }
    }
}
