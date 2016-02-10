using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using SystemWrapper.Configuration;
using SystemWrapper.IO;
using SystemWrapper.Threading;
using System.IO.Compression;
using Utils.GDrive;

/* Example code taken from

    https://msdn.microsoft.com/en-us/library/dd997371(v=vs.110).aspx
    https://msdn.microsoft.com/en-us/library/aa645739(v=vs.71).aspx

*/

// TODO: proper exception handling

namespace Utils.DataManager
{
    public delegate void UploadStartedEventHandler(IDatasetInfo info);
    public delegate void UploadFinishedEventHandler(IDatasetInfo info,
        bool success, string message);

    public interface IUploadScheduler
    {
        void UploadMagneticData(IDatasetInfo info);
        int ActiveUploads { get; }
        void RetryFailed();
        event UploadFinishedEventHandler FinishedEvent;
        event UploadStartedEventHandler StartedEvent;
    }

    public interface IZipFile
    {
        void CreateFromDirectory(string src, string dst);
    }

    /* Adapter to wrap ZipFile class. */
    public class ZipFileWrapper : IZipFile
    {
        public void CreateFromDirectory(string src, string dst)
        {
            ZipFile.CreateFromDirectory(src, dst);
        }
    }

    public class UploadScheduler : IUploadScheduler
    {
        private int maxActiveUploads, maxRetryCount, waitBetweenRetriesSeconds,
            maxDelayBeforeUploadSeconds,
            minDelayBetweenFailedRetriesSeconds,
            maxDelayBetweenFailedRetriesSeconds;
        bool enableDelayBeforeUpload, enableFailedRetryWorker;
        private string dataCacheFolder, zipFileNameFormat;
        private string remoteFileName;
        private int ActiveUploadCount = 0;
        private BlockingCollection<IDatasetInfo> queue;
        private ConcurrentQueue<IDatasetInfo> queueFailed
            = new ConcurrentQueue<IDatasetInfo>();
        private IUploader uploader;
        private IConfigurationManagerWrap ConfigurationManager;
        private IFileWrap File;
        private IDirectoryWrap Directory;
        private IZipFile zip;
        private IPathWrap Path;
        private IThreadWrap ThreadWrap;
        private AutoResetEvent retryEvent = new AutoResetEvent(false);
        public event UploadFinishedEventHandler FinishedEvent;
        public event UploadStartedEventHandler StartedEvent;
        private string failedPath
        {
            get { return Path.Combine(dataCacheFolder, "failed"); }
        }


        public int ActiveUploads
        {
            get { return ActiveUploadCount; }
        }

        /* Creates an uploader with no queue bound. */
        public UploadScheduler(IUploader uploader)
        {
            UseSystemWrapper();
            InitNoQueueBound(uploader);
        }

        /* Creates an uploader with a pre-defined queue bound. */
        public UploadScheduler(IUploader uploader, int maxQueueLength)
        {
            UseSystemWrapper();
            InitWithQueueBound(uploader, maxQueueLength);
        }

        /* Creates an uploader that uses provided wrappers and no queue bound*/
        public UploadScheduler(IUploader uploader,
            IConfigurationManagerWrap config, IFileWrap file, IDirectoryWrap dir,
            IZipFile zip, IPathWrap path, IThreadWrap thread)
        {
            UseCustomWrapper(config, file, dir, zip, path, thread);
            InitNoQueueBound(uploader);
        }

        /* Creates an uploader that uses provided wrappers and a queue bound*/
        public UploadScheduler(IUploader uploader, int maxQueueLength,
            IConfigurationManagerWrap config, IFileWrap file, IDirectoryWrap dir,
            IZipFile zip, IPathWrap path, IThreadWrap thread)
        {
            UseCustomWrapper(config, file, dir, zip, path, thread);
            InitWithQueueBound(uploader, maxQueueLength);
        }

        /* Assigns the wrappers to use the system objects. */
        private void UseSystemWrapper()
        {
            ConfigurationManager = new ConfigurationManagerWrap();
            File = new FileWrap();
            Directory = new DirectoryWrap();
            zip = new ZipFileWrapper();
            Path = new PathWrap();
            ThreadWrap = new ThreadWrap();
        }

        /* Assigns the wrappers to use the provided objects. */
        private void UseCustomWrapper(IConfigurationManagerWrap config,
            IFileWrap file, IDirectoryWrap dir, IZipFile zip, IPathWrap path,
            IThreadWrap thread)
        {
            ConfigurationManager = config;
            File = file;
            Directory = dir;
            this.zip = zip;
            Path = path;
            ThreadWrap = thread;
        }

        /* Initializes uploader with no queue bound. */
        private void InitNoQueueBound(IUploader uploader)
        {
            ReadAppConfig();
            this.uploader = uploader;
            queue = new BlockingCollection<IDatasetInfo>();
            EnqueueFailed();
            StartWorkerThreads();
        }

        /* Initializes uploader with a queue bound. */
        private void InitWithQueueBound(IUploader uploader, int maxQueueLength)
        {
            ReadAppConfig();
            this.uploader = uploader;
            queue = new BlockingCollection<IDatasetInfo>(maxQueueLength);
            EnqueueFailed();
            StartWorkerThreads();
        }
        /* Initializes settings from the configuration file. */
        private void ReadAppConfig()
        {
            var settings = ConfigurationManager.AppSettings;
            maxActiveUploads = Convert.ToInt32(settings["MaxActiveUploads"]);
            maxRetryCount = Convert.ToInt32(settings["MaxRetryCount"]);
            waitBetweenRetriesSeconds = Convert.ToInt32(
                settings["WaitBetweenRetriesSeconds"]);
            remoteFileName = settings["RemoteFileNameFormat"];

            enableDelayBeforeUpload = Convert.ToBoolean(
                settings["EnableDelayBeforeUpload"]);
            maxDelayBeforeUploadSeconds = Convert.ToInt32(
                settings["MaxDelayBeforeUploadSeconds"]);

            enableFailedRetryWorker = Convert.ToBoolean(
                settings["EnableFailedRetryWorker"]);
            minDelayBetweenFailedRetriesSeconds = Convert.ToInt32(
                settings["MinDelayBetweenFailedRetriesSeconds"]);
            maxDelayBetweenFailedRetriesSeconds = Convert.ToInt32(
                settings["MaxDelayBetweenFailedRetriesSeconds"]);

            dataCacheFolder = settings["DataCacheFolder"];
            zipFileNameFormat = settings["ZipFileNameFormat"];
        }

        /* Starts the worker threads. */
        private void StartWorkerThreads()
        {
            Console.WriteLine("Launching background upload workers");
            for (int i = 0; i < maxActiveUploads; i++)
            {
                Task.Run(() => Worker());
            }

            if (enableFailedRetryWorker)
            {
                Console.WriteLine("Launching failed upload worker");
                Task.Run(() => RetryWorker());
            }
        }

        /* Creates a new temporary directory in the folder containing data. */
        private Mutex tmpDirMutex = new Mutex();
        private string CreateTemporaryDirectory(IDatasetInfo info)
        {
            string name = null;
            bool success = false;
            tmpDirMutex.WaitOne();
            while (!success)
            {
                name = Path.Combine(info.FolderPath, Path.GetRandomFileName());
                if (!Directory.Exists(name))
                {
                    Directory.CreateDirectory(name);
                    success = true;
                }
            }
            tmpDirMutex.ReleaseMutex();
            return name;
        }

        /* Removes the temporary directory. */
        private void DeleteTemporaryDirectory(string name)
        {
            tmpDirMutex.WaitOne();
            Directory.Delete(name, true);
            tmpDirMutex.ReleaseMutex();
        }


        /* Adds the magnetic field dataset to an archive and returns full path
        to the archive. */
        private string MoveDataToTmpDir(IDatasetInfo info)
        {
            string tmpDirFullPath, newXFileName, newYFileName, newZFileName,
                newTFileName;

            tmpDirFullPath = CreateTemporaryDirectory(info);
            newXFileName = Path.Combine(tmpDirFullPath, info.XFileName);
            newYFileName = Path.Combine(tmpDirFullPath, info.YFileName);
            newZFileName = Path.Combine(tmpDirFullPath, info.ZFileName);
            newTFileName = Path.Combine(tmpDirFullPath, info.TFileName);

            File.Move(info.FullPath(info.XFileName), newXFileName);
            File.Move(info.FullPath(info.YFileName), newYFileName);
            File.Move(info.FullPath(info.ZFileName), newZFileName);
            File.Move(info.FullPath(info.TFileName), newTFileName);

            return tmpDirFullPath;
        }

        /* Adds the files from the dataset into a zip archive. */
        private string ArchiveFiles(IDatasetInfo info)
        {
            string tmpDirFullPath = "";
            string path = Path.Combine(info.FolderPath, info.ZipFileName);
            tmpDirFullPath = MoveDataToTmpDir(info);
            zip.CreateFromDirectory(tmpDirFullPath, path);
            DeleteTemporaryDirectory(tmpDirFullPath);
            return path;
        }

        /* Uploads the files given DatasetInfo. */
        private void UploadDo(IDatasetInfo info)
        {
            OnStarted(info);
            string parent = string.Format(remoteFileName, info.Year,
                            info.Month, info.Day, info.Hour, info.StationName);
            for (int i = 0; i < maxRetryCount; i++)
            {
                try
                {
                    uploader.Upload(info.ArchivePath, parent);
                }
                catch (Exception e)
                {
                    if (i + 1 >= maxRetryCount)
                    {
                        ProcessFailedUpload(info);
                        OnFinished(info, false, e.Message);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Upload failed: " + e.Message);
                        ThreadWrap.Sleep(waitBetweenRetriesSeconds * 1000);
                        continue;
                    }
                }

                File.Delete(info.ArchivePath);
                OnFinished(info, true, "Upload successful");
                return;
            }
        }

        /* Moves the files into the failed uploads folder and adds
        it to the failed files queue. */
        private void ProcessFailedUpload(IDatasetInfo info)
        {
            var file = Path.GetFileName(info.ArchivePath);
            var dstDir = failedPath;
            if (!Directory.Exists(dstDir))
            {
                Directory.CreateDirectory(dstDir);
            }

            var dst = Path.Combine(dstDir, file);
            if (Path.GetFullPath(dst) != Path.GetFullPath(info.ArchivePath))
            {
                File.Move(info.ArchivePath, dst);
                info.ArchivePath = dst;
            }

            queueFailed.Enqueue(info);
        }

        /* Retrieves the arriving data in background. */
        private void Worker()
        {
            Thread thread = Thread.CurrentThread;
            thread.Priority = ThreadPriority.Lowest;
            Console.WriteLine(string.Format("Background worker {0} has started",
                thread.ManagedThreadId));
            while (!queue.IsCompleted)
            {
                IDatasetInfo info = null;
                try
                {
                    info = queue.Take();

                    if (info != null)
                    {
                        info.ArchivePath = ArchiveFiles(info);
                        Console.WriteLine(string.Format(
                            "Worker {0} received a new dataset to upload, "
                            + "StartDate = {1}, ArchivePath = {2}",
                            thread.ManagedThreadId, info.StartDate,
                            info.ArchivePath));

                        if (enableDelayBeforeUpload)
                        {
                            Random rnd = new Random();
                            ThreadWrap.Sleep(rnd.Next(
                                maxDelayBeforeUploadSeconds * 1000));
                            Console.WriteLine(string.Format(
                                "Sleeping for {0}ms before starting the upload"));
                        }

                        Interlocked.Increment(ref ActiveUploadCount);
                        UploadDo(info);
                        Interlocked.Decrement(ref ActiveUploadCount);
                    }
                    else
                    {
                        Console.WriteLine(string.Format(
                            "Worker {0} received a null dataset to upload",
                            thread.ManagedThreadId));
                    }
                }
                catch (InvalidOperationException)
                {
                    return;
                }
                catch (Exception e)
                {
                    if (info != null)
                    {
                        OnFinished(info, false, e.Message);
                    }
                }
            }
        }

        /* Triggers uploading of the files for which upload has failed. */
        public void RetryFailed()
        {
            Console.WriteLine("Triggering retryEvent");
            retryEvent.Set();
        }

        /* Adds all files in "failed" folder into retryQueue. */
        private void EnqueueFailed()
        {
            Console.WriteLine("Enqueuing failed uploads");
            try
            {
                var failed = Directory.GetFiles(failedPath, "*"
                    + Path.GetExtension(zipFileNameFormat));
                Console.WriteLine("Files found in " + failedPath);
                foreach (var name in failed)
                {
                    try
                    {
                        Console.WriteLine("Enqueuing " + name);
                        var info = new DatasetInfo(name, ConfigurationManager);
                        queueFailed.Enqueue(info);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /* Retries the failed uploads. */
        private void RetryWorker()
        {
            Thread thread = Thread.CurrentThread;
            thread.Priority = ThreadPriority.Lowest;
            Console.WriteLine(string.Format("Background worker {0} has started",
                thread.ManagedThreadId));
            var rnd = new Random();
            while (true)
            {
                try
                {
                    var delay = rnd.Next(
                        minDelayBetweenFailedRetriesSeconds * 1000,
                        maxDelayBetweenFailedRetriesSeconds * 1000);
                    Console.WriteLine(string.Format(
                        "Sleeping for {0} ms before retrying the failed uploads",
                        delay));
                    retryEvent.WaitOne(delay);

                    var count = queueFailed.Count;
                    Console.WriteLine(string.Format(
                        "RetryWoker wakes up, there are {0} datasets in the queue",
                        count));
                    for (int i = 0; i < count; i++)
                    {
                        IDatasetInfo info = null;
                        if (!queueFailed.TryDequeue(out info))
                        {
                            break;
                        }

                        Interlocked.Increment(ref ActiveUploadCount);
                        UploadDo(info);
                        Interlocked.Decrement(ref ActiveUploadCount);
                    }
                    retryEvent.Reset();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /* Queues uploading a single magnetic field dataset to be uploaded. */
        public void UploadMagneticData(IDatasetInfo info)
        {
            queue.Add(info);
        }

        /* Invoke the event when an upload starts. */
        protected virtual void OnStarted(IDatasetInfo info)
        {
            Console.WriteLine(string.Format(
                "Started uploading dataset {0}", info.ArchivePath));
            if (StartedEvent == null)
                return;
            StartedEvent(info);
        }

        /* Invoke the event when an upload finishes. */
        protected virtual void OnFinished(IDatasetInfo info, bool success,
            string msg)
        {
            Console.WriteLine(string.Format(
                "Done uploading dataset {0}, success = {1}, msg = {2}",
                info.ArchivePath, success, msg));
            if (FinishedEvent == null)
                return;
            FinishedEvent(info, success, msg);
        }
    }

}
