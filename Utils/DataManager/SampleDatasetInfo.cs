using System;
using SystemWrapper.Configuration;
using Utils.Configuration;

namespace Utils.DataManager
{
    class SampleDatasetInfo : DatasetInfo, IDatasetInfo
    {
        public SampleDatasetInfo(DateTime time, IConfigurationManagerWrap config) 
            : base(time, config) { }

        /* Returns the remote path where to upload the file. */
        override public string RemotePath
        {
            get
            {
                return string.Format(@"Signal Samples\{0}", Settings.SampleName);
            }
        }
    }
}
