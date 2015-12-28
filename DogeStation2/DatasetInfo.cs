using System;
using System.IO;

namespace GDriveNURI
{
    /* Stores the information about the dataset naming in a convenient way. */
    public class DatasetInfo
    {
        private string dataCacheFolder, channelNameX, channelNameY, channelNameZ, 
            channelNameTime, dataFileNameFormat, timeFileNameFormat;
        
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

        /* Returns full path to a data file. */
        public String FullPath(String file)
        {
            return Path.Combine(dataCacheFolder, file);
        }

    }
}
