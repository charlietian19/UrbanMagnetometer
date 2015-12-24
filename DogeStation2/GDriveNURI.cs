using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/* Example code taken from 

    https://developers.google.com/drive/v2/web/quickstart/dotnet 
    https://developers.google.com/api-client-library/dotnet/guide/media_upload

*/

namespace GDriveNURI
{
    public class Writer
    {
        private UserCredential credential;
        private DriveService service;
        private static string[] Scopes = { DriveService.Scope.Drive };
        private static string credentialDirectory, dataCacheFolder, 
            filesMimeType, folderMimeType, googleAuthUser;
        private static int maxListResults;

        /* Initializes settings from the configuration file. */
        private void ReadAppConfig()
        {
            var settings = System.Configuration.ConfigurationManager.AppSettings;
            credentialDirectory = settings["CredentialDirectory"];
            dataCacheFolder = settings["DataCacheFolder"];
            filesMimeType = settings["FilesMimeType"];
            folderMimeType = settings["FoldersMimeType"];
            googleAuthUser = settings["GoogleAuthUser"];
            maxListResults = Convert.ToInt32(settings["MaxListResults"]);
        }

        /* Initializes Google DriveService object given application name and the credential. */
        public Writer(string ApplicationName, string secretPath)
        {
            ReadAppConfig();
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

        /* Uploads a file to Google drive. */
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

        /* Stores the data from the sensor. */
        public void Write(double[] dataX, double[] dataY, double[] dataZ, double systemSeconds, DateTime time)
        {

        }
    }

}
