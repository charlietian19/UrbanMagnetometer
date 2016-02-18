using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils.Filters;
using Moq;
using Moq.Protected;

namespace UnitTests.Filters
{
    [TestClass]
    public class AbstractSimpleFilterTest
    {
        /* Test filter that returns the input. */
        class UnityFilter : AbstractSimpleFilter
        {
            public UnityFilter() { }
            override protected double[] Filter(double[] x)
            {
                lastInput = x;
                timesCalled++;
                return x;
            }

            public int timesCalled = 0;
            public double[] lastInput = null;            
        }

        private double[] lastOutput = null;
        private int timesCalled = 0;
        private void outputHandler(double[] data)
        {
            lastOutput = data;
            timesCalled++;
        }        

        [TestMethod]
        public void FilterFunctionIsCalled()
        {
            var data = new double[] { 1.1, 2.0, 5.0, 3.0 };
            var filter = new UnityFilter();
            filter.InputData(data);
            Assert.AreEqual(1, filter.timesCalled);
            CollectionAssert.AreEqual(data, filter.lastInput);
        }

        [TestMethod]
        public void OutputEventNonEmptyResult()
        {
            var data = new double[] { 4, 12, 634, 22, 3 };
            var filter = new UnityFilter();
            filter.output += new FilterEvent(outputHandler);
            filter.InputData(data);
            Assert.AreEqual(1, timesCalled);
            CollectionAssert.AreEqual(data, lastOutput);
        }

        [TestMethod]
        public void OutputEventEmptyResult()
        {
            var data = new double[] { };
            var filter = new UnityFilter();
            filter.output += new FilterEvent(outputHandler);
            filter.InputData(data);
            Assert.AreEqual(0, timesCalled);
        }
    }
}
