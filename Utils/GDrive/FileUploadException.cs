using System.IO;

namespace Utils.GDrive
{
    public class FileUploadException : IOException
    {
        public FileUploadException(string msg) : base(msg) { }
        public FileUploadException() : base() { }
    }
}
