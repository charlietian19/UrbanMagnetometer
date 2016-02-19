using System;
using SystemWrapper.IO;
using SystemWrapper.Configuration;

namespace Utils.DataManager
{
    public class LegacySampleStorage : LegacyStorage
    {
        public LegacySampleStorage(IUploadScheduler scheduler) 
            : base(scheduler) { }
        public LegacySampleStorage(IUploadScheduler scheduler,
            PartitionCondition condition) : base(scheduler, condition) { }
        public LegacySampleStorage(IUploadScheduler scheduler, IFileWrap file,
            IDirectoryWrap dir, IBinaryWriterFactory binaryWriterFactory,
            IConfigurationManagerWrap configManager) : base(scheduler, file, 
                dir, binaryWriterFactory, configManager) { }
        public LegacySampleStorage(IUploadScheduler scheduler,
            PartitionCondition condition,
            IFileWrap file, IDirectoryWrap dir,
            IBinaryWriterFactory binaryWriterFactory,
            IConfigurationManagerWrap configManager)
            : base(scheduler, condition, file, dir, binaryWriterFactory,
                  configManager) { }

        protected override IDatasetInfo NewDatasetInfo(DateTime time, 
            IConfigurationManagerWrap configuration)
        {
            return new SampleDatasetInfo(time, configuration);
        }
    }
}
