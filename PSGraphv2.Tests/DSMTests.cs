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

        private static PSBidirectionalGraph _simpleTestGraph1;
        private static PSBidirectionalGraph _simpleTestGraph2;
        private static PSBidirectionalGraph _simpleTestGraph3;

        [ClassInitialize()]
        public static void InitPsGraphUnitTest(TestContext tc)
        {
            InitializeSimpleTestGraph1();
            InitializeSimpleTestGraph2();
            InitializeSimpleTestGraph3();
        }

        private static void InitializeSimpleTestGraph1()
        {
            var g = new PSBidirectionalGraph();

            g.AddVertex(new PSVertex("A"));
            g.AddVertex(new PSVertex("B"));
            g.AddVertex(new PSVertex("C"));
            g.AddVertex(new PSVertex("D"));
            g.AddVertex(new PSVertex("E"));

            g.AddEdge(new PSEdge(new PSVertex("A"), new PSVertex("C"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("C"), new PSVertex("A"), new PSEdgeTag()));

            g.AddEdge(new PSEdge(new PSVertex("C"), new PSVertex("E"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("E"), new PSVertex("C"), new PSEdgeTag()));


            g.AddEdge(new PSEdge(new PSVertex("D"), new PSVertex("C"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("B"), new PSVertex("E"), new PSEdgeTag()));

            _simpleTestGraph1 = g;

            //throw new NotImplementedException();
        }

        private static void InitializeSimpleTestGraph2()
        {
            var g = new PSBidirectionalGraph();

            g.AddVertex(new PSVertex("A"));
            g.AddVertex(new PSVertex("B"));
            g.AddVertex(new PSVertex("C"));
            g.AddVertex(new PSVertex("D"));
            g.AddVertex(new PSVertex("E"));
            g.AddVertex(new PSVertex("F"));
            g.AddVertex(new PSVertex("G"));




            _simpleTestGraph1 = g;

            g.AddEdge(new PSEdge(new PSVertex("F"), new PSVertex("A"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("E"), new PSVertex("A"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("B"), new PSVertex("A"), new PSEdgeTag()));

            g.AddEdge(new PSEdge(new PSVertex("G"), new PSVertex("B"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("D"), new PSVertex("B"), new PSEdgeTag()));

            g.AddEdge(new PSEdge(new PSVertex("G"), new PSVertex("C"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("D"), new PSVertex("C"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("B"), new PSVertex("C"), new PSEdgeTag()));

            g.AddEdge(new PSEdge(new PSVertex("B"), new PSVertex("D"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("C"), new PSVertex("D"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("E"), new PSVertex("D"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("G"), new PSVertex("D"), new PSEdgeTag()));

            g.AddEdge(new PSEdge(new PSVertex("D"), new PSVertex("E"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("F"), new PSVertex("E"), new PSEdgeTag()));

            g.AddEdge(new PSEdge(new PSVertex("D"), new PSVertex("E"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("F"), new PSVertex("E"), new PSEdgeTag()));

            g.AddEdge(new PSEdge(new PSVertex("A"), new PSVertex("F"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("E"), new PSVertex("F"), new PSEdgeTag()));

            g.AddEdge(new PSEdge(new PSVertex("B"), new PSVertex("G"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("C"), new PSVertex("G"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("D"), new PSVertex("G"), new PSEdgeTag()));

            System.IO.File.WriteAllText("c:\\temp\\test2.gv", g.ToGraphviz()); 

            _simpleTestGraph2 = g;

            //throw new NotImplementedException();
        }

        private static void InitializeSimpleTestGraph3()
        {
            var g = new PSBidirectionalGraph();

            g.AddVertex(new PSVertex("A"));
            g.AddVertex(new PSVertex("B"));
            g.AddVertex(new PSVertex("C"));
            g.AddVertex(new PSVertex("D"));
            g.AddVertex(new PSVertex("E"));
            g.AddVertex(new PSVertex("F"));
            g.AddVertex(new PSVertex("G"));
            g.AddVertex(new PSVertex("H"));

            g.AddEdge(new PSEdge(new PSVertex("D"), new PSVertex("A"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("H"), new PSVertex("A"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("E"), new PSVertex("B"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("G"), new PSVertex("B"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("E"), new PSVertex("C"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("F"), new PSVertex("D"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("H"), new PSVertex("D"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("B"), new PSVertex("E"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("C"), new PSVertex("E"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("G"), new PSVertex("E"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("A"), new PSVertex("F"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("H"), new PSVertex("F"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("B"), new PSVertex("G"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("C"), new PSVertex("G"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("A"), new PSVertex("H"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("D"), new PSVertex("H"), new PSEdgeTag()));


            System.IO.File.WriteAllText("c:\\temp\\test3.gv", g.ToGraphviz());

            _simpleTestGraph3 = g;
        }

        [TestMethod]
        public void BasicDsmStorageTest()
        {
            var dsmStorage = new DsmStorage(_simpleTestGraph1);
            Assert.IsNotNull(dsmStorage);
        }

        [TestMethod]
        public void TestGetIndexer()
        {
            var dsmStorage = new DsmStorage(_simpleTestGraph1);

            // TODO: in case Tags are used as weights Asserts need to change
            Assert.AreEqual(1, dsmStorage["A", "C"]);
            Assert.AreEqual(1, dsmStorage["C", "A"]);

            Assert.AreEqual(1, dsmStorage["C", "E"]);
            Assert.AreEqual(1, dsmStorage["E", "C"]);

            Assert.AreEqual(1, dsmStorage["D", "C"]);

            Assert.AreEqual(1, dsmStorage["B", "E"]);


            Assert.AreEqual(0, dsmStorage["B", "A"]);
            Assert.AreEqual(0, dsmStorage["E", "B"]);

            // if i = j -> it is a diagonal and the weight is 0
            Assert.AreEqual(0, dsmStorage["E", "E"]);
        }

        [TestMethod]
        public void TestBasicElementList()
        {
            var dsmStorage = new DsmStorage(_simpleTestGraph1);
            List<object> list = new List<object>() { "A", "B", "C", "D", "E"};

            ///Assert.AreEqual(list, dsmStorage.Objects);
        }

        [TestMethod]
        public void BasicClusterTest()
        {
            var dsm = new Dsm(_simpleTestGraph2);
            dsm.Cluster();

        }

        [TestMethod]
        public void BasicClusterOrderTest()
        {
            var dsm = new Dsm(_simpleTestGraph2);
            dsm.Cluster();
            var r = dsm.Order();

        }

        [TestMethod]
        public void BasicDsmExportTest()
        {
            var dsm = new Dsm(_simpleTestGraph2);
            dsm.Cluster();
            var r = dsm.Order();
            r.ExportSvg("c:\\temp\\basictest.svg");
        }

        //[TestMethod]
        //public void CreateEmptyDSM()
        //{
        //    var m = Matrix<Single>.Build.Random(100, 100, Random.Shared.Next());
        //    var tstDsm = new dsm(m);
        //}

        //[TestMethod]
        //public void DsmStorageBasic()
        //{

        //}

        //[TestMethod]
        //public void ClusterDSM()
        //{
        //    var m = Matrix<Single>.Build.Dense(10, 10);

        //    var r = new Random();
        //    var list = new List<int> { 0, 1 };

        //    for (int i = 0; i < m.RowCount; i++)
        //    {
        //        for (int j = 0; j < m.ColumnCount; j++)
        //        {
        //            m[i,j] = r.Next(list.Count);
        //        }
        //    }


        //    var tstDsm = new dsm(m);
        //    tstDsm.Cluster();
        //}

    }
}
