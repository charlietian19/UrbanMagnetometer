namespace Utils.GPS
{
    public interface IStopwatch
    {
        long Frequency { get; }
        long GetTimestamp();
    }
}
