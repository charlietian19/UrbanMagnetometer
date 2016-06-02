/* Determines if each given timestamp/tick counter combination
   corresponds to a valid or delayed PPS signal  */

namespace Utils.GPS
{
    public interface ITimeStorage
    {
        /* Add the time data point to the storage. Returns true if the point
        is valid, and false otherwise. */
        bool Store(GpsData data);

        /* Returns the number of valid points currently in the storage. */
        int ValidPointsCount { get; }

        /* Returns the array of valid points from the storage. */
        GpsData[] GetValidPoints();
    }
}
