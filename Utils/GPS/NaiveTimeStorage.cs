using System;
using System.Diagnostics;

/*
    Class that stores a given amount of last GPS data points for a TimeSource
    to use. Decides whether the GPS data is valid based on the GPS message, and
    whether each GPS time is more than threshold seconds late than the previously
    observed data.
*/

namespace Utils.GPS
{    
    public class NaiveTimeStorage : ITimeStorage
    {
        /* Default time source parameters */
        /* Use the GPS data from up to 2 minutes ago */
        const int maxHistoryDefault = 120;
        /* Point rejection threshold is 50 us */
        const double thresholdDefault = 50 * 1e-6;
        /* Compare each new GPS point to 5 previous points*/
        const int lookbackDefault = 5;

        /* How many previous GPS points to store. */
        public readonly int maxHistory;

        /* Stores the GPS data history */
        public GpsData[] history
        {
            get; private set;
        }

        /* If the point arrives later than this many ticks, it's invalid */
        public readonly long threshold;

        /* Stopwatch freqeuncy */
        public readonly long frequency = Stopwatch.Frequency;

        /* How many previous points should within the threshold of the new one
        for it to be counted as valid */
        public readonly int lookback;

        /* How many GPS points have been received up to now. */
        public long pointsReceived
        {
            get; private set;
        } = 0;

        /* Create a new threshold TimeSource given threshold in seconds,
            how many previous points to compare to. */
        public NaiveTimeStorage(double threshold, int maxHistory, int lookback)
        {
            if (threshold < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "TimeSource threshold can't be negative");
            }

            if (maxHistory < 2)
            {
                throw new ArgumentOutOfRangeException(
                    "TimeSource history length can't be less than two");
            }

            if (lookback < 1)
            {
                throw new ArgumentOutOfRangeException(
                    "TimeSource lookback can't be less than one");
            }

            if (lookback > maxHistory)
            {
                throw new ArgumentOutOfRangeException(
                    "TimeSource lookback can't be larger than history");
            }

            this.threshold = Convert.ToInt64(threshold * frequency);
            this.maxHistory = maxHistory;
            this.lookback = lookback;
            InitializeHistory();
        }

        /* Initialize the GPS history */
        private void InitializeHistory()
        {
            history = new GpsData[maxHistory];
            for (int i = 0; i < maxHistory; i++)
            {
                history[i].valid = false;
            }
        }

        /* Creates a TimeSource with default parameters */
        public NaiveTimeStorage() : this(thresholdDefault, maxHistoryDefault,
            lookbackDefault) { }

        /* Returns the number of valid points currently stored. */
        public int ValidPointsCount { get { return GetValidPointsCount(); } }

        /* Stores a new point, returns true if the point is valid and false 
        otherwise. */
        public bool Store(GpsData data)
        {
            if (!data.valid)
            {                
                return false; // GPS receiver said this point is invalid
            }

            bool valid = IsWithinThreshold(data);
            pointsReceived += 1;
            SavePoint(data, valid);

            if (pointsReceived < lookback)
            {
                return false;
            }

            return valid;
        }

        /* Checks the point against other points in history */
        private bool IsWithinThreshold(GpsData data)
        {
            bool valid;
            if (pointsReceived < lookback)
            {
                valid = false;
            }
            else
            {
                valid = true;
                for (int i = 0; i < lookback; i++)
                {
                    // TODO!! this dt is incorrect, find the right one from timestamps!!
                    valid &= IsWithinThresholdHelper(data, history[i], i + 1);
                    if (!valid) break;
                }
            }
            return valid;
        }

        /* Returns true if the point from dt seconds ago is within 
           the threshold */
        private bool IsWithinThresholdHelper(GpsData newPoint, 
            GpsData oldPoint, int dt)
        {
            long delta = newPoint.ticks - oldPoint.ticks;
            return delta - frequency * dt < threshold;
        }

        /* Stores the given GPS data into the buffer. */
        private void SavePoint(GpsData data, bool valid)
        {
            for (int i = history.Length - 1; i > 0; i--)
            {
                history[i] = history[i - 1];
            }
            history[0] = data;
            history[0].valid = valid;
        }        

        /* Returns the number of valid points stored in history */
        private int GetValidPointsCount()
        {
            int num = 0;
            foreach (var point in history)
            {
                if (point.valid)
                {
                    num++;
                }
            }
            return num;
        }

        /* Returns valid points from history */
        public GpsData[] GetValidPoints()
        {
            var result = new GpsData[GetValidPointsCount()];
            int i = 0;

            foreach (var point in history)
            {
                if (point.valid)
                {
                    result[i] = point;
                    i++;
                }
            }
            return result;
        }
    }
}
