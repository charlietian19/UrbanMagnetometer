using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SystemWrapper.Configuration;
using SystemWrapper.Threading;
using System.Threading;
using System.IO;
using SystemWrapper.IO;
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
        private Mock<IDatasetInfo> infoMock;
        private Mock<IThreadWrap> threadMock;
        private Mock<IZipFile> zipMock;
        private Mock<IFileWrap> fileMock;
        private Mock<IDirectoryWrap> directoryMock;
        private Mock<IPathWrap> pathMock;
        private IConfigurationManagerWrap config;
        private IZipFile zip;
        private IFileWrap file;
        private IDirectoryWrap dir;
        private IPathWrap path;
        private IUploader uploader;
        private IDatasetInfo info;
        private IThreadWrap thread;

        private Semaphore finished;
        int timeout;        

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
            settings["WaitBetweenRetriesSeconds"] = "234";
            settings["StationName"] = "TestStation";
            settings["RemoteRoot"] = @"\root";
            settings["MaxDelayBeforeUploadSeconds"] = "352";
            settings["EnableDelayBeforeUpload"] = "false";
            settings["EnableFailedRetryWorker"] = "false";
            settings["MinDelayBetweenFailedRetriesSeconds"] = "36000";
            settings["MaxDelayBetweenFailedRetriesSeconds"] = "36000";
            settings["DataCacheFolder"] = "";
            settings["ZipFileNameFormat"] = "{0}-{1}-{2}_{3}-xx.zip";

            timeout = 10000;
            finished = new Semaphore(0, 1);

            configMock = new Mock<IConfigurationManagerWrap>();
            configMock.Setup(o => o.AppSettings).Returns(settings);

            // UploaderMock setup
            uploaderMock = new Mock<IUploader>();
            uploaderMock.Setup(o => o.Upload(It.IsAny<string>(),
                It.IsAny<string>()));

            // ThreadMock setup
            threadMock = new Mock<IThreadWrap>();
            threadMock.Setup(o => o.Sleep(It.IsAny<int>()));

            // InfoMock setup
            infoMock = new Mock<IDatasetInfo>();
            infoMock.Setup(o => o.Year).Returns("2015");
            infoMock.Setup(o => o.Month).Returns("5");
            infoMock.Setup(o => o.Day).Returns("30");
            infoMock.Setup(o => o.Hour).Returns("15");
            infoMock.Setup(o => o.StationName).Returns("TestStation");
            infoMock.Setup(o => o.FullPath(It.IsAny<string>()))
                .Returns<string>(x => x);
            infoMock.Setup(o => o.XFileName).Returns("x0.bin");
            infoMock.Setup(o => o.YFileName).Returns("y0.bin");
            infoMock.Setup(o => o.ZFileName).Returns("z0.bin");
            infoMock.Setup(o => o.TFileName).Returns("t0.bin");
            infoMock.Setup(o => o.ZipFileName).Returns("data0.zip");
            infoMock.SetupProperty(o => o.ArchivePath);
            infoMock.Object.ArchivePath = "data0.zip";
            infoMock.Setup(o => o.FolderPath).Returns("");
            infoMock.Setup(o => o.RemotePath).Returns("RemotePath");
            infoMock.Setup(o => o.ArchiveFiles());

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

            config = configMock.Object;
            uploader = uploaderMock.Object;
            info = infoMock.Object;
            thread = threadMock.Object;
            file = fileMock.Object;
            dir = directoryMock.Object;
            path = pathMock.Object;
            zip = zipMock.Object;
        }

        [TestMethod]
        public void RetryFailedSingleSuccess()
        {
            string filePath = @"failed\2014-3-2_14-xx.zip";
            settings["EnableFailedRetryWorker"] = "true";
            directoryMock.Setup(o => o.GetFiles(
                It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>(
                (path, pattern) => new string[] { filePath });
            var scheduler = new UploadScheduler(uploader, config, file,
                dir, zip, path, thread);
            scheduler.FinishedEvent +=
                new UploadFinishedEventHandler(SignalFinished);
            scheduler.RetryFailed();
            finished.WaitOne(timeout);
            uploaderMock.Verify(o => o.Upload(filePath,
                @"\root\2014\3\2\14\TestStation"), Times.Once());
            fileMock.Verify(o => o.Delete(filePath), 
                Times.Once());
            threadMock.Verify(o => o.Sleep(It.IsAny<int>()), Times.Never());
            infoMock.Verify(o => o.ArchiveFiles(), Times.Never());
        }

        [TestMethod]
        public void RetryFailedMultipleSuccess()
        {
            string fp1 = @"failed\2014-3-2_14-xx.zip";
            string fp2 = @"failed\2015-05-23_09-xx.zip";
            string fp3 = @"failed\2010-8-31_1-xx.zip";
            var filePathList = new string[] { fp1, fp2, fp3 };
            settings["EnableFailedRetryWorker"] = "true";
            directoryMock.Setup(o => o.GetFiles(
                It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>(
                (path, pattern) => filePathList);
            var scheduler = new UploadScheduler(uploader, config, file,
                dir, zip, path, thread);
            scheduler.FinishedEvent +=
                new UploadFinishedEventHandler(SignalFinished);
            scheduler.RetryFailed();
            finished.WaitOne(timeout);
            uploaderMock.Verify(o => o.Upload(fp1,
                @"\root\2014\3\2\14\TestStation"), Times.Once());
            fileMock.Verify(o => o.Delete(fp1), Times.Once());
            uploaderMock.Verify(o => o.Upload(fp2,
                @"\root\2015\5\23\9\TestStation"), Times.Once());
            fileMock.Verify(o => o.Delete(fp2), Times.Once());
            uploaderMock.Verify(o => o.Upload(fp3,
                @"\root\2010\8\31\1\TestStation"), Times.Once());
            fileMock.Verify(o => o.Delete(fp3), Times.Once());
            threadMock.Verify(o => o.Sleep(It.IsAny<int>()), Times.Never());
            infoMock.Verify(o => o.ArchiveFiles(), Times.Never());
        }

        [TestMethod]
        public void RetryFailedSingleFail()
        {
            string filePath = @"failed\2014-3-2_14-xx.zip";
            var retries = Convert.ToInt32(settings["MaxRetryCount"]);
            var sleepMs = Convert.ToInt32(
                settings["WaitBetweenRetriesSeconds"]) * 1000;
            settings["EnableFailedRetryWorker"] = "true";
            pathMock.Setup(o => o.GetFileName(@"failed\2014-3-2_14-xx.zip"))
                .Returns(@"2014-3-2_14-xx.zip");
            uploaderMock.Setup(o => o.Upload(It.IsAny<string>(),
                It.IsAny<string>())).Throws(new FileUploadException());
            directoryMock.Setup(o => o.GetFiles(
                It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>(
                (path, pattern) => new string[] { filePath });
            directoryMock.Setup(o => o.Exists("failed")).Returns(true);
            var scheduler = new UploadScheduler(uploader, config, file,
                dir, zip, path, thread);
            scheduler.FinishedEvent +=
                new UploadFinishedEventHandler(SignalFinished);
            scheduler.RetryFailed();
            finished.WaitOne(timeout);
            uploaderMock.Verify(o => o.Upload(filePath,
                @"\root\2014\3\2\14\TestStation"), Times.Exactly(retries));
            threadMock.Verify(o => o.Sleep(sleepMs),
                Times.Exactly(retries - 1));
            fileMock.Verify(o => o.Delete(filePath), Times.Never());
            directoryMock.Verify(o => o.CreateDirectory("failed"), Times.Never());
            fileMock.Verify(o => o.Move(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
            infoMock.Verify(o => o.ArchiveFiles(), Times.Never());
        }

        [TestMethod]
        public void UploadSingleFileSuccessWithDelay()
        {
            settings["EnableDelayBeforeUpload"] = "true";
            var scheduler = new UploadScheduler(uploader, config, file,
                dir, zip, path, thread);
            scheduler.FinishedEvent +=
                new UploadFinishedEventHandler(SignalFinished);
            scheduler.UploadMagneticData(info);
            finished.WaitOne(timeout);
            threadMock.Verify(o => o.Sleep(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void UploadSingleFileSuccessNoDelay()
        {            
            var scheduler = new UploadScheduler(uploader, config, file,
                dir, zip, path, thread);            
            scheduler.FinishedEvent += 
                new UploadFinishedEventHandler(SignalFinished);
            scheduler.UploadMagneticData(info);
            finished.WaitOne(timeout);
            infoMock.Verify(o => o.ArchiveFiles(), Times.Once());
            threadMock.Verify(o => o.Sleep(It.IsAny<int>()), Times.Never());
            uploaderMock.Verify(o => o.Upload("data0.zip",
                @"\root\RemotePath"), Times.Once());
            fileMock.Verify(o => o.Delete("data0.zip"), Times.Once());
        }


        [TestMethod]
        public void UploadSingleFileCompressionFailed()
        {
            infoMock.Setup(o => o.ArchiveFiles())
                .Throws(new FileNotFoundException());
            var scheduler = new UploadScheduler(uploader, config, file,
                dir, zip, path, thread);
            scheduler.FinishedEvent +=
                new UploadFinishedEventHandler(SignalFinished);
            scheduler.UploadMagneticData(info);
            finished.WaitOne(timeout);

            infoMock.Verify(o => o.ArchiveFiles(), Times.Once());
            directoryMock.Verify(o => o.Delete("random0", true), Times.Never());
            uploaderMock.Verify(o => o.Upload("data0.zip", It.IsAny<string>()),
                Times.Never());
            threadMock.Verify(o => o.Sleep(It.IsAny<int>()), Times.Never());
            fileMock.Verify(o => o.Delete("data0.zip"), Times.Never());
        }

        [TestMethod]
        public void UploadSingleFileUploadFailed()
        {
            var retries = Convert.ToInt32(settings["MaxRetryCount"]);
            uploaderMock.Setup(o => o.Upload(It.IsAny<string>(),
                It.IsAny<string>())).Throws(new FileUploadException());
            var scheduler = new UploadScheduler(uploader, config, file,
                dir, zip, path, thread);
            var sleepMs = Convert.ToInt32(
                settings["WaitBetweenRetriesSeconds"]) * 1000;
            scheduler.FinishedEvent +=
                new UploadFinishedEventHandler(SignalFinished);
            scheduler.UploadMagneticData(info);
            finished.WaitOne(timeout);

            infoMock.Verify(o => o.ArchiveFiles(), Times.Once());
            uploaderMock.Verify(o => o.Upload("data0.zip",
                @"\root\RemotePath"), Times.Exactly(retries));
            threadMock.Verify(o => o.Sleep(sleepMs), 
                Times.Exactly(retries - 1));
            fileMock.Verify(o => o.Delete("data0.zip"), Times.Never());
            directoryMock.Verify(o => o.Exists("failed"), Times.Once());
            directoryMock.Verify(o => o.CreateDirectory("failed"), Times.Once());
            fileMock.Verify(o => o.Move("data0.zip", @"failed\data0.zip"),
                Times.Once());
            Assert.AreEqual(info.ArchivePath, @"failed\data0.zip");
        }
    }
}
