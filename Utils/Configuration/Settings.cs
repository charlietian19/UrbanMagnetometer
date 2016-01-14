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

        public static string MaxActiveUploads
        {
            get
            {
                return ConfigurationManager.AppSettings["MaxActiveUploads"];
            }
        }

        public static string SamplingRate
        {
            get
            {
                return ConfigurationManager.AppSettings["SamplingRate"];
            }
        }

        public static string DataUnits
        {
            get
            {
                return ConfigurationManager.AppSettings["DataUnits"];
            }
        }
    }
}
