using MathNet.Numerics.LinearAlgebra;
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
            var dsm = new DSMMatrix(TestData.SimpleTestGraph1);
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
        public void CreateGraphFromDsmMatrix(){
            var dsm = new DSMMatrix(TestData.SimpleTestGraph1);
            var graph = dsm.GraphFromDSM();
            Assert.IsNotNull(graph);
        }

        [TestMethod]
        public void DsmMatrixPartition(){
            var dsm = new DSMMatrix(TestData.DSMFull);
            var partitioned = dsm.Partition();
            Assert.IsNotNull(partitioned);
        }

    }
}