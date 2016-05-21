using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.GPS
{
    public interface ITimeSource
    {
        /* Returns an interpolated timestamp based on previous data. */
        GpsData GetTimeStamp(long counter);

        /* Add a GPS timing information as a data point. */
        void PutTimestamp(GpsData data);
    }
}
