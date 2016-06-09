using SystemWrapper.IO;

namespace Utils.Fixtures
{
    /* Interface for IFileInfoWrap object factory */
    public interface IFileInfoFactory
    {
        IFileInfoWrap Create(string path);
    }
}
