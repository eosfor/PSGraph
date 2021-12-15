using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSGraph.DesignStructureMatrix;
using QuikGraph;
using QuikGraph.Graphviz.Dot;
using QuikGraph.Graphviz;
using System;
using System.Collections.Generic;

namespace PSGraph.Tests
{
    [TestClass]
    public class DSMTests
    {

        private static BidirectionalGraph<object, STaggedEdge<object, object>> _simpleTestGraph1;
        private static BidirectionalGraph<object, STaggedEdge<object, object>> _simpleTestGraph2;
        private static BidirectionalGraph<object, STaggedEdge<object, object>> _simpleTestGraph3;

        [ClassInitialize()]
        public static void InitPsGraphUnitTest(TestContext tc)
        {
            InitializeSimpleTestGraph1();
            InitializeSimpleTestGraph2();
            InitializeSimpleTestGraph3();
        }

        private static void InitializeSimpleTestGraph1()
        {
            var g = new BidirectionalGraph<object, STaggedEdge<object, object>>();

            g.AddVertex("A");
            g.AddVertex("B");
            g.AddVertex("C");
            g.AddVertex("D");
            g.AddVertex("E");

            g.AddEdge(new STaggedEdge<object, object>("A", "C", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("C", "A", String.Empty));

            g.AddEdge(new STaggedEdge<object, object>("C", "E", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("E", "C", String.Empty));


            g.AddEdge(new STaggedEdge<object, object>("D", "C", String.Empty));

            g.AddEdge(new STaggedEdge<object, object>("B", "E", String.Empty));


            _simpleTestGraph1 = g;

            //throw new NotImplementedException();
        }

        private static void InitializeSimpleTestGraph2()
        {
            var g = new BidirectionalGraph<object, STaggedEdge<object, object>>();

            g.AddVertex("A");
            g.AddVertex("B");
            g.AddVertex("C");
            g.AddVertex("D");
            g.AddVertex("E");
            g.AddVertex("F");
            g.AddVertex("G");

            g.AddEdge(new STaggedEdge<object, object>("F", "A", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("E", "A", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("B", "A", String.Empty));

            g.AddEdge(new STaggedEdge<object, object>("G", "B", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("D", "B", String.Empty));

            g.AddEdge(new STaggedEdge<object, object>("G", "C", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("D", "C", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("B", "C", String.Empty));

            g.AddEdge(new STaggedEdge<object, object>("B", "D", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("C", "D", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("E", "D", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("G", "D", String.Empty));

            g.AddEdge(new STaggedEdge<object, object>("D", "E", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("F", "E", String.Empty));

            g.AddEdge(new STaggedEdge<object, object>("A", "F", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("E", "F", String.Empty));

            g.AddEdge(new STaggedEdge<object, object>("B", "G", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("C", "G", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("D", "G", String.Empty));

            System.IO.File.WriteAllText("c:\\temp\\test2.gv", g.ToGraphviz()); 

            _simpleTestGraph2 = g;

            //throw new NotImplementedException();
        }

        private static void InitializeSimpleTestGraph3()
        {
            var g = new BidirectionalGraph<object, STaggedEdge<object, object>>();

            g.AddVertex("A");
            g.AddVertex("B");
            g.AddVertex("C");
            g.AddVertex("D");
            g.AddVertex("E");
            g.AddVertex("F");
            g.AddVertex("G");
            g.AddVertex("H");

            g.AddEdge(new STaggedEdge<object, object>("D", "A", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("H", "A", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("E", "B", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("G", "B", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("E", "C", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("F", "D", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("H", "D", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("B", "E", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("C", "E", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("G", "E", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("A", "F", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("H", "F", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("B", "G", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("C", "G", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("A", "H", String.Empty));
            g.AddEdge(new STaggedEdge<object, object>("D", "H", String.Empty));



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
