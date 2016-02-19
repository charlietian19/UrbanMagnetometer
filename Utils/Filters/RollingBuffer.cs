using System;
using System.Linq;
using System.Threading;

namespace Utils.Filters
{
    /* Accumulates SIZE most recent samples. New samples roll over the old ones. */
    public class RollingBuffer : AbstractSimpleFilter
    {
        private int size;
        double[] buffer;
        Mutex mutex = new Mutex();

        public RollingBuffer(int size)
        {
            InitFilter(size, 0.0);
        }

        public RollingBuffer(int size, double initValue)
        {
            InitFilter(size, initValue);
        }

        private void InitFilter(int size, double initValue)
        {
            if (size < 1)
            {
                var msg = "Buffer size can't be smaller than one";
                throw new ArgumentOutOfRangeException(msg);
            }

            this.size = size;
            buffer = Enumerable.Repeat(initValue, size).ToArray();
        }

        override protected double[] Filter(double[] data)
        {
            if (data.Length == 0)
            {
                return buffer;
            }

            mutex.WaitOne();
            if (data.Length < buffer.Length)
            {
                for (int i = 0; i < buffer.Length - data.Length; i++)
                {
                    buffer[i] = buffer[i + data.Length];
                }
                for (int i = 0; i < data.Length; i++)
                {
                    buffer[i + buffer.Length - data.Length] = data[i];
                }
            }
            else
            {
                for (int i = data.Length - buffer.Length; i < data.Length; i++)
                {
                    buffer[i - (data.Length - buffer.Length)] = data[i];
                }
            }
            mutex.ReleaseMutex();

            return buffer;
        }

        /* Returns the current buffer contents. */
        public double[] GetData()
        {
            mutex.WaitOne();
            double[] result = buffer;
            mutex.ReleaseMutex();
            return result;
        }
    }
}
