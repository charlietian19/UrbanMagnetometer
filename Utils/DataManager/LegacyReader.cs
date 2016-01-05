﻿using System.IO;
using SystemWrapper.Configuration;
using SystemWrapper.IO;

namespace Utils.DataManager
{
    class LegacyReader : DatasetReader
    {
        private IConfigurationManagerWrap ConfigurationManager;
        private string xFileName, yFileName, zFileName, tFileName;

        /* Creates a new LegacyReader with system wrapper. */
        public LegacyReader() : base()
        {
            ConfigurationManager = new ConfigurationManagerWrap();
            ReadConfiguration();
        }

        /* Creates a new LegacyReader with provided wrappers. */
        public LegacyReader(IFileWrap file, IFileInfoFactory fileInfoFactory,
            IBinaryReaderFactory readerFactory, 
            IConfigurationManagerWrap config) :
            base(file, fileInfoFactory, readerFactory)
        {
            ConfigurationManager = config;
            ReadConfiguration();
        }

        /* Opens the dataset files. */
        public void OpenDataFiles(string path)
        {
            string xPath, yPath, zPath, tPath, dir;
            dir = Path.GetDirectoryName(path);
            xPath = Path.Combine(dir, xFileName);
            yPath = Path.Combine(dir, yFileName);
            zPath = Path.Combine(dir, zFileName);
            tPath = Path.Combine(dir, tFileName);
            OpenDataFiles(xPath, yPath, zPath, tPath);
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

    }
}
