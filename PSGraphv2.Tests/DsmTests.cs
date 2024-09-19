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
using PSGraph.Tests;

namespace PSGraph.Tests
{
    [TestClass]
    public class DsmTests
    {
        [TestMethod]
        public void DsmBaseBasicTest()
        {
            var dsm = new DsmClassic(TestData.SimpleTestGraph1);
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
        public void BasicPartitioningTest()
        {
            var dsm = new DsmClassic(TestData.DSMFull);
            Assert.IsNotNull(dsm);

            var algo = new DsmClassicPartitioningAlgorithm(dsm);
            var ret = algo.Partition();
            
            
            float[,] target = { {0,0,0,0,0,0,0},
                                {0,0,0,1,0,0,0},
                                {0,1,0,0,0,0,0},
                                {1,0,1,0,0,0,0},
                                {0,0,1,0,0,1,0},
                                {1,1,0,1,1,0,0},
                                {1,0,0,0,0,1,0}};

            var targetStore = DenseColumnMajorMatrixStorage<Single>.OfArray(target);
            var targetMatrix = Matrix<Single>.Build.Dense(targetStore);
            
            Assert.AreEqual(targetMatrix, ret.DsmMatrixView);
            
            var idx = 0;
            string[] idx2 = { "F", "B", "D", "G", "A", "C", "E" };
            foreach (var el in ret.RowIndex)
            {
                Assert.AreEqual(idx2[idx], el.Key.Name);
                Assert.AreEqual(idx, el.Value);

                idx++;
            }
        }
        
        [TestMethod]
        public void Graph5ClassicPartitioningTest()
        {
            var dsm = new DsmClassic(TestData.SimpleTestGraph5);
            Assert.IsNotNull(dsm);

            var algo = new DsmClassicPartitioningAlgorithm(dsm);
            var part = algo.Partition();
            
            Assert.AreEqual(algo.Partitioned.RowIndex.Count, algo.Partitioned.DsmMatrixView.RowCount);
            Assert.AreEqual(algo.Partitioned.ColIndex.Count, algo.Partitioned.DsmMatrixView.ColumnCount);
            
            var view = new DsmView(part, algo.Partitions);
            var s = view.ToSvg();
            var f = System.IO.Path.GetTempPath();
            var p = System.IO.Path.Combine(f, "dsmViewPartitionedGraph5Test.svg");
            s.Write(p);
        }
        
        [TestMethod]
        public void Graph5GraphBasedPartitioningTest()
        {
            var dsm = new DsmClassic(TestData.SimpleTestGraph5);
            Assert.IsNotNull(dsm);

            var algo = new DsmGraphPartitioningAlgorithm(dsm);
            var part = algo.Partition();
            
            Assert.AreEqual(algo.Partitioned.RowIndex.Count, algo.Partitioned.DsmMatrixView.RowCount);
            Assert.AreEqual(algo.Partitioned.ColIndex.Count, algo.Partitioned.DsmMatrixView.ColumnCount);
            
            var view = new DsmView(part, algo.Partitions);
            var s = view.ToSvg();
            var f = System.IO.Path.GetTempPath();
            var p = System.IO.Path.Combine(f, "dsmViewPartitionedGraph5Test.svg");
            s.Write(p);
        }
    }
}