using System.Diagnostics;

namespace Utils.GPS.SerialGPS
{
    /* Wraps System.Stopwatch class for testing */
    class StopwatchWrapper : IStopwatch
    {
        public long Frequency
        {
            get { return Stopwatch.Frequency; }
        }

        public long GetTimestamp()
        {
            return Stopwatch.GetTimestamp();
        }
    }
}
