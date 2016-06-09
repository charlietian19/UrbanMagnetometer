namespace Utils.Fixtures
{
    public interface IStopwatch
    {
        long Frequency { get; }
        long GetTimestamp();
    }
}
