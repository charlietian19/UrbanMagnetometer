using System;

namespace Utils.Filters
{
    public class MovingAverage : AbstractSimpleFilter
    {
        double a;
        double yz_1;

        /* Creates a running average filter given how many points to average
        and zero initial conditions. */
        public MovingAverage(int n)
        {
            Initialize(n, 0.0);
        }

        /* Creates a running average filter given how many points to average
        and the initial conditions. */
        public MovingAverage(int n, double y0)
        {
            Initialize(n, y0);
        }

        /* Initializes filter parameters and initial conditions. */
        private void Initialize(int n, double y0)
        {
            if (n < 1)
            {
                var msg = "Filter ratio can't be smaller than one";
                throw new ArgumentOutOfRangeException(msg);
            }
            a = 1.0 / n;
            yz_1 = y0;
        }

        /* Moving average filter function */
        override protected double[] Filter(double[] data)
        {
            var result = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = a * data[i] + (1 - a) * yz_1;
                yz_1 = result[i];
            }
            return result;
        }
    }
}
