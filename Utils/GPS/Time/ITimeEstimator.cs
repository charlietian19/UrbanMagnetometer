namespace Utils.GPS.Time
{
    public interface ITimeEstimator
    {
        /* Returns an interpolated timestamp based on previous data. */
        GpsData GetTimeStamp(long counter);

        /* Add a GPS timing information as a data point.
        Returns true if the point is valid.
        TODO: make this void, update PPS_Tracker to not use this flag. */
        bool PutTimestamp(GpsData data);

        /* Re-calculated the prediction model. */
        void Update();
    }
}
