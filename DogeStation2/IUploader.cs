using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDriveNURI
{
    public interface IUploader
    {
        void Upload(string fileNameToUpload, string parent);
        void NewFolder(string name, string parent);
        bool FolderExists(string fullPath);
        bool FileExists(string fullPath);
    }
}
