using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.Filters;

namespace UnitTests.Filters
{
    [TestClass]
    public class MovingAverageTest
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
        public void NOutOfRangeSingleArgConstructor()
        {
            var filter = new MovingAverage(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NOutOfRangeTwoArgsConstructor()
        {
            var filter = new MovingAverage(0, 0);
        }

        [TestMethod]
        public void Sample1()
        {
            var data = new double[] { 1, 0, 1 };
            var output = new double[] { 0.5, 0.25, 0.625 };
            var filter = new MovingAverage(2);
            filter.output += new FilterEvent(outputHandler);
            filter.InputData(data);
            CollectionAssert.AreEqual(output, lastOutput);
        }

        [TestMethod]
        public void Sample2()
        {
            var data = new double[] { 1, 0, 0, 0 };
            var output = new double[] { 0.5, 0.25, 0.125, 0.0625 };
            var filter = new MovingAverage(2);
            filter.output += new FilterEvent(outputHandler);
            filter.InputData(data);
            CollectionAssert.AreEqual(output, lastOutput);
        }

        [TestMethod]
        public void TwoSamples1()
        {
            var data = new double[] { 1, 0, 0, 0 };
            var output1 = new double[] { 0.5, 0.25, 0.125, 0.0625 };
            var output2 = new double[] { 0.53125, 0.265625, 0.1328125, 0.06640625 };
            var filter = new MovingAverage(2);
            filter.output += new FilterEvent(outputHandler);
            filter.InputData(data);
            CollectionAssert.AreEqual(output1, lastOutput);
            filter.InputData(data);
            CollectionAssert.AreEqual(output2, lastOutput);
        }
    }
}
