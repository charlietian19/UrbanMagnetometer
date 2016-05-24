using System;
using System.Threading;
using Accord.Statistics.Models.Regression.Linear;

/* 
    Class that sets a correspondence between counter ticks and GPS 
    timestamps with a least square linear fit. It's a placeholder until 
    a better model is ready and for testing. Not intended to go into 
    production.
    
    The accuracy is within the spec, although it's unclear how robust it is.

    TODO: invalidate the model if no new data arrives for a long time
*/

namespace Utils.GPS
{
    public class NaiveTimeSource : ITimeSource
    {
        /* Minimum points needed to get a valid linear fit */
        private int minPoints = 2;
        public int MinPointsToFit
        {
            get { return minPoints; }
            set
            {
                if (value < 2)
                {
                    throw new ArgumentOutOfRangeException(
                        "Need at least two point to fit a line");
                }
                minPoints = value;
            }
        }

        /* The linear fit coefficients */
        private SimpleLinearRegression regression = null;

        /* Mutex that ensures the regression model updates are thread safe */
        private Mutex mutex = new Mutex();

        /* Some initial date and tick that the data will be counted from so 
        the rounding errors are less of a problem */
        DateTime startTimestamp;
        long startTick;

        /* Where the GPS data is stored and discriminated on valid/invalid 
        points */
        ITimeStorage storage;

        /* Creates a NaiveTimeSource using a default NaiveTimeStorage. */
        public NaiveTimeSource()
        {
            storage = new NaiveTimeStorage();
        }

        /* Create a NaiveTimeSource provided a TimeStorage */
        public NaiveTimeSource(ITimeStorage storage)
        {
            this.storage = storage;
        }

        /* Updates the linear fit when a new GPS data arrives or on demand */
        public void Update()
        {
            var validCount = storage.ValidPointsCount;
            if (validCount < MinPointsToFit)
            {
                return;
            }

            mutex.WaitOne();
            double[] x = new double[validCount];
            double[] y = new double[validCount];

            var history = storage.GetValidPoints();

            startTimestamp = history[0].timestamp;
            startTick = history[0].ticks;

            for (int i = 0; i < history.Length; i++)
            {
                x[i] = Convert.ToDouble(history[i].ticks - startTick);
                y[i] = Convert.ToDouble(history[i].timestamp.Ticks -
                    startTimestamp.Ticks);
            }

            regression = new SimpleLinearRegression();
            regression.Regress(x, y);            
            mutex.ReleaseMutex();
        }

        /* Returns absolute timing data based on the previous timestamps. */
        public GpsData GetTimeStamp(long counter)
        {
            if (regression == null)
            {
                /* There is no model, can't return a valid timestamp */
                return new GpsData()
                {
                    ticks = counter,
                    timestamp = DateTime.Now,
                    valid = false
                };
            }

            /* Return the best guess based on the model at hand */
            mutex.WaitOne();
            var ticks = Convert.ToInt64(regression.Compute(counter - 
                startTick));
            var date = new DateTime(ticks + startTimestamp.Ticks);
            var result = new GpsData()
            {
                ticks = counter,
                timestamp = date,
                valid = true
            };
            mutex.ReleaseMutex();
            return result;
        }

        /* Add the GPS point to the data set. */
        public bool PutTimestamp(GpsData data)
        {
            bool valid = storage.Store(data);
            if (valid)
            {
                Update();
            }
            return valid;
        }        
    }
}
