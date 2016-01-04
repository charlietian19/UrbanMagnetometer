using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using SystemWrapper.Configuration;
using SystemWrapper.IO;
using System.IO.Compression;


/* Example code taken from 

    https://msdn.microsoft.com/en-us/library/dd997371(v=vs.110).aspx

*/

// TODO: proper exception handling

namespace GDriveNURI
{
    public interface IUploadScheduler
    {
        void UploadMagneticData(IDatasetInfo info);
        EventWaitHandle UploadStarted { get; }
        EventWaitHandle UploadFinished { get; }
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
        private int maxActiveUploads;
        private string remoteFileName;
        private int ActiveUploads = 0;
        private readonly EventWaitHandle UploadStartedEventHandle
            = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly EventWaitHandle UploadFinishedEventHandle
            = new EventWaitHandle(false, EventResetMode.AutoReset);
        private BlockingCollection<IDatasetInfo> queue;
        private IUploader uploader;
        private IConfigurationManagerWrap ConfigurationManager;
        private IFileWrap File;
        private IDirectoryWrap Directory;
        private IZipFile zip;
        private IPathWrap Path;
        public EventWaitHandle UploadStarted
        {
            get { return UploadStartedEventHandle; }
        }
        public EventWaitHandle UploadFinished
        {
            get { return UploadFinishedEventHandle; }
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
            IZipFile zip, IPathWrap path)
        {
            UseCustomWrapper(config, file, dir, zip, path);
            InitNoQueueBound(uploader);
        }

        /* Creates an uploader that uses provided wrappers and a queue bound*/
        public UploadScheduler(IUploader uploader, int maxQueueLength,
            IConfigurationManagerWrap config, IFileWrap file, IDirectoryWrap dir,
            IZipFile zip, IPathWrap path)
        {
            UseCustomWrapper(config, file, dir, zip, path);
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
        }

        /* Assigns the wrappers to use the provided objects. */
        private void UseCustomWrapper(IConfigurationManagerWrap config, 
            IFileWrap file, IDirectoryWrap dir, IZipFile zip, IPathWrap path)
        {
            ConfigurationManager = config;
            File = file;
            Directory = dir;
            this.zip = zip;
            Path = path;
        }

        /* Initializes uploader with no queue bound. */
        private void InitNoQueueBound(IUploader uploader)
        {
            ReadAppConfig();
            this.uploader = uploader;
            queue = new BlockingCollection<IDatasetInfo>();
            StartWorkerThreads();
        }

        /* Initializes uploader with a queue bound. */
        private void InitWithQueueBound(IUploader uploader, int maxQueueLength)
        {
            ReadAppConfig();
            this.uploader = uploader;
            queue = new BlockingCollection<IDatasetInfo>(maxQueueLength);
            StartWorkerThreads();
        }
        /* Initializes settings from the configuration file. */
        private void ReadAppConfig()
        {
            var settings = ConfigurationManager.AppSettings;
            maxActiveUploads = Convert.ToInt32(settings["MaxActiveUploads"]);
            remoteFileName = settings["RemoteFileNameFormat"];
        }

        /* Starts the worker threads. */
        private void StartWorkerThreads()
        {
            for (int i = 0; i < maxActiveUploads; i++)
            {
                Task.Run(() => Worker());
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
        private string Archive(IDatasetInfo info)
        {
            string tmpDirFullPath, newXFileName, newYFileName, newZFileName,
                newTFileName, archiveName;

            tmpDirFullPath = CreateTemporaryDirectory(info);
            newXFileName = Path.Combine(tmpDirFullPath, info.XFileName);
            newYFileName = Path.Combine(tmpDirFullPath, info.YFileName);
            newZFileName = Path.Combine(tmpDirFullPath, info.ZFileName);
            newTFileName = Path.Combine(tmpDirFullPath, info.TFileName);
            archiveName = Path.Combine(info.FolderPath, info.ZipFileName);

            File.Move(info.FullPath(info.XFileName), newXFileName);
            File.Move(info.FullPath(info.YFileName), newYFileName);
            File.Move(info.FullPath(info.ZFileName), newZFileName);
            File.Move(info.FullPath(info.TFileName), newTFileName);

            zip.CreateFromDirectory(tmpDirFullPath, archiveName);
            DeleteTemporaryDirectory(tmpDirFullPath);
            return archiveName;
        }


        /* Retrieves the arriving data in background. */
        private void Worker()
        {
            while (!queue.IsCompleted)
            {
                IDatasetInfo info = null;
                try
                {
                    info = queue.Take();
                }
                catch (InvalidOperationException) { }

                if (info != null)
                {
                    Interlocked.Increment(ref ActiveUploads);
                    UploadStartedEventHandle.Set();
                    string filePath = Archive(info);
                    string parent = string.Format(remoteFileName, info.Year,
                        info.Month, info.Day, info.Hour, info.StationName);
                    uploader.Upload(filePath, parent);
                    File.Delete(filePath);
                    Interlocked.Decrement(ref ActiveUploads);
                    UploadFinishedEventHandle.Set();
                }
            }
        }

        /* Queues uploading a single magnetic field dataset to be uploaded. */
        public void UploadMagneticData(IDatasetInfo info)
        {
            queue.Add(info);
        }
    }

}
