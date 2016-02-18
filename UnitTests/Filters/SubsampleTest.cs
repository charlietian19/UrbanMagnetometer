using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.Filters;

namespace UnitTests.Filters
{
    [TestClass]
    public class SubsampleTest
    {
        private double[] lastOutput = null;
        private int timesCalled = 0;
        private void outputHandler(double[] data)
        {
            lastOutput = data;
            timesCalled++;
        }

        [TestMethod]
        public void SingleSampleRatio1()
        {
            var data = new double[] { 1, 2, 3, 4, 5, 6 };
            var filter = new Subsample(1);
            filter.output += new FilterEvent(outputHandler);
            filter.InputData(data);
            CollectionAssert.AreEqual(data, lastOutput);
        }

        [TestMethod]
        public void SingleSampleRatio2()
        {
            var data = new double[] { 1, 2, 3, 4, 5, 6 };
            var filter = new Subsample(2);
            filter.output += new FilterEvent(outputHandler);
            filter.InputData(data);
            CollectionAssert.AreEqual(new double[] { 2, 4, 6 }, lastOutput);
        }

        [TestMethod]
        public void SingleSampleRatio3()
        {
            var data = new double[] { 1, 2, 3, 4, 5, 6 };
            var filter = new Subsample(3);
            filter.output += new FilterEvent(outputHandler);
            filter.InputData(data);
            CollectionAssert.AreEqual(new double[] { 3, 6 }, lastOutput);
        }

        [TestMethod]
        public void SingleSampleRatio4()
        {
            var data = new double[] { 1, 2, 3, 4, 5, 6 };
            var filter = new Subsample(4);
            filter.output += new FilterEvent(outputHandler);
            filter.InputData(data);
            CollectionAssert.AreEqual(new double[] { 4 }, lastOutput);
        }

        [TestMethod]
        public void MultipleSamples1()
        {
            var filter = new Subsample(3);
            filter.output += new FilterEvent(outputHandler);
            filter.InputData(new double[] { 1, 2 });
            Assert.AreEqual(null, lastOutput);
            filter.InputData(new double[] { 3, 4, 5 });
            CollectionAssert.AreEqual(new double[] { 3 }, lastOutput);
        }

        [TestMethod]
        public void MultipleSamples2()
        {
            var filter = new Subsample(3);
            filter.output += new FilterEvent(outputHandler);
            filter.InputData(new double[] { 1 });
            Assert.AreEqual(null, lastOutput);
            filter.InputData(new double[] { 2 });
            Assert.AreEqual(null, lastOutput);
            filter.InputData(new double[] { 3, 4, 5 });
            CollectionAssert.AreEqual(new double[] { 3 }, lastOutput);
            filter.InputData(new double[] { 6 });
            CollectionAssert.AreEqual(new double[] { 6 }, lastOutput);
        }

        [TestMethod]
        public void MultipleSamples3()
        {
            var filter = new Subsample(3);
            filter.output += new FilterEvent(outputHandler);
            filter.InputData(new double[] { 1, 2, 3, 4 });
            CollectionAssert.AreEqual(new double[] { 3 }, lastOutput);
            filter.InputData(new double[] { 5, 6, 7, 8, 9 });
            CollectionAssert.AreEqual(new double[] { 6, 9 }, lastOutput);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RatioOutOfRange()
        {
            var data = new double[] { 1, 2, 3, 4, 5, 6 };
            var filter = new Subsample(0);
        }
    }
}
