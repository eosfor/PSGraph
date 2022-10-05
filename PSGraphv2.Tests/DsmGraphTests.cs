using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSGraph.DesignStructureMatrix;

namespace PSGraph.Tests;

[TestClass]
public class DsmGraphTests
{
        [TestMethod]
        public void DsmGraphBasicTest()
        {
                var dsm = new DsmClassic(TestData.DSMFull);
                Assert.IsNotNull(dsm);

                var algo = new DsmGraphPartitioningAlgorithm(dsm);
                var ret = algo.Partition();
        }

}