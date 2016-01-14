using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Win32;

namespace Utils.Configuration
{
    public class Settings
    {
        private static string path = @"Software\Budker labs\NURI Magnetometer";
        private static string cache = "cache";
        private static string stationName = "StationName";
        private static string missing = "Configuration settings are missing, please reinstall the program.";

        /* Returns a string registry value given its name */
        private static string GetStringValue(string name)
        {
            try
            {
                var view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                    RegistryView.Registry32);
                var key = view32.OpenSubKey(path, false);
                var val = key.GetValue(name);
                key.Close();
                return val.ToString();
            }
            catch (Exception)
            {
                throw new System.IO.IOException(missing);
            }
        }

        /* Updates a string registry value given its name */
        private static void SetStringValue(string name, string value)
        {
            try
            {
                var view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                        RegistryView.Registry32);
                var key = view32.OpenSubKey(path, true);
                key.SetValue(name, value);
                key.Close();
            }
            catch (Exception)
            {
                throw new System.IO.IOException(missing);
            }
        }

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
                return ConfigurationManager.AppSettings[stationName];
            }
        }
    }
}
