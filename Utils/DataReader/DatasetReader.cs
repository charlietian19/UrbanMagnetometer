using System;
using SystemWrapper.IO;
using System.IO;

namespace Utils.DataReader
{
    /* Interface for IBinaryReaderWrap object factory */
    public interface IBinaryReaderFactory
    {
        IBinaryReaderWrap Create(IFileStreamWrap stream);
    }

    /* Interface for IFileInfoWrap object factory */
    public interface IFileInfoFactory
    {
        IFileInfoWrap Create(string path);
    }

    public class DatasetReader
    {
        /* Factory for IBinaryReaderWrap objects */
        public class BinaryReaderFactory : IBinaryReaderFactory
        {
            public IBinaryReaderWrap Create(IFileStreamWrap stream)
            {
                return new BinaryReaderWrap(stream);
            }
        }

        /* Factory for IFileInfoWrap objects */
        public class FileInfoFactory : IFileInfoFactory
        {
            public IFileInfoWrap Create(string path)
            {
                return new FileInfoWrap(path);
            }
        }

        private IFileWrap File;
        private IFileInfoFactory infoFactory;
        private IBinaryReaderWrap x, y, z, t;
        private IBinaryReaderFactory readerFactory;
        
        private long xFileLength, tFileLength;
        private DateTime _datasetStartTime;
        public DatasetChunk Chunk;

        /* Creates a new DatasetReader with system wrapper. */
        public DatasetReader()
        {
            File = new FileWrap();
            infoFactory = new FileInfoFactory();
            readerFactory = new BinaryReaderFactory();
        }

        /* Creates a new DatasetReader with provided wrappers. */
        public DatasetReader(IFileWrap file,
            IFileInfoFactory fileInfoFactory, 
            IBinaryReaderFactory readerFactory)
        {
            File = file;
            infoFactory = fileInfoFactory;
            this.readerFactory = readerFactory;
        }

        /* Returns the approximate size of the raw data in Bytes. */
        public long Size
        {
            get { return xFileLength * 3 + tFileLength; }
        }

        /* Returns DateTime object when the data was taken. */
        public DateTime DatasetStartTime
        {
            get { return _datasetStartTime; }
        }

        /* Returns the number of chunks in the dataset. */
        public long Chunks
        {            
            get
            {
                // int64 + int32 + int64 + double
                return tFileLength / 28;
            }
        }

        /* Returns the number of points in the dataset. */
        public long Points
        {
            get { return xFileLength / 8; }
        }


        /* Opens the dataset files. */
        public void OpenDataFiles(string xPath, string yPath, string zPath,
            string tPath)
        {            
            ReadStats(xPath, tPath);
            x = readerFactory.Create(File.Open(xPath, FileMode.Open,
                FileAccess.Read));
            y = readerFactory.Create(File.Open(yPath, FileMode.Open,
                FileAccess.Read));
            z = readerFactory.Create(File.Open(zPath, FileMode.Open,
                FileAccess.Read));
            t = readerFactory.Create(File.Open(tPath, FileMode.Open,
                FileAccess.Read));
        }

        /* Fills out the dataset stats */
        private void ReadStats(string xPath, string tPath)
        {
            xFileLength = infoFactory.Create(xPath).Length;
            tFileLength = infoFactory.Create(tPath).Length;

            using (var file = readerFactory.Create(File.Open(tPath, FileMode.Open,
                FileAccess.Read)))
            {
                var index = file.ReadInt64();
                var length = file.ReadInt32();
                var timeUtc = file.ReadInt64();
                var counter = file.ReadDouble();
                _datasetStartTime = new DateTime(timeUtc, DateTimeKind.Utc);
            }
        }

        /* Reads the next chunk from the dataset. */
        public DatasetChunk GetNextChunk()
        {
            var index = t.ReadInt64();
            var length = t.ReadInt32();
            var timeUtc = t.ReadInt64();
            var performanceCounter = t.ReadDouble();

            var time = new DateTime(timeUtc, DateTimeKind.Utc);
            var xdata = new double[length];
            var ydata = new double[length];
            var zdata = new double[length];

            for (int i = 0; i < length; i++)
            {
                xdata[i] = x.ReadDouble();
                ydata[i] = y.ReadDouble();
                zdata[i] = z.ReadDouble();
            }

            return new DatasetChunk(time, index, performanceCounter, xdata,
                ydata, zdata);
        }

        /* Returns true if there are more chunks and false otherwise. */
        public bool HasNextChunk()
        {
            return t.BaseStream.Position < t.BaseStream.Length;
        }

        /* Closes the dataset files. */
        public void Close()
        {
            t.Close();
            x.Close();
            y.Close();
            z.Close();
        }
    }
}
