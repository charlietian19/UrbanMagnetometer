using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Filters
{
    /* Accumulates SIZE most recent samples. New samples roll over the old ones. */
    public class RollingBuffer : AbstractSimpleFilter
    {
        private int size;
        double[] buffer;

        public RollingBuffer(int size)
        {
            if (size < 1)
            {
                var msg = "Buffer size can't be smaller than one";
                throw new ArgumentOutOfRangeException(msg);
            }

            this.size = size;
            buffer = Enumerable.Repeat<double>(0, size).ToArray();
        }

        override protected double[] Filter(double[] data)
        {
            if (data.Length == 0)
            {
                return buffer;
            }

            if (data.Length < buffer.Length)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    buffer[i] = buffer[i + buffer.Length - data.Length];
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

            return buffer;
        }
    }
}
