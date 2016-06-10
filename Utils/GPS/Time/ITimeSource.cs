/*
    Describes a (hardware) source of timing data
*/

using System;

namespace Utils.GPS.Time
{
    public interface ITimeSource
    {
        /* Called when a rising PPS edge is received. */
        event Action<long> PpsReceived;

        /* Called when new timing data is received. */
        event Action<GpsData> TimestampReceived;

        /* Open the time source */
        void Open();

        /* Close the time source */
        void Close();
    }
}
