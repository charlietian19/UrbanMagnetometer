using System.Threading;

namespace Utils.Fixtures
{
    /* AutoResetEvent wrapper for testing */
    public class AutoResetEventWrapper : IAutoResetEvent
    {
        AutoResetEvent myEvent;
        public AutoResetEventWrapper(bool initialState)
        {
            myEvent = new AutoResetEvent(initialState);
        }

        public void Set()
        {
            myEvent.Set();
        }

        public void Reset()
        {
            myEvent.Reset();
        }

        public void WaitOne()
        {
            myEvent.WaitOne();
        }

        public void WaitOne(int timeout)
        {
            myEvent.WaitOne(timeout);
        }
    }
}
