using System;

namespace Utils.DataReader
{
    /* Describes the latest retrieved chunk of data. */
    public class DatasetChunk
    {
        private DateTime _time;
        private long _index = 0;
        private double _performanceCounter;
        private double[] _xdata, _ydata, _zdata;

        public DatasetChunk(DateTime time, long index, double performanceCounter,
            double[] xdata, double[] ydata, double[] zdata)
        {
            _time = time;
            _index = index;
            _performanceCounter = performanceCounter;
            _xdata = xdata;
            _ydata = ydata;
            _zdata = zdata;
        }

        /* Returns the index of the current chunk. */
        public long Index
        {
            get { return _index; }
        }

        /* Returns the timestamp of the current chunk. */
        public DateTime Time
        {
            get { return _time; }
        }

        /* Returns the performance counter value of the current chunk. */
        public double PerformanceCounter
        {
            get { return _performanceCounter; }
        }

        /* Returns the X channel data of the current chunk. */
        public double[] XData
        {
            get { return _xdata; }
        }

        /* Returns the Y channel data of the current chunk. */
        public double[] YData
        {
            get { return _ydata; }
        }

        /* Returns the Z channel data of the current chunk. */
        public double[] ZData
        {
            get { return _zdata; }
        }
    }
}
