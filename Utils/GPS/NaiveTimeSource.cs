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
        public int MinPointsToFit
        {
            get { return MinPointsToFit; }
            set
            {
                if (value < 2)
                {
                    throw new ArgumentOutOfRangeException(
                        "Need at least two point to fit a line");
                }
                MinPointsToFit = value;
            }
        }

        /* The linear fit coefficients */
        public SimpleLinearRegression regression
        {
            get; private set;
        } = null;

        /* Mutex that ensures the regression model updates are thread safe */
        Mutex mutex = new Mutex();

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
            MinPointsToFit = 2;
            storage = new NaiveTimeStorage();
        }

        /* Create a NaiveTimeSource provided a TimeStorage */
        public NaiveTimeSource(ITimeStorage storage)
        {
            MinPointsToFit = 2;
            this.storage = storage;
        }

        /* Updates the linear fit when a new GPS data arrives.
        I wonder if the long -> double conversion will be an issue. */
        private void UpdateModel()
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
                UpdateModel();
            }
            return valid;
        }

        /* Reports slope and intercept */
        public void ReportModel()
        {
            if (regression == null)
            {
                return;
            }

            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(string.Format("y = {0}*x + {1}",
                regression.Slope, regression.Intercept));
            Console.ForegroundColor = color;            
        }
    }
}
