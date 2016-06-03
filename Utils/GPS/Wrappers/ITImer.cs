using System.Timers;

namespace Utils.GPS
{
    public interface ITimer
    {
        event ElapsedEventHandler Elapsed;
        double Interval { get; set; }
        bool Enabled { get; set; }
        bool AutoReset { get; set; }
    }
}
