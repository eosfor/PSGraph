using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSGraph.DSM;
using System;

namespace PSGraphv2.Tests
{
    [TestClass]
    public class DSMTests
    {
        [ClassInitialize()]
        public static void InitPsGraphUnitTest(TestContext tc)
        {
        }

        [TestMethod]
        public void CreateEmptyDSM()
        {
            var m = Matrix<Single>.Build.Random(100, 100, Random.Shared.Next());
            var tstDsm = new dsm(m);
        }

        [TestMethod]
        public void ClusterDSM()
        {
            var m = Matrix<Single>.Build.Random(100, 100, Random.Shared.Next());
            var tstDsm = new dsm(m);
            tstDsm.Cluster();
        }

    }
}