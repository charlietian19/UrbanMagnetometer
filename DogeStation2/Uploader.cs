using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

/* Example code taken from 

    https://developers.google.com/drive/v2/web/quickstart/dotnet 
    https://developers.google.com/api-client-library/dotnet/guide/media_upload

*/

    // TODO: proper exception handling

namespace GDriveNURI
{
    public class Uploader
    {
        private UserCredential credential;
        private DriveService service;
        private string[] Scopes = { DriveService.Scope.Drive };
        private string credentialDirectory, filesMimeType, folderMimeType, 
            googleAuthUser;
        private int maxListResults;
        private BlockingCollection<DatasetInfo> queue;

        /* Initializes settings from the configuration file. */
        private void ReadAppConfig()
        {
            var settings = System.Configuration.ConfigurationManager.AppSettings;
            credentialDirectory = settings["CredentialDirectory"];
            filesMimeType = settings["FilesMimeType"];
            folderMimeType = settings["FoldersMimeType"];
            googleAuthUser = settings["GoogleAuthUser"];
            maxListResults = Convert.ToInt32(settings["MaxListResults"]);
        }

        /* Creates an uploader with no queue bound. */
        public Uploader(string ApplicationName, string secretPath)
        {
            ReadAppConfig();
            GoogleDriveInit(ApplicationName, secretPath);
            queue = new BlockingCollection<DatasetInfo>();
        }

        /* Creates an uploader with a pre-defined queue bound. */
        public Uploader(string ApplicationName, string secretPath, int maxQueueLength)
        {
            ReadAppConfig();
            GoogleDriveInit(ApplicationName, secretPath);
            queue = new BlockingCollection<DatasetInfo>(maxQueueLength);
        }

        /* Initializes Google DriveService object given application name and the credential. */
        private void GoogleDriveInit(string ApplicationName, string secretPath)
        {
            using (var stream = new FileStream(secretPath, FileMode.Open, FileAccess.Read))
            {
                string credName = Path.GetFileNameWithoutExtension(secretPath);
                string credPath = Path.Combine(
                    Path.GetFullPath(credentialDirectory),
                    credName);
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    googleAuthUser,
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
        }

        /* Returns the list of files in the given folder */
        private IList<ChildReference> FileList(string folderId)
        {
            var listRequest = service.Children.List(folderId);
            listRequest.MaxResults = maxListResults;
            IList<ChildReference> files = listRequest.Execute().Items;
            return files;
        }

        /* Uploads a file to the Google Drive. */
        private void Upload(string path, string parentId)
        {
            string fileName = Path.GetFileName(path);

            var uploadStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var insertRequest = service.Files.Insert(
                new Google.Apis.Drive.v2.Data.File
                {
                    Title = fileName,
                    Parents = new List<ParentReference>
                        { new ParentReference() { Id = parentId } }
                },
                uploadStream,
                filesMimeType);

            insertRequest.ResponseReceived += Upload_ResponseReceived;
            insertRequest.ProgressChanged += Upload_ProgressChanged;

            var task = insertRequest.UploadAsync();
            task.ContinueWith(t =>
            {
                uploadStream.Dispose();
            });
        }

        /* Receives notifications on the upload status. */
        private void Upload_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            Console.WriteLine(progress.Status + " " + progress.BytesSent);
        }

        /* Receives notification on upload completion. */
        private void Upload_ResponseReceived(Google.Apis.Drive.v2.Data.File file)
        {
            Console.WriteLine(file.Title + " was uploaded successfully");
        }

        /* Creates a new folder. */
        private void NewFolder(string name, string parentId)
        {
            var insertRequest = service.Files.Insert(
                new Google.Apis.Drive.v2.Data.File
                {
                    Title = name,
                    MimeType = folderMimeType,
                    Parents = new List<ParentReference>
                        { new ParentReference() { Id = parentId } }
                });
            insertRequest.Execute();
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

            System.IO.File.Move(info.FullPath(info.XFileName), newXFileName);
            System.IO.File.Move(info.FullPath(info.YFileName), newYFileName);
            System.IO.File.Move(info.FullPath(info.ZFileName), newZFileName);
            System.IO.File.Move(info.FullPath(info.TFileName), newTFileName);

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
        // TODO: kill the worker threads more cleanly?
        // https://msdn.microsoft.com/en-us/library/dd997371(v=vs.110).aspx
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
                    Process(info);
                }
            }
        }

        /* Uploads the data to Google Drive. */
        private void Process(DatasetInfo info)
        {
            String filePath = Archive(info);
            String parentId = CreateDirectoryTree(info);
            Upload(filePath, parentId);
        }

        /* Queues uploading a single magnetic field dataset to be uploaded. */
        public void UploadMagneticData(DatasetInfo info)
        {
            queue.Add(info);
        }
    }

}
