using System;
using System.IO;
using SystemWrapper.IO;
using SystemWrapper.Configuration;
using Utils.GPS;
using Utils.Fixtures;

// TODO: proper exception handling
// Make the written data structure flexible
// Throws IOException, FileNotFound, etc...
// Handle DirectoryNotFound?

namespace Utils.DataManager
{
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
            GpsData gps)
        {
            if (!isWriting)
            {
                CreateFiles(gps.timestamp);
            }

            if (partitionCondition(info.StartDate, gps.timestamp))
            {
                CloseAll(gps.timestamp);
                CreateFiles(gps.timestamp);
            }

            Append(dataX, dataY, dataZ, gps);
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
            GpsData gps)
        {
            int length = dataX.Length;
            WriteArray(x, dataX);
            WriteArray(y, dataY);
            WriteArray(z, dataZ);
            WriteTime(t, gps, index, length);
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
            GpsData gps, long index, int length)
        {
            var timestamp = Helpers.DateTimeToUnixTimestamp(gps.timestamp);
            writer.Write(index);
            writer.Write(length);
            writer.Write(Convert.ToByte(gps.valid));
            writer.Write(gps.ticks);
            writer.Write(timestamp);
            writer.Write(gps.latitude);
            writer.Write(Convert.ToByte(gps.ew[0]));
            writer.Write(gps.longitude);
            writer.Write(Convert.ToByte(gps.ns[0]));
            writer.Write(gps.speedKnots);
            writer.Write(gps.angleDegrees);
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
            CloseDataFiles();

            if (uploadOnClose)
            {
                Upload();
            }
        }

        /* Closes the data files. */
        private void CloseDataFiles()
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
        }
        
        /* Closes the data files (eg. when the application exits).
        Store() can be subsequently called, which may result in appending 
        the data files, or creating new files. */
        public void Close()
        {
            CloseAll(DateTime.Now);
        }

        /* Hands the files over to the Upload Scheduler*/
        public void Upload()
        {
            CloseDataFiles();
            scheduler.UploadMagneticData(info);
        }

        /* Discard (truncate) the current data files. */
        public void Discard()
        {
            CloseDataFiles();
            IFile.Delete(info.FullPath(info.XFileName));
            IFile.Delete(info.FullPath(info.YFileName));
            IFile.Delete(info.FullPath(info.ZFileName));
            IFile.Delete(info.FullPath(info.TFileName));
        }
    }

}
