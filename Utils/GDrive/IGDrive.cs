using System.Collections.Generic;
using Google.Apis.Drive.v2.Data;

namespace Utils.GDrive
{
    public interface IGDrive
    {
        string GetRootFolderId();
        File GetFileInfo(string id);
        IList<ChildReference> ChildList(File file);
        void NewFolder(string name, File parent);
        event UploadProgressDelegate ProgressEvent;
    }
}
