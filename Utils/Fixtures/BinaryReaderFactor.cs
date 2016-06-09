using SystemWrapper.IO;

namespace Utils.Fixtures
{
    /* Factory for IBinaryReaderWrap objects */
    public class BinaryReaderFactory : IBinaryReaderFactory
    {
        public IBinaryReaderWrap Create(IFileStreamWrap stream)
        {
            return new BinaryReaderWrap(stream);
        }
    }
}
