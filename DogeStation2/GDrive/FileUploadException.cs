using System.IO;

namespace GDriveNURI
{
    public class FileUploadException : IOException
    {
        public FileUploadException(string msg) : base(msg) { }
        public FileUploadException() : base() { }
    }
}
