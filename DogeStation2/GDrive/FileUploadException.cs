using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDriveNURI
{
    public class FileUploadException : IOException
    {
        public FileUploadException(string msg) : base(msg) { }
        public FileUploadException() : base() { }
    }
}
