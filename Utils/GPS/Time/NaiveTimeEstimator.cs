using System;
using System.Threading;
using Accord.Statistics.Models.Regression.Linear;

/* 
    Class that sets a correspondence between counter ticks and GPS 
    timestamps with a least square linear fit. It's a placeholder until 
    a better model is ready and for testing. Not intended to go into 
    production.
    
    The accuracy is within the spec, but it's unclear how robust it is.

*/

namespace Utils.GPS
{
    public class NaiveTimeEstimator : ITimeEstimator
    {
        /* Most recent GPS data */
        private GpsData recentGpsData = new GpsData()
        {
            ticks = 0,
            timestamp = DateTime.Now,
            valid = false,
            longitude = 0,
            latitude = 0,
            speedKnots = 0,
            angleDegrees = 0,
            ew = "-",
            ns = "-",
            active = "V"
        };

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
        private Mutex modelMutex = new Mutex();

        /* Mutex that ensures the recent GPS data is thread safe */
        private Mutex gpsDataMutex = new Mutex();

        /* Some initial date and tick that the data will be counted from so 
        the rounding errors are less of a problem */
        DateTime startTimestamp;
        long startTick;

        /* Where the GPS data is stored and discriminated on valid/invalid 
        points */
        ITimeValidator storage;

        /* Creates a NaiveTimeSource using a default NaiveTimeStorage. */
        public NaiveTimeEstimator()
        {
            storage = new NaiveTimeValidator();
        }

        /* Create a NaiveTimeSource provided a TimeStorage */
        public NaiveTimeEstimator(ITimeValidator storage)
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

            modelMutex.WaitOne();
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
            modelMutex.ReleaseMutex();
        }

        /* Returns absolute timing data based on the previous timestamps. */
        public GpsData GetTimeStamp(long counter)
        {
            if (regression == null)
            {
                /* There is no model, can't return a valid timestamp */
                gpsDataMutex.WaitOne();
                var data = new GpsData()
                {
                    ticks = counter,
                    timestamp = DateTime.Now,
                    valid = false,
                    longitude = recentGpsData.longitude,
                    latitude = recentGpsData.latitude,
                    speedKnots = recentGpsData.speedKnots,
                    angleDegrees = recentGpsData.angleDegrees,
                    ew = recentGpsData.ew,
                    ns = recentGpsData.ns,
                    active = recentGpsData.active
                };
                gpsDataMutex.ReleaseMutex();
                return data;
            }

            /* Return the best guess based on the model at hand */
            modelMutex.WaitOne();
            var ticks = Convert.ToInt64(regression.Compute(counter - 
                startTick));
            var date = new DateTime(ticks + startTimestamp.Ticks);
            var result = new GpsData()
            {
                ticks = counter,
                timestamp = date,
                valid = true,                
            };
            modelMutex.ReleaseMutex();

            gpsDataMutex.WaitOne();
            result.longitude = recentGpsData.longitude;
            result.latitude = recentGpsData.latitude;
            result.speedKnots = recentGpsData.speedKnots;
            result.angleDegrees = recentGpsData.angleDegrees;
            result.ew = recentGpsData.ew;
            result.ns = recentGpsData.ns;
            result.active = recentGpsData.active;
            gpsDataMutex.ReleaseMutex();

            return result;
        }

        /* Add the GPS point to the data set. */
        public bool PutTimestamp(GpsData data)
        {
            if (data.active == "A")
            {
                gpsDataMutex.WaitOne();
                recentGpsData.longitude = data.longitude;
                recentGpsData.latitude = data.latitude;
                recentGpsData.speedKnots = data.speedKnots;
                recentGpsData.angleDegrees = data.angleDegrees;
                recentGpsData.ew = data.ew;
                recentGpsData.ns = data.ns;
                recentGpsData.active = data.active;
                gpsDataMutex.ReleaseMutex();
            }

            bool valid = storage.Store(data);
            if (valid)
            {
                Update();
            }

            return valid;
        }        
    }
}
