using System;
using System.IO;

namespace GDriveNURI
{
    /* Stores the information about the dataset naming in a convenient way. */
    public class DatasetInfo
    {
        private string dataCacheFolder, channelNameX, channelNameY, channelNameZ, 
            channelNameTime, dataFileNameFormat, timeFileNameFormat,
            zipFileNameFormat;
        
        public String Hour { get; set; }
        public String Year { get; set; }
        public String Day { get; set; }
        public String Month { get; set; }
        public String Units { get; set; }
        public String SamplingRate { get; set; }
        public String StationName { get; set; }
        public DateTime StartDate { get; set; }

        /* Initializes settings from the configuration file. */
        private void ReadAppConfig()
        {
            var settings = System.Configuration.ConfigurationManager.AppSettings;
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
            dataCacheFolder = settings["DataCacheFolder"];
        }

        public DatasetInfo(DateTime time)
        {
            ReadAppConfig();
            Hour = time.Hour.ToString();
            Year = time.Year.ToString();
            Day = time.Day.ToString();
            Month = time.Month.ToString();
            StartDate = time;
        }

        /* Returns file name for X, Y or Z data file. */
        private String FormatFileName(String channel)
        {
            return String.Format(dataFileNameFormat, Year, Month, Day, Hour, 
                channel, Units, SamplingRate);
        }

        /* Returns file name for the X data file. */
        public String XFileName
        {
            get
            {
                return FormatFileName(channelNameX);
            }
        }

        /* Returns file name for the Y data file. */
        public String YFileName
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
        public String TFileName
        {
            get
            {
                return String.Format(timeFileNameFormat,
                    Year, Month, Day, Hour, channelNameTime);
            }
        }

        /* Returns the archive name for this dataset. */
        public String ZipFileName
        {
            get
            {
                return String.Format(timeFileNameFormat,
                    Year, Month, Day, Hour);
            }
        }

        /* Returns full path to the folder containing the data. */
        public String FolderPath
        {
            get
            {
                return dataCacheFolder;
            }
        }

        /* Returns full path to a data file. */
        public String FullPath(String file)
        {
            return Path.Combine(FolderPath, file);
        }

        /* Returns true if the data is supposed to be in the same file. */
        public bool isSameFile(DatasetInfo other)
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
