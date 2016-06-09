using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.DataManager
{
    public interface IUploadScheduler
    {
        /* Adds the provided dataset to the upload queue */
        void UploadMagneticData(IDatasetInfo info);

        /* Returns the number of active uploads */
        int ActiveUploads { get; }

        /* Retry failed uploads*/
        void RetryFailed();

        /* Schedule all datasets to upload now,
        wake up sleeping threads */
        void Flush();

        /* Called when an upload is finished */
        event UploadFinishedEventHandler FinishedEvent;

        /* Called when an upload has started */
        event UploadStartedEventHandler StartedEvent;        
    }
}
