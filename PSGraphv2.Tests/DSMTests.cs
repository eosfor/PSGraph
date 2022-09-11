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
    public class DSMTests
    {

        [TestMethod]
        public void BasicDsmStorageTest()
        {
            var dsmStorage = new DsmStorage(TestData.SimpleTestGraph1);
            Assert.IsNotNull(dsmStorage);
        }

        [TestMethod]
        public void TestGetIndexer()
        {
            var dsmStorage = new DsmStorage(TestData.SimpleTestGraph1);

            // TODO: in case Tags are used as weights Asserts need to change
            Assert.AreEqual(1, dsmStorage[new PSVertex("A"), new PSVertex("C")]);
            Assert.AreEqual(1, dsmStorage[new PSVertex("C"), new PSVertex("A")]);


            Assert.AreEqual(1, dsmStorage[new PSVertex("C"), new PSVertex("E")]);
            Assert.AreEqual(1, dsmStorage[new PSVertex("E"), new PSVertex("C")]);

            Assert.AreEqual(1, dsmStorage[new PSVertex("D"), new PSVertex("C")]);

            Assert.AreEqual(1, dsmStorage[new PSVertex("B"), new PSVertex("E")]);

            Assert.AreEqual(0, dsmStorage[new PSVertex("B"), new PSVertex("A")]);
            Assert.AreEqual(0, dsmStorage[new PSVertex("E"), new PSVertex("B")]);

            // if i = j -> it is a diagonal and the weight is 0
            Assert.AreEqual(0, dsmStorage[new PSVertex("E"), new PSVertex("E")]);
        }

        [TestMethod]
        public void TestBasicElementList()
        {
            var dsmStorage = new DsmStorage(TestData.SimpleTestGraph1);
        }

        [TestMethod]
        public void BasicClusterTest()
        {
            var dsm = new Dsm(TestData.SimpleTestGraph3);
            dsm.Cluster();

        }

        [TestMethod]
        public void BasicClusterOrderTest()
        {
            var dsm = new Dsm(TestData.SimpleTestGraph3);
            dsm.Cluster();
            dsm.Order();
        }

        [TestMethod]
        public void BasicDsmExportTest()
        {
            var dsm = new Dsm(TestData.SimpleTestGraph3);
            dsm.Cluster();
            dsm.Order();
            var f = System.IO.Path.GetTempPath();
            var p = System.IO.Path.Combine(f, "basictest.svg");
            dsm.ExportSvg(p);
        }

        [TestMethod]
        public void GetOutEdgesTest()
        {
            var dsm = new Dsm(TestData.SimpleTestGraph4);
            dsm.Cluster();
            foreach (var c in dsm.Clusters)
            {
                var r = c.GetOutEdges(dsm.Clusters, dsm);
            }
        }
    }
}
