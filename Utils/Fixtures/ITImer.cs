using System.Timers;

namespace Utils.Fixtures
{
    public interface ITimer
    {
        event ElapsedEventHandler Elapsed;
        double Interval { get; set; }
        bool Enabled { get; set; }
        bool AutoReset { get; set; }
    }
}
