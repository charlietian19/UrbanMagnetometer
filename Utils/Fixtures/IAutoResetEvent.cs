using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Fixtures
{
    public interface IAutoResetEvent
    {
        void Set();
        void Reset();
        void WaitOne();
        void WaitOne(int timeout);
    }
}
