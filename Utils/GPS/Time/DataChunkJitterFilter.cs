using Accord.Statistics;
using Accord.Statistics.Models.Regression.Linear;
using System;
using Utils.DataReader;
using Utils.Fixtures;

namespace Utils.GPS.Time
{
    /* Interpolates jittered time on magnetometer data chunks */
    public class DataChunkJitterFilter
    {
        protected IStorage<GpsDatasetChunk> data;
        private IStopwatch stopwatch;
        public event Action<GpsDatasetChunk> OnPop;
        public double Tolerance = 200 * 1e-6;

        /* Create the filter with the default storage given size */
        public DataChunkJitterFilter(int size)
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
        public DataChunkJitterFilter(IStorage<GpsDatasetChunk> storage,
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
        public void Push(GpsDatasetChunk chunk)
        {
            lock (data)
            {
                data.Add(chunk);
            }            
        }

        /* Flushes the data out of the filter */
        public void Flush()
        {
            lock (data)
            {
                data.Flush();
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
                if (ToSeconds(Math.Abs(chunk.Gps.ticks - expected)) > Tolerance)
                {
                    var gps = chunk.Gps;                    
                    var expectedGps = chunk.estimator.GetTimeStamp(expected);
                    gps.ticks = expected;
                    gps.timestamp = expectedGps.timestamp;
                    chunk.Gps = gps;
                }
            }
            catch { }

            if (OnPop != null)
            {
                OnPop(chunk);
            }
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
            for (int i = 0; i < dt.Length; i++)
            {
                if (Math.Abs(dt[i] - median) < Tolerance)
                {
                    x[j] = Convert.ToDouble(i);
                    y[j] = Convert.ToDouble(ticks[i] - ticks[0]);
                    j++;
                }
            }

            if (j != validPoints)
            {
                throw new InvalidOperationException(
                    "Number of valid data points is inconsistent");
            }

            var regression = new SimpleLinearRegression();
            regression.Regress(x, y);
            var result = Convert.ToInt64(regression.Compute(-1)) + ticks[0];
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
                if (Math.Abs(dt - median) < Tolerance)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
