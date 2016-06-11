using Accord.Statistics;
using Accord.Statistics.Models.Regression.Linear;
using System;
using Utils.DataReader;
using Utils.Fixtures;

namespace Utils.GPS.Time
{
    /* Interpolates jittered time on magnetometer data chunks */
    public class LagSpikeFilter
    {
        protected IStorage<GpsDatasetChunk> data;
        private IStopwatch stopwatch;
        public event Action<GpsDatasetChunk> OnPop;

        private SimpleLinearRegression regression = null;
        private long t0;

        /* Fix the data points that are further than this from the 
        expected value */
        public double ToleranceLow = 200 * 1e-6; // 200 microseconds

        /* Leave alone the data points that are further than this from
        the expected value because maybe the fit has blown up */
        public double ToleranceHigh = 50 * 1e-3; // 20 milliseconds

        /* Create the filter with the default storage given size */
        public LagSpikeFilter(int size)
        {
            if (size < 5)
            {
                throw new ArgumentOutOfRangeException(
                    "Storage size must be at least 5");
            }
            data = new FifoStorage<GpsDatasetChunk>(size);
            stopwatch = new StopwatchWrapper();
            Initialize();
        }

        /* Create the filter given a storage */
        public LagSpikeFilter(IStorage<GpsDatasetChunk> storage,
            IStopwatch stopwatch)
        {
            data = storage;
            this.stopwatch = stopwatch;
            Initialize();
        }

        /* Initializes the filter */
        protected virtual void Initialize()
        {
            data.AfterPop += FixJitter;
        }

        /* Adds the data into the filter */
        public void InputData(GpsDatasetChunk chunk)
        {
            lock (data)
            {
                data.Add(chunk);
            }            
        }

        /* Clear the data points */
        public void Clear()
        {
            lock (data)
            {
                data.Clear();
                regression = null;
            }
        }

        /* Flushes the data out of the filter */
        public void Flush()
        {
            lock (data)
            {
                InterpolateAndPopAll();
                data.Clear();
            }
        }

        /* Interpolate and pop all points using the current model */
        private void InterpolateAndPopAll()
        {
            if (regression == null)
            {
                return;
            }

            for (int i = 0; i < data.Count; i++)
            {
                var chunk = data[i];
                var expected = Convert.ToInt64(regression.Compute(i)) + t0;
                var delta = ToSeconds(Math.Abs(chunk.Gps.ticks - expected));

                if ((delta > ToleranceLow) && (delta < ToleranceHigh))
                {
                    chunk.Gps = CorrectedGps(chunk, expected);
                }

                if (OnPop != null)
                {
                    OnPop(chunk);
                }
            }            
        }

        /* Fix the timing in this chunk of magnetometer data */
        protected virtual void FixJitter(GpsDatasetChunk chunk)
        {
            var ticks = GetTicksArray();
            var differences = Differences(ticks);
            var period = Tools.Median(differences);
            try
            {
                var expected = ExpectedTicks(ticks, differences, period);
                var delta = ToSeconds(Math.Abs(chunk.Gps.ticks - expected));
                if ((delta > ToleranceLow) && (delta < ToleranceHigh))
                {
                    chunk.Gps = CorrectedGps(chunk, expected);
                }
            }
            catch { }

            if (OnPop != null)
            {
                OnPop(chunk);
            }
        }

        /* Returns corrected GpsData given expected ticks and data chunk */
        private static GpsData CorrectedGps(GpsDatasetChunk chunk, 
            long expected)
        {
            var gps = chunk.Gps;
            var expectedGps = chunk.estimator.GetTimeStamp(expected);
            gps.ticks = expected;
            gps.timestamp = expectedGps.timestamp;
            gps.valid = expectedGps.valid;
            return gps;
        }

        /* Returns the array of the ticks from the GPS arrival data */
        protected long[] GetTicksArray()
        {
            long[] result;   
            lock (data)
            {
                result = new long[data.Count];
                for (int i = 0; i < data.Count; i++)
                {
                    result[i] = data[i].Gps.ticks;
                }
            }
            return result;
        }

        /* Returs seconds given ticks */
        private double ToSeconds(long ticks)
        {
            return Convert.ToDouble(ticks) / Convert.ToDouble(stopwatch.Frequency);
        }

        /* Returns the vector of the sample arrival time differences 
        in seconds */
        protected double[] Differences(long[] ticks)
        {
            var result = new double[ticks.Length - 1];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ToSeconds(ticks[i + 1] - ticks[i]);
            }
            return result;
        }

        /* Returns a linear extrapolation of the expected arrival time of 
        the point just pushed out */
        protected long ExpectedTicks(long[] ticks, double[] dt, double median)
        {
            var validPoints = PointsWithinToleranceCount(dt, median);
            if (validPoints < 2)
            {
                throw new InvalidOperationException(
                    "Too few data points to extrapolate");
            }
            var x = new double[validPoints];
            var y = new double[validPoints];

            int j = 0;
            t0 = ticks[0];
            for (int i = 0; i < dt.Length; i++)
            {
                if (Math.Abs(dt[i] - median) < ToleranceLow)
                {
                    x[j] = Convert.ToDouble(i);
                    y[j] = Convert.ToDouble(ticks[i] - t0);
                    j++;
                }
            }

            if (j != validPoints)
            {
                throw new InvalidOperationException(
                    "Number of valid data points is inconsistent");
            }

            regression = new SimpleLinearRegression();
            regression.Regress(x, y);
            var result = Convert.ToInt64(regression.Compute(-1)) + t0;
            return result;
        }

        /* Returns the number of data points with the differences within the 
        tolerance */
        protected int PointsWithinToleranceCount(double[] differences, 
            double median)
        {
            int count = 0;
            foreach (var dt in differences)
            {
                if (Math.Abs(dt - median) < ToleranceLow)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
