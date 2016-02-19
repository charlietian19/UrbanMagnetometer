using System;
using SystemWrapper.IO;
using SystemWrapper.Configuration;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;

namespace Utils.DataManager
{
    public interface IDatasetInfo
    {
        string Hour { get; set; }
        string Year { get; set; }
        string Day { get; set; }
        string Month { get; set; }
        string Units { get; set; }
        string SamplingRate { get; set; }
        string StationName { get; set; }
        DateTime StartDate { get; set; }
        string XFileName { get; }
        string YFileName { get; }
        string ZFileName { get; }
        string TFileName { get; }
        string ZipFileName { get; }
        string FolderPath { get; }
        string FullPath(string file);
        bool isSameFile(IDatasetInfo other);
        bool isSameFile(DateTime other);
        string ArchivePath { get; set; }
        string RemotePath { get; }
        string ArchiveFiles();
    }

    /* Stores the information about the dataset naming in a convenient way. */
    public class DatasetInfo : IDatasetInfo
    {
        private string dataCacheFolder, channelNameX, channelNameY, 
            channelNameZ, channelNameTime, dataFileNameFormat, 
            timeFileNameFormat, zipFileNameFormat;
        private IConfigurationManagerWrap ConfigurationManager;

        public string Hour { get; set; }
        public string Year { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Units { get; set; }
        public string SamplingRate { get; set; }
        public string StationName { get; set; }
        public DateTime StartDate { get; set; }
        public string ArchivePath { get; set; }
        protected IZipFile zip;
        protected IFileWrap File;
        protected IDirectoryWrap Directory;
        protected IPathWrap Path;

        /* Initializes settings from the configuration file. */
        private void ReadAppConfig()
        {
            var settings = ConfigurationManager.AppSettings;
            dataCacheFolder = settings["DataCacheFolder"];
            StationName = settings["StationName"];
            SamplingRate = settings["SamplingRate"];
            Units = settings["DataUnits"];
            channelNameX = settings["ChannelNameX"];
            channelNameY = settings["ChannelNameY"];
            channelNameZ = settings["ChannelNameZ"];
            channelNameTime = settings["ChannelNameTime"];
            zipFileNameFormat = settings["ZipFileNameFormat"];
            timeFileNameFormat = settings["TimeFileNameFormat"];
            dataFileNameFormat = settings["DataFileNameFormat"];
        }

        public DatasetInfo(DateTime time) : this(time,
            new ConfigurationManagerWrap(), new ZipFileWrapper(),
            new FileWrap(), new DirectoryWrap(), new PathWrap())
        { }

        public DatasetInfo(string fullPath) : this(fullPath,
            new ConfigurationManagerWrap(), new ZipFileWrapper(),
            new FileWrap(), new DirectoryWrap(), new PathWrap())
        { }

        public DatasetInfo(DateTime time, IConfigurationManagerWrap config,
            IZipFile zip, IFileWrap file, IDirectoryWrap dir, IPathWrap path)
        {
            ConfigurationManager = config;            
            Hour = time.Hour.ToString();
            Year = time.Year.ToString();
            Day = time.Day.ToString();
            Month = time.Month.ToString();
            File = file;
            Directory = dir;
            Path = path;
            StartDate = time;
            this.zip = zip;
            ReadAppConfig();
        }

        public DatasetInfo(string fullPath, IConfigurationManagerWrap config,
            IZipFile zip, IFileWrap file, IDirectoryWrap dir, IPathWrap path)
            : this(DateTime.Now, config, zip, file, dir, path)
        {
            ArchivePath = fullPath;
            string formatPattern = @"(\{[0-9]\})";
            string newFormatPattern = @"([0-9]+)";
            string fileNamePattern = Regex.Replace(zipFileNameFormat,
                formatPattern, newFormatPattern);
            string name = Path.GetFileName(fullPath);
            Debug.WriteLine(string.Format("Matching {0} against {1}",
                name, fileNamePattern));
            Regex re = new Regex(fileNamePattern);
            GroupCollection groups = re.Matches(name)[0]
                .Groups;
            var zero = new char[] { '0' };
            Year = groups[1].Value.TrimStart(zero);
            Year = (Year == "") ? "0" : Year ;
            Month = groups[2].Value.TrimStart(zero);
            Month = (Month == "") ? "0" : Month;
            Day = groups[3].Value.TrimStart(zero);
            Day = (Day == "") ? "0" : Day;
            Hour = groups[4].Value.TrimStart(zero);
            Hour = (Hour == "") ? "0" : Hour;
            StartDate = new DateTime(Convert.ToInt32(Year),
                Convert.ToInt32(Month), Convert.ToInt32(Day),
                Convert.ToInt32(Hour), 0, 0);
        }

        /* Returns file name for X, Y or Z data file. */
        virtual protected string FormatFileName(string channel)
        {
            return string.Format(dataFileNameFormat, Year, Month, Day, Hour, 
                channel, Units, SamplingRate);
        }

        /* Returns file name for the X data file. */
        virtual public string XFileName
        {
            get
            {
                return FormatFileName(channelNameX);
            }
        }

        /* Returns file name for the Y data file. */
        virtual public string YFileName
        {
            get
            {
                return FormatFileName(channelNameY);
            }
        }

        /* Returns file name for the Z data file. */
        virtual public string ZFileName
        {
            get
            {
                return FormatFileName(channelNameZ);
            }
        }

        /* Returns file name for time data file. */
        virtual public string TFileName
        {
            get
            {
                return string.Format(timeFileNameFormat,
                    Year, Month, Day, Hour, channelNameTime);
            }
        }

        /* Returns the archive name for this dataset. */
        virtual public string ZipFileName
        {
            get
            {
                return string.Format(zipFileNameFormat,
                    Year, Month, Day, Hour);
            }
        }

        /* Returns full path to the folder containing the data. */
        virtual public string FolderPath
        {
            get
            {
                return dataCacheFolder;
            }
        }

        /* Returns full path to a data file. */
        virtual public string FullPath(string file)
        {
            return Path.Combine(FolderPath, file);
        }

        /* Returns true if the data is supposed to be in the same file. */
        public bool isSameFile(IDatasetInfo other)
        {
            // If parameter is null return false.
            if (other == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (StartDate.Year == other.StartDate.Year) &&
                (StartDate.Month == other.StartDate.Month) &&
                (StartDate.Day == other.StartDate.Day) && 
                (StartDate.Hour == other.StartDate.Hour);
        }

        /* Returns true if the data is supposed to be in the same file. */
        public bool isSameFile(DateTime other)
        {
            // If parameter is null return false.
            if (other == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (StartDate.Year == other.Year) &&
                (StartDate.Month == other.Month) &&
                (StartDate.Day == other.Day) &&
                (StartDate.Hour == other.Hour);
        }

        /* Returns the remote path where to upload the file. */
        virtual public string RemotePath
        {
            get
            {
                return string.Format(@"{0}\{1}\{2}\{3}\{4}", 
                    Year, Month, Day, Hour, StationName);
            }
        }

        /* Creates a new temporary directory in the folder containing data. */
        private Mutex tmpDirMutex = new Mutex();
        private string CreateTemporaryDirectory()
        {
            string name = null;
            bool success = false;
            tmpDirMutex.WaitOne();
            while (!success)
            {
                name = Path.Combine(FolderPath, Path.GetRandomFileName());
                if (!Directory.Exists(name))
                {
                    Directory.CreateDirectory(name);
                    success = true;
                }
            }
            tmpDirMutex.ReleaseMutex();
            return name;
        }

        /* Removes the temporary directory. */
        private void DeleteTemporaryDirectory(string name)
        {
            tmpDirMutex.WaitOne();
            Directory.Delete(name, true);
            tmpDirMutex.ReleaseMutex();
        }


        /* Adds the magnetic field dataset to an archive and returns full path
        to the archive. */
        virtual protected string MoveDataToTmpDir()
        {
            string tmpDirFullPath, newXFileName, newYFileName, newZFileName,
                newTFileName;

            tmpDirFullPath = CreateTemporaryDirectory();
            newXFileName = Path.Combine(tmpDirFullPath, XFileName);
            newYFileName = Path.Combine(tmpDirFullPath, YFileName);
            newZFileName = Path.Combine(tmpDirFullPath, ZFileName);
            newTFileName = Path.Combine(tmpDirFullPath, TFileName);

            File.Move(FullPath(XFileName), newXFileName);
            File.Move(FullPath(YFileName), newYFileName);
            File.Move(FullPath(ZFileName), newZFileName);
            File.Move(FullPath(TFileName), newTFileName);

            return tmpDirFullPath;
        }

        /* Adds the files from the dataset into a zip archive. */
        public string ArchiveFiles()
        {
            string tmpDirFullPath = "";
            string path = Path.Combine(FolderPath, ZipFileName);
            ArchivePath = path;
            tmpDirFullPath = MoveDataToTmpDir();
            zip.CreateFromDirectory(tmpDirFullPath, path);
            DeleteTemporaryDirectory(tmpDirFullPath);
            return path;
        }
    }
}
