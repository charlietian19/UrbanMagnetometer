using System;

namespace Utils.GPS
{
    public struct GpsData
    {
        /* Performance counter value at the moment of receiving the data. */
        public long ticks;

        /* DateTime (created from GPRMC message or interpolated). */
        public DateTime timestamp;

        /* Indicates whether this time data is valid. */
        public bool valid;

        /* Auxilary GPS data (not currently in use) */
        public string longitude;
        public string latitude;
        public double speedKnots;
        public double angleDegrees;
        public string ew;
        public string ns;
        public string active;
    }
}
