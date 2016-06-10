using System;
using System.Diagnostics;
using Utils.GPS;

namespace Utils.DataReader
{
    public class GpsDatasetChunk : DatasetChunk
    {
        public GpsData Gps { get; private set; }

        public GpsDatasetChunk(GpsData gps, long index, double[] xdata, 
            double[] ydata, double[] zdata) : base(gps.timestamp, index,
                Convert.ToDouble(gps.ticks) / Convert.ToDouble(Stopwatch.Frequency),
                xdata, ydata, zdata)
        {
            Gps = gps;
        }
    }
}
