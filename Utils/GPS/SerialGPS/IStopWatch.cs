namespace Utils.GPS.SerialGPS
{
    public interface IStopwatch
    {
        long Frequency { get; }
        long GetTimestamp();
    }
}
