using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.Filters;

namespace UnitTests.Filters
{
    [TestClass]
    public class BufferTest
    {
        private double[] lastOutput = null;
        private int timesCalled = 0;
        private void outputHandler(double[] data)
        {
            lastOutput = data;
            timesCalled++;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SizeOutOfRange()
        {
            var filter = new RollingBuffer(0);
        }

        [TestMethod]        
        public void BufferIsZeroed()
        {
            var data = new double[] { };
            var output = new double[] { 0, 0, 0, 0 };
            var filter = new RollingBuffer(4);
            filter.output += new Action<double[]>(outputHandler);
            filter.InputData(data);
            CollectionAssert.AreEqual(output, lastOutput);
        }

        [TestMethod]
        public void SamplesSmallerThanSize()
        {
            var data1 = new double[] { 1, 2, 3 };
            var data2 = new double[] { 1, 2 };
            var output1 = new double[] { 0, 1, 2, 3 };
            var output2 = new double[] { 2, 3, 1, 2 };
            var filter = new RollingBuffer(4);
            filter.output += new Action<double[]>(outputHandler);
            filter.InputData(data1);
            CollectionAssert.AreEqual(output1, lastOutput);
            filter.InputData(data2);
            CollectionAssert.AreEqual(output2, lastOutput);
        }

        [TestMethod]
        public void SamplesLargerThanSize()
        {
            var data1 = new double[] { 1, 2, 3, 4, 5 };
            var data2 = new double[] { 6, 7, 8, 9, 10 };
            var output1 = new double[] { 3, 4, 5 };
            var output2 = new double[] { 8, 9, 10 };
            var filter = new RollingBuffer(3);
            filter.output += new Action<double[]>(outputHandler);
            filter.InputData(data1);
            CollectionAssert.AreEqual(output1, lastOutput);
            filter.InputData(data2);
            CollectionAssert.AreEqual(output2, lastOutput);
        }

    }
}
