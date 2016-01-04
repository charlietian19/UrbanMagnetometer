using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;

/* Example code taken from 

    https://developers.google.com/drive/v2/web/quickstart/dotnet 
    https://developers.google.com/api-client-library/dotnet/guide/media_upload

*/

namespace GDriveNURI
{
    /* Provide the interface to enable service mocking. */
    public interface IService
    {
        AboutResource About { get; }
        FilesResource Files { get; }
        ChildrenResource Children { get; }
    }

    /* The adapter class makes DriveService conform to the testing interface */
    public class DriveServiceAdapter : IService
    {
        private DriveService service;

        public DriveServiceAdapter(DriveService service)
        {
            this.service = service;
        }

        public AboutResource About { get { return service.About; } }
        public FilesResource Files { get { return service.Files; } }
        public ChildrenResource Children { get { return service.Children; } }
    }

    public class GDrive : IUploader, IGDrive
    {
        //private DriveService service;
        private IService service;
        private IGDrivePathHelper pathHelper;
        private static string[] Scopes = { DriveService.Scope.Drive };
        private static string credentialDirectory, ApplicationName,
            googleAuthUser;
        public static string filesMimeType, folderMimeType;
        private static int maxListResults;

        /* Initializes settings from the configuration file. */
        private static void ReadAppConfig()
        {
            var settings = System.Configuration.ConfigurationManager
                .AppSettings;
            credentialDirectory = settings["CredentialDirectory"];
            filesMimeType = settings["FilesMimeType"];
            folderMimeType = settings["FoldersMimeType"];
            googleAuthUser = settings["GoogleAuthUser"];
            maxListResults = Convert.ToInt32(settings["MaxListResults"]);
            ApplicationName = settings["GoogleApplicationName"];
        }

        public GDrive(IService service)
        {
            this.service = service;
            pathHelper = new GDrivePathHelper(this);
        }

        public GDrive(string secretPath) : this(GoogleDriveInit(secretPath))
        {
        }

        /* Initializes Google DriveService object given the credential. */
        public static DriveServiceAdapter GoogleDriveInit(string secretPath)
        {
            ReadAppConfig();
            using (var stream = new FileStream(secretPath, FileMode.Open,
                FileAccess.Read))
            {
                string credName = Path.GetFileNameWithoutExtension(secretPath);
                string credPath = Path.Combine(
                    Path.GetFullPath(credentialDirectory),
                    credName);
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    googleAuthUser,
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
                return new DriveServiceAdapter(service);
            }
        }

        /* Returns the root folder ID. */
        public string GetRootFolderId()
        {
            About about = service.About.Get().Execute();
            return about.RootFolderId;
        }

        /* Returns a file metadata given file ID. */
        public Google.Apis.Drive.v2.Data.File GetFileInfo(string id)
        {
            return service.Files.Get(id).Execute();
        }

        /* Returns the list of files in the given folder */
        // TODO: make a batch request that returns a list of Files instead
        public IList<ChildReference> ChildList(
            Google.Apis.Drive.v2.Data.File file)
        {
            var listRequest = service.Children.List(file.Id);
            listRequest.MaxResults = maxListResults;
            IList<ChildReference> files = listRequest.Execute().Items;
            return files;
        }

        /* Synchronously uploads a file given by path to Google Drive. 
        parent is in the form of \foo\bar\file.bin, rather than Google IDs.
        Recursively creates the parent folder if it doesn't exist. */
        public async void Upload(string path, string remotePath)
        {
            string fileName = Path.GetFileName(path);
            string parent = Path.GetDirectoryName(path);

            using (var uploadStream = new FileStream(path, FileMode.Open,
                FileAccess.Read))
            {
                string parentId = pathHelper.CreateDirectoryTree(parent).Id;
                var insertRequest = service.Files.Insert(
                    new Google.Apis.Drive.v2.Data.File
                    {
                        Title = fileName,
                        Parents = new List<ParentReference>
                            { new ParentReference() { Id = parentId } }
                    },
                    uploadStream,
                    filesMimeType);

                Google.Apis.Upload.IUploadProgress progress = 
                    await insertRequest.UploadAsync();
                if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    string msg = string.Format("Can't upload {0} into {1}", 
                        path, remotePath);
                    throw new IOException(msg);
                }
            }
        }

        /* Creates a new folder. */
        public void NewFolder(string name, 
            Google.Apis.Drive.v2.Data.File parent)
        {
            var insertRequest = service.Files.Insert(
                new Google.Apis.Drive.v2.Data.File
                {
                    Title = name,
                    MimeType = folderMimeType,
                    Parents = new List<ParentReference>
                        { new ParentReference() { Id = parent.Id } }
                });
            insertRequest.Execute();
        }
    }


}
