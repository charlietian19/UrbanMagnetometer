using SystemWrapper.IO;

namespace Utils.Fixtures
{
    /* Interface for IBinaryReaderWrap object factory */
    public interface IBinaryReaderFactory
    {
        IBinaryReaderWrap Create(IFileStreamWrap stream);
    }
}
