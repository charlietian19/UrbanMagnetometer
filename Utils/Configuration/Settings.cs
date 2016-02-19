using System;
using System.Configuration;
using Microsoft.Win32;


namespace Utils.Configuration
{
    public class Settings
    {
        private static string path = @"HKEY_CURRENT_USER\Software\Budker labs\NURI Magnetometer";
        private static string missing = "Registry key {0} is missing, please reinstall the program.";
        private static string stationName = "StationName", cache = "cache";

        /* Returns a string registry value given its name */
        private static string GetStringValue(string name)
        {
            try
            {
                var val = Registry.GetValue(path, name, "");
                return val.ToString();
            }
            catch (Exception)
            {
                throw new System.IO.IOException(string.Format(missing, name));
            }
        }

        /* Updates a string registry value given its name */
        private static void SetStringValue(string name, string value)
        {
            try
            {
                Registry.SetValue(path, name, value);
            }
            catch (Exception)
            {
                throw new System.IO.IOException(string.Format(missing, name));
            }
        }

        /* Path to the data cache folder */
        public static string CacheFolder
        {            
            get
            {
                return GetStringValue(cache);
            }

            set
            {                
                ConfigurationManager.AppSettings[cache] = value;
                SetStringValue(cache, value);
            }
        }

        /* Name of the data acquisition station. */
        public static string StationName
        {
            get
            {
                return GetStringValue(stationName);
            }

            set
            {
                ConfigurationManager.AppSettings[stationName] = value;
                SetStringValue(stationName, value);
            }
        }

        /* Maximum number of active google uploads at a time*/
        public static string MaxActiveUploads
        {
            get
            {
                return ConfigurationManager.AppSettings["MaxActiveUploads"];
            }
        }

        /* Sampling rate of the magnetometer */
        public static string SamplingRate
        {
            get
            {
                return ConfigurationManager.AppSettings["SamplingRate"];
            }
        }

        /* Units of the magnetic data */
        public static string DataUnits
        {
            get
            {
                return ConfigurationManager.AppSettings["DataUnits"];
            }
        }

        /* Name of the google drive folder where the magnetic data is stored. */
        public static string RemoteDataFolder
        {
            get
            {
                return ConfigurationManager.AppSettings["RemoteDataFolder"];
            }
        }

        /* Mime type for GDrive folders. */
        public static string FoldersMimeType
        {
            get
            {
                return ConfigurationManager.AppSettings["FoldersMimeType"];
            }
        }

        /* Path for the signal sample files */
        public static string SampleName = @"TestSample\Test";

        /* Comment for the signal sample files */
        public static string SampleComment = "";

        }
}
