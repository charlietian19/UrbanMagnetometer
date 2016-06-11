using System;
using System.Diagnostics;
using Utils.GPS;
using Utils.GPS.Time;

namespace Utils.DataReader
{
    public class GpsDatasetChunk : DatasetChunk
    {
        public GpsData Gps;
        public ITimeEstimator estimator { get; private set; }

        public GpsDatasetChunk(GpsData gps, ITimeEstimator estimator, long index, 
            double[] xdata, double[] ydata, double[] zdata) : 
            base(gps.timestamp, index,
                Convert.ToDouble(gps.ticks) / Convert.ToDouble(Stopwatch.Frequency),
                xdata, ydata, zdata)
        {
            Gps = gps;
            this.estimator = estimator;
        }
    }
}
