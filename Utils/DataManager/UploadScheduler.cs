using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using SystemWrapper.Configuration;
using SystemWrapper.IO;
using SystemWrapper.Threading;
using Utils.GDrive;
using System.Diagnostics;
using Utils.Fixtures;

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

    

    public class UploadScheduler : IUploadScheduler
    {
        protected int maxActiveUploads, maxRetryCount, waitBetweenRetriesSeconds,
            maxDelayBeforeUploadSeconds,
            minDelayBetweenFailedRetriesSeconds,
            maxDelayBetweenFailedRetriesSeconds;
        protected bool EnableDelayBeforeUpload { get; set; }
        protected bool enableFailedRetryWorker;
        private string dataCacheFolder, zipFileNameFormat;
        private string remoteRoot;
        private int ActiveUploadCount = 0;
        protected BlockingCollection<IDatasetInfo> queue;
        protected ConcurrentQueue<IDatasetInfo> queueFailed
            = new ConcurrentQueue<IDatasetInfo>();
        protected IUploader uploader;
        protected IConfigurationManagerWrap ConfigurationManager;
        protected IFileWrap File;
        protected IDirectoryWrap Directory;
        protected IPathWrap Path;
        protected IThreadWrap ThreadWrap;
        protected IZipFile zip;
        protected AutoResetEvent retryEvent = new AutoResetEvent(false);
        public event UploadFinishedEventHandler FinishedEvent;
        public event UploadStartedEventHandler StartedEvent;
        protected IAutoResetEvent[] workerSemaphores;
        private bool flushing = false;

        protected string failedPath
        {
            get { return Path.Combine(dataCacheFolder, "failed"); }
        }

        /* Signal all the upload worker threads to proceed the upload
        Typically called before exiting the application
        */
        public void Flush()
        {
            flushing = true;
            retryEvent.Set();
            foreach (var sema in workerSemaphores)
            {
                sema.Set();
            }
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
            remoteRoot = settings["RemoteRoot"];

            EnableDelayBeforeUpload = Convert.ToBoolean(
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
            Debug.WriteLine("Launching background upload workers");
            workerSemaphores = new AutoResetEventWrapper[maxActiveUploads];
            for (int i = 0; i < maxActiveUploads; i++)
            {
                workerSemaphores[i].Reset();
                Task.Run(() => Worker(i));
            }

            if (enableFailedRetryWorker)
            {
                Debug.WriteLine("Launching failed upload worker");
                Task.Run(() => RetryWorker());
            }
        }

        /* Uploads the files given DatasetInfo. */
        private void UploadDo(IDatasetInfo info)
        {
            OnStarted(info);
            string parent = Path.Combine(remoteRoot, info.RemotePath);
            for (int i = 0; i < maxRetryCount; i++)
            {
                try
                {
                    uploader.Upload(info.ArchivePath, parent);
                }
                catch (Exception e)
                {
                    if ((i + 1 >= maxRetryCount) || flushing)
                    {
                        ProcessFailedUpload(info);
                        OnFinished(info, false, e.Message);
                        return;
                    }
                    else
                    {
                        Debug.WriteLine("Upload failed: " + e.Message);
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
        private void Worker(int mySemaphoreId)
        {
            Thread thread = Thread.CurrentThread;
            thread.Priority = ThreadPriority.Lowest;
            Debug.WriteLine(string.Format("Background worker {0} has started",
                thread.ManagedThreadId));
            while (!queue.IsCompleted)
            {
                IDatasetInfo info = null;
                try
                {
                    info = queue.Take();

                    if (info != null)
                    {
                        info.ArchiveFiles();
                        Debug.WriteLine(string.Format(
                            "Worker {0} received a new dataset to upload, "
                            + "StartDate = {1}, ArchivePath = {2}",
                            thread.ManagedThreadId, info.StartDate,
                            info.ArchivePath));

                        if (EnableDelayBeforeUpload && !flushing)
                        {
                            Random rnd = new Random();
                            var delay = rnd.Next(
                                maxDelayBeforeUploadSeconds * 1000);
                            Debug.WriteLine(string.Format(
                                "Sleeping for {0}ms before starting the upload",
                                delay));
                            workerSemaphores[mySemaphoreId].WaitOne(delay);
                        }

                        Interlocked.Increment(ref ActiveUploadCount);
                        UploadDo(info);
                        Interlocked.Decrement(ref ActiveUploadCount);
                    }
                    else
                    {
                        Debug.WriteLine(string.Format(
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
            Debug.WriteLine("Triggering retryEvent");
            retryEvent.Set();
        }

        /* Adds all files in "failed" folder into retryQueue. */
        private void EnqueueFailed()
        {
            Debug.WriteLine("Enqueuing failed uploads");
            try
            {
                var failed = Directory.GetFiles(failedPath, "*"
                    + Path.GetExtension(zipFileNameFormat));
                Debug.WriteLine("Files found in " + failedPath);
                foreach (var name in failed)
                {
                    try
                    {
                        Debug.WriteLine("Enqueuing " + name);
                        var info = new DatasetInfo(name, ConfigurationManager,
                            zip, File, Directory, Path);
                        queueFailed.Enqueue(info);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        /* Retries the failed uploads. */
        private void RetryWorker()
        {
            Thread thread = Thread.CurrentThread;
            thread.Priority = ThreadPriority.Lowest;
            Debug.WriteLine(string.Format("Background worker {0} has started",
                thread.ManagedThreadId));
            var rnd = new Random();
            while (true)
            {
                try
                {
                    var delay = rnd.Next(
                        minDelayBetweenFailedRetriesSeconds * 1000,
                        maxDelayBetweenFailedRetriesSeconds * 1000);
                    Debug.WriteLine(string.Format(
                        "Sleeping for {0} ms before retrying the failed uploads",
                        delay));
                    retryEvent.WaitOne(delay);

                    var count = queueFailed.Count;
                    Debug.WriteLine(string.Format(
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
                    Debug.WriteLine(e.Message);
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
            Debug.WriteLine(string.Format(
                "Started uploading dataset {0}", info.ArchivePath));
            if (StartedEvent == null)
                return;
            StartedEvent(info);
        }

        /* Invoke the event when an upload finishes. */
        protected virtual void OnFinished(IDatasetInfo info, bool success,
            string msg)
        {
            Debug.WriteLine(string.Format(
                "Done uploading dataset {0}, success = {1}, msg = {2}",
                info.ArchivePath, success, msg));
            if (FinishedEvent == null)
                return;
            FinishedEvent(info, success, msg);
        }
    }

}
