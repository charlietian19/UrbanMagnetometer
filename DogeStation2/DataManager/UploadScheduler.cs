using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

/* Example code taken from 

    https://msdn.microsoft.com/en-us/library/dd997371(v=vs.110).aspx

*/

// TODO: proper exception handling

namespace GDriveNURI
{
    public class UploadScheduler
    {
        private int maxActiveUploads;
        private BlockingCollection<DatasetInfo> queue;
        private IUploader uploader;

        /* Initializes settings from the configuration file. */
        private void ReadAppConfig()
        {
            var settings = System.Configuration.ConfigurationManager.AppSettings;
            maxActiveUploads = Convert.ToInt32(settings["MaxActiveUploads"]);
        }

        /* Starts the worker threads. */
        private void StartWorkerThreads()
        {
            for (int i = 0; i < maxActiveUploads; i++)
            {
                Task.Run(() => Worker());
            }
        }

        /* Creates an uploader with no queue bound. */
        public UploadScheduler(IUploader uploader)
        {
            ReadAppConfig();
            this.uploader = uploader;
            queue = new BlockingCollection<DatasetInfo>();
            StartWorkerThreads();
        }

        /* Creates an uploader with a pre-defined queue bound. */
        public UploadScheduler(IUploader uploader, int maxQueueLength)
        {
            ReadAppConfig();
            this.uploader = uploader;
            queue = new BlockingCollection<DatasetInfo>(maxQueueLength);
            StartWorkerThreads();
        }

        /* Creates a new temporary directory in the folder containing data. */
        private Mutex tmpDirMutex = new Mutex();
        private string CreateTemporaryDirectory(DatasetInfo info)
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
        private void DeleteTemporaryDirectory(String name)
        {
            tmpDirMutex.WaitOne();
            Directory.Delete(name, true);
            tmpDirMutex.ReleaseMutex();
        }


        /* Adds the magnetic field dataset to an archive and returns full path
        to the archive. */
        private String Archive(DatasetInfo info)
        {
            String tmpDirFullPath, newXFileName, newYFileName, newZFileName,
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

            ZipFile.CreateFromDirectory(tmpDirFullPath, archiveName);
            DeleteTemporaryDirectory(tmpDirFullPath);
            return archiveName;
        }

        /* Creates a directory tree corresponding to the dataset information
        and returns the id of the folder to place the file into. */
        private String CreateDirectoryTree(DatasetInfo info)
        {
            // TODO: implement this
            return null;
        }

        /* Retrieves the arriving data in background. */
        private void Worker()
        {
            while (!queue.IsCompleted)
            {
                DatasetInfo info = null;
                try
                {
                    info = queue.Take();
                }
                catch (InvalidOperationException) { }

                if (info != null)
                {
                    String filePath = Archive(info);
                    String parentId = CreateDirectoryTree(info);
                    uploader.Upload(filePath, parentId);
                }
            }
        }

        /* Queues uploading a single magnetic field dataset to be uploaded. */
        public void UploadMagneticData(DatasetInfo info)
        {
            queue.Add(info);
        }
    }

}
