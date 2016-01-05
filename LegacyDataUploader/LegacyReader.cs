using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemWrapper.IO;
using SystemWrapper.Configuration;
using System.IO;

namespace LegacyDataUploader
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

    class LegacyReader
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

        /* Describes the latest retrieved chunk of data. */
        public class DatasetChunk
        {
            internal DateTime _time;
            internal long _index;
            internal double _performanceCounter;
            internal double[] _xdata, _ydata, _zdata;

            /* Returns the index of the current chunk. */
            public long Index
            {
                get { return _index; }
            }

            /* Returns the timestamp of the current chunk. */
            public DateTime Time
            {
                get { return _time; }
            }

            /* Returns the performance counter value of the current chunk. */
            public double PerformanceCounter
            {
                get { return _performanceCounter; }
            }

            /* Returns the X channel data of the current chunk. */
            public double[] XData
            {
                get { return _xdata; }
            }

            /* Returns the Y channel data of the current chunk. */
            public double[] YData
            {
                get { return _ydata; }
            }

            /* Returns the Z channel data of the current chunk. */
            public double[] ZData
            {
                get { return _zdata; }
            }
        }

        private IFileWrap File;
        private IFileInfoFactory infoFactory;
        private IBinaryReaderWrap x, y, z, t;
        private IBinaryReaderFactory readerFactory;
        private IConfigurationManagerWrap ConfigurationManager;
        private string xFileName, yFileName, zFileName, tFileName;
        private long xFileLength, tFileLength;
        private DateTime _datasetStartTime;
        public DatasetChunk Chunk = new DatasetChunk();

        /* Creates a new LegacyReader with system wrapper. */
        public LegacyReader(string path)
        {
            File = new FileWrap();
            infoFactory = new FileInfoFactory();
            readerFactory = new BinaryReaderFactory();
            ConfigurationManager = new ConfigurationManagerWrap();
            InitReader(path);
        }

        /* Creates a new LegacyReader with provided wrappers. */
        public LegacyReader(string path, IFileWrap file,
            IFileInfoFactory fileInfoFactory, 
            IBinaryReaderFactory readerFactory,
            IConfigurationManagerWrap config)
        {
            File = file;
            infoFactory = fileInfoFactory;
            this.readerFactory = readerFactory;
            ConfigurationManager = config;
            InitReader(path);
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

        /* Initializes the LegacyReader. */
        private void InitReader(string path)
        {
            ReadConfiguration();
            OpenDataFiles(path);            
        }

        /* Reads configuration settings. */
        private void ReadConfiguration()
        {
            var settings = ConfigurationManager.AppSettings;
            xFileName = settings["LegacyChannelNameX"];
            yFileName = settings["LegacyChannelNameY"];
            zFileName = settings["LegacyChannelNameZ"];
            tFileName = settings["LegacyChannelNameTime"];
        }

        /* Opens the dataset files. */
        private void OpenDataFiles(string path)
        {
            string xPath, yPath, zPath, tPath, dir;
            dir = Path.GetDirectoryName(path);
            xPath = Path.Combine(dir, xFileName);
            yPath = Path.Combine(dir, yFileName);
            zPath = Path.Combine(dir, zFileName);
            tPath = Path.Combine(dir, tFileName);
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
        void ReadStats(string xPath, string tPath)
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
        void GetNextChunk()
        {
            Chunk._index = t.ReadInt64();
            var length = t.ReadInt32();
            var timeUtc = t.ReadInt64();
            Chunk._performanceCounter = t.ReadDouble();

            Chunk._time = new DateTime(timeUtc, DateTimeKind.Utc);
            Chunk._xdata = new double[length];
            Chunk._ydata = new double[length];
            Chunk._zdata = new double[length];

            for (int i = 0; i < length; i++)
            {
                Chunk._xdata[i] = x.ReadDouble();
                Chunk._ydata[i] = y.ReadDouble();
                Chunk._zdata[i] = z.ReadDouble();
            }
        }

    }
}
