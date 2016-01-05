using System;
using System.IO;
using SystemWrapper.Configuration;

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
    }

    /* Stores the information about the dataset naming in a convenient way. */
    public class DatasetInfo : IDatasetInfo
    {
        private string dataCacheFolder, channelNameX, channelNameY, channelNameZ, 
            channelNameTime, dataFileNameFormat, timeFileNameFormat,
            zipFileNameFormat;
        private IConfigurationManagerWrap ConfigurationManager;

        public string Hour { get; set; }
        public string Year { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Units { get; set; }
        public string SamplingRate { get; set; }
        public string StationName { get; set; }
        public DateTime StartDate { get; set; }

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

        public DatasetInfo(DateTime time, IConfigurationManagerWrap config)
        {
            ConfigurationManager = config;
            ReadAppConfig();
            Hour = time.Hour.ToString();
            Year = time.Year.ToString();
            Day = time.Day.ToString();
            Month = time.Month.ToString();
            StartDate = time;
        }

        /* Returns file name for X, Y or Z data file. */
        private string FormatFileName(String channel)
        {
            return String.Format(dataFileNameFormat, Year, Month, Day, Hour, 
                channel, Units, SamplingRate);
        }

        /* Returns file name for the X data file. */
        public string XFileName
        {
            get
            {
                return FormatFileName(channelNameX);
            }
        }

        /* Returns file name for the Y data file. */
        public string YFileName
        {
            get
            {
                return FormatFileName(channelNameY);
            }
        }

        /* Returns file name for the Z data file. */
        public String ZFileName
        {
            get
            {
                return FormatFileName(channelNameZ);
            }
        }

        /* Returns file name for time data file. */
        public string TFileName
        {
            get
            {
                return String.Format(timeFileNameFormat,
                    Year, Month, Day, Hour, channelNameTime);
            }
        }

        /* Returns the archive name for this dataset. */
        public string ZipFileName
        {
            get
            {
                return String.Format(zipFileNameFormat,
                    Year, Month, Day, Hour);
            }
        }

        /* Returns full path to the folder containing the data. */
        public string FolderPath
        {
            get
            {
                return dataCacheFolder;
            }
        }

        /* Returns full path to a data file. */
        public string FullPath(string file)
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
    }
}
