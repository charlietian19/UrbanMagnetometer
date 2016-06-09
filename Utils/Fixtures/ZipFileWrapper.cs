using System.IO.Compression;

namespace Utils.Fixtures
{
    /* ZipFile wrapper for testing */
    public class ZipFileWrapper : IZipFile
    {
        public void CreateFromDirectory(string src, string dst)
        {
            ZipFile.CreateFromDirectory(src, dst);
        }
    }
}
