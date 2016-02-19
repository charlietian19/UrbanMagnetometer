using System;
using System.IO;
using SystemWrapper.IO;
using SystemWrapper.Configuration;

// TODO: proper exception handling
// Throws IOException, FileNotFound, etc...
// Handle DirectoryNotFound?

namespace Utils.DataManager
{
    public interface ILegacyStorage
    {
        void Store(double[] dataX, double[] dataY, double[] dataZ, double systemSeconds, 
            DateTime time);
        void Close();
        void Discard();
        bool UploadOnClose { get; set; }
    }

    /* Interface for IBinaryWriterWrap object factory */
    public interface IBinaryWriterFactory
    {
        IBinaryWriterWrap Create(IFileStreamWrap stream);
    }

    // TODO: scan the data cache folder for files that haven't been uploaded
    // and upload them as well
    public class LegacyStorage : ILegacyStorage
    {
        /* Factory for IBinaryWriterWrap objects */
        public class BinaryWriterFactory : IBinaryWriterFactory
        {
            public IBinaryWriterWrap Create(IFileStreamWrap stream)
            {
                return new BinaryWriterWrap(stream);
            }
        }

        public delegate bool PartitionCondition(DateTime start, DateTime now);
        private IUploadScheduler scheduler;
        private IBinaryWriterWrap x, y, z, t;
        private IBinaryWriterFactory _BinaryWriterFactory;
        private IFileWrap IFile;
        private IConfigurationManagerWrap ConfigurationManager;
        private IDirectoryWrap Directory;

        private string dataCacheFolder;
        private IDatasetInfo info = null;

        private bool isWriting = false;
        private long index;
        PartitionCondition partitionCondition;
        PartitionCondition defaultCondition = 
            (start, now) => start.Hour != now.Hour;
        private bool uploadOnClose = true;
        public bool UploadOnClose
        {
            get { return uploadOnClose; }
            set { uploadOnClose = value; }
        }

        public bool HasCachedData
        {
            get { return isWriting; }
        }

        /* Constructs the data writer given a Google Drive connection. */
        public LegacyStorage(IUploadScheduler scheduler)
        {
            this.scheduler = scheduler;
            partitionCondition = defaultCondition;
            UseDefaultWrappers();
        }

        /* Constructs the data writer given a Google Drive connection
        and a condition to partition the datastream into files. */
        public LegacyStorage(IUploadScheduler scheduler,
            PartitionCondition condition)
        {
            this.scheduler = scheduler;
            partitionCondition = condition;
            UseDefaultWrappers();
        }

        /* Constructs the data writer given a Google Drive connection 
        and the wrappers. */
        public LegacyStorage(IUploadScheduler scheduler, IFileWrap file,
            IDirectoryWrap dir, IBinaryWriterFactory binaryWriterFactory,
            IConfigurationManagerWrap configManager)
        {
            this.scheduler = scheduler;
            partitionCondition = defaultCondition;
            UseCustomWrappers(file, dir, binaryWriterFactory, configManager);
        }

        /* Constructs the data writer given a Google Drive connection, 
        partition condition and the wrappers. */
        public LegacyStorage(IUploadScheduler scheduler, 
            PartitionCondition condition,
            IFileWrap file, IDirectoryWrap dir, 
            IBinaryWriterFactory binaryWriterFactory,
            IConfigurationManagerWrap configManager)
        {
            this.scheduler = scheduler;
            partitionCondition = condition;
            UseCustomWrappers(file, dir, binaryWriterFactory, configManager);
        }

        /* Constructs the object with default filesystem wrappers. */
        public void UseDefaultWrappers()
        {
            IFile = new FileWrap();
            _BinaryWriterFactory = new BinaryWriterFactory();
            ConfigurationManager = new ConfigurationManagerWrap();
            Directory = new DirectoryWrap();
            InitCacheFolder();
        }

        /* Constructs the object with custom filesystem wrappers for testing. */
        private void UseCustomWrappers(IFileWrap file, IDirectoryWrap dir, 
            IBinaryWriterFactory binaryWriterFactory,
            IConfigurationManagerWrap configManager)
        {
            IFile = file;
            _BinaryWriterFactory = binaryWriterFactory;
            ConfigurationManager = configManager;
            Directory = dir;
            InitCacheFolder();
        }

        /* Stores the data from the sensor. */
        public void Store(double[] dataX, double[] dataY, double[] dataZ,
            double precisionCounter, DateTime time)
        {
            if (!isWriting)
            {
                CreateFiles(time);
            }

            if (partitionCondition(info.StartDate, time))
            {
                CloseAll(time);
                CreateFiles(time);
            }

            Append(dataX, dataY, dataZ, precisionCounter, time);
        }

        /* Initializes the cache. */
        private void InitCacheFolder()
        {
            var settings = ConfigurationManager.AppSettings;
            dataCacheFolder = settings["DataCacheFolder"];
            if (!Directory.Exists(dataCacheFolder))
            {
                Directory.CreateDirectory(dataCacheFolder);
            }
        }

        /* Appends the data into existing streams. */
        private void Append(double[] dataX, double[] dataY, double[] dataZ,
            double precisionCounter, DateTime time)
        {
            int length = dataX.Length;
            WriteArray(x, dataX);
            WriteArray(y, dataY);
            WriteArray(z, dataZ);
            WriteTime(t, precisionCounter, time, index, length);
            index += length;
        }

        /* Writes an array of doubles into a binary file. */
        private static void WriteArray(IBinaryWriterWrap writer, double[] data)
        {
            foreach (double value in data)
            {
                writer.Write(value);
            }
        }

        /* Writes time data into a binary file. */
        private static void WriteTime(IBinaryWriterWrap writer, 
            double precisionCounter, DateTime time, long index, int length)
        {
            writer.Write(index);
            writer.Write(length);
            writer.Write(time.ToBinary());
            writer.Write(precisionCounter);
        }

        /* Creates a new dataset. */
        virtual protected IDatasetInfo NewDatasetInfo(DateTime time, 
            IConfigurationManagerWrap configuration)
        {
            return new DatasetInfo(time, ConfigurationManager,
                new ZipFileWrapper(), new FileWrap(), new DirectoryWrap(),
                new PathWrap());
        }

        /* Creates the data files in cache folder. */
        private void CreateFiles(DateTime time)
        {
            info = NewDatasetInfo(time, ConfigurationManager);
            x = _BinaryWriterFactory.Create(IFile.Open(info.FullPath(info.XFileName),
                FileMode.Append, FileAccess.Write));
            y = _BinaryWriterFactory.Create(IFile.Open(info.FullPath(info.YFileName),
                FileMode.Append, FileAccess.Write));
            z = _BinaryWriterFactory.Create(IFile.Open(info.FullPath(info.ZFileName),
                FileMode.Append, FileAccess.Write));
            t = _BinaryWriterFactory.Create(IFile.Open(info.FullPath(info.TFileName),
                FileMode.Append, FileAccess.Write));
            index = 0;
            isWriting = true;
        }

        /* Closes the data files and sends them to the Google Drive.
        time indicates when the latest chunk of data was received. */
        private void CloseAll(DateTime time)
        {
            if (!isWriting)
            {
                return;
            }

            x.Close();
            y.Close();
            z.Close();
            t.Close();
            isWriting = false;

            if (uploadOnClose)
            {
                scheduler.UploadMagneticData(info);
            }
        }
        
        /* Closes the data files (eg. when the application exits).
        Store() can be subsequently called, which may result in appending 
        the data files, or creating new files. */
        public void Close()
        {
            CloseAll(DateTime.Now);
        }

        /* Discard (truncate) the current data files. */
        public void Discard()
        {
            if (isWriting)
            {
                x.BaseStream.SetLength(0);
                y.BaseStream.SetLength(0);
                z.BaseStream.SetLength(0);
                t.BaseStream.SetLength(0);
                index = 0;
            }
            else
            {
                IFile.Delete(info.FullPath(info.XFileName));
                IFile.Delete(info.FullPath(info.YFileName));
                IFile.Delete(info.FullPath(info.ZFileName));
                IFile.Delete(info.FullPath(info.TFileName));
            }
        }
    }

}
