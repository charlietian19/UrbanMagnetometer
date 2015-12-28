using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

/* Example code taken from 

    https://developers.google.com/drive/v2/web/quickstart/dotnet 
    https://developers.google.com/api-client-library/dotnet/guide/media_upload

*/


namespace GDriveNURI
{
    public class GDrive : IUploader
    {
        private UserCredential credential;
        private DriveService service;
        private string[] Scopes = { DriveService.Scope.Drive };
        private string credentialDirectory, filesMimeType, folderMimeType,
            googleAuthUser;
        private int maxListResults;


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

        public GDrive(string ApplicationName, string secretPath)
        {
            ReadAppConfig();
            GoogleDriveInit(ApplicationName, secretPath);
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
        public void UploadAsync(string path, string parent)
        {
            string fileName = Path.GetFileName(path);

            var uploadStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var insertRequest = service.Files.Insert(
                new Google.Apis.Drive.v2.Data.File
                {
                    Title = fileName,
                    Parents = new List<ParentReference>
                        { new ParentReference() { Id = parent } }
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

        /* Synchronously uploads a file to Google Drive. */
        public void UploadSync(string path, string parent)
        {
            // TODO: implement me
        }

        /* Uploads a file given by path to Google Drive. 
        parent is in the form of /foo/bar, rather than Google IDs.
        Recursively creates the parent folder if it doesn't exist. */
        public void Upload(string path, string parent)
        {
            UploadSync(path, parent);
        }

        public bool FileExists(string path)
        {
            return false;
        }

        public bool FolderExists(string path)
        {
            return false;
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
        public void NewFolder(string name, string parent)
        {
            var insertRequest = service.Files.Insert(
                new Google.Apis.Drive.v2.Data.File
                {
                    Title = name,
                    MimeType = folderMimeType,
                    Parents = new List<ParentReference>
                        { new ParentReference() { Id = parent } }
                });
            insertRequest.Execute();
        }
    }


}
