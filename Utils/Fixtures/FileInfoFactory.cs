using SystemWrapper.IO;

namespace Utils.Fixtures
{
    /* Factory for IFileInfoWrap objects */
    public class FileInfoFactory : IFileInfoFactory
    {
        public IFileInfoWrap Create(string path)
        {
            return new FileInfoWrap(path);
        }
    }
}
