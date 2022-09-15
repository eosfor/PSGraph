using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSGraph.DesignStructureMatrix;
using QuikGraph;
using QuikGraph.Graphviz.Dot;
using QuikGraph.Graphviz;
using System;
using System.Collections.Generic;
using PSGraph.Model;

namespace PSGraph.Tests
{
    [TestClass]
    public class DSMMatrixTests
    {

        [TestMethod]
        public void CreateDsmMatrixFromGraph()
        {
            var dsm = new DSMMatrixClassic(TestData.SimpleTestGraph1);
            Assert.IsNotNull(dsm);

            Assert.AreEqual(1, dsm[new PSVertex("A"), new PSVertex("C")]);
            Assert.AreEqual(1, dsm[new PSVertex("C"), new PSVertex("A")]);

            Assert.AreEqual(1, dsm[new PSVertex("C"), new PSVertex("E")]);
            Assert.AreEqual(1, dsm[new PSVertex("E"), new PSVertex("C")]);

            Assert.AreEqual(1, dsm[new PSVertex("D"), new PSVertex("C")]);

            Assert.AreEqual(1, dsm[new PSVertex("B"), new PSVertex("E")]);

            Assert.AreEqual(0, dsm[new PSVertex("A"), new PSVertex("A")]);
        }

        [TestMethod]
        public void CreateGraphFromDsmMatrix()
        {
            var dsm = new DSMMatrixClassic(TestData.SimpleTestGraph1);
            var graph = dsm.GraphFromDSM();
            Assert.IsNotNull(graph);
        }

        [TestMethod]
        public void DsmMatrixPartition()
        {
            var dsm = new DSMMatrixClassic(TestData.DSMFull);
            var partitioned = dsm.Partition();

            float[,] target = { {0,0,0,0,0,0,0},
                                {0,0,0,1,0,0,0},
                                {0,1,0,0,0,0,0},
                                {1,0,1,0,0,0,0},
                                {0,0,1,0,0,1,0},
                                {1,1,0,1,1,0,0},
                                {1,0,0,0,0,1,0}};

            var targetStore = DenseColumnMajorMatrixStorage<Single>.OfArray(target);
            var targetMatrix = Matrix<Single>.Build.Dense(targetStore);

            var idx = 0;
            string[] idx2 = { "F", "B", "D", "G", "A", "C", "E" };
            foreach (var el in partitioned.RowIndex)
            {
                Assert.AreEqual(idx2[idx], el.Key.Name);
                Assert.AreEqual(idx, el.Value);

                idx++;
            }

            Assert.AreEqual(targetMatrix, partitioned.Dsm);
        }

        [TestMethod]
        public void ExportSvgTest()
        {
            //var dsm = new DSMMatrixClassic(TestData.DSMFull);
            //var dsmViewSimple = new DSMViewSimple<DSMMatrixClassic>(dsm);
        }
    }


}