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

namespace GDriveWriter
{
    /* Example code taken from https://developers.google.com/drive/v2/web/quickstart/dotnet */
    public class GDriveWriter
    {
        private UserCredential credential;
        private DriveService service;
        private static string[] Scopes = { DriveService.Scope.Drive };
        private static string credentialDirectory = ".credentials";

        /* Initializes Google DriveService object given a credential. */
        public GDriveWriter(string ApplicationName, string secret)
        {
            try
            {
                var stream = new FileStream(secret, FileMode.Open, FileAccess.Read);
                string credPath = Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                    credentialDirectory);
                string credName = Path.GetFileNameWithoutExtension(secret);
                if (!Directory.Exists(credPath))
                {
                    Directory.CreateDirectory(credPath);
                }
                credPath = Path.Combine(credPath, credName);
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
                service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        /* Lists all the files stored in the drive */
        public IList<Google.Apis.Drive.v2.Data.File> ListFiles()
        {
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.MaxResults = 10;

            IList<Google.Apis.Drive.v2.Data.File> files = listRequest.Execute()
                .Items;
            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    Console.WriteLine("{0} ({1})", file.Title, file.Id);
                }
            }
            else
            {
                Console.WriteLine("No files found.");
            }
            return files;
        }
    }

}
