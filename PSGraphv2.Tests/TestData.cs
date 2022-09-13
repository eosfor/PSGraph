using PSGraph.Model;
using QuikGraph.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PSGraph.Tests
{
    public static class TestData
    {
        public static PSBidirectionalGraph SimpleTestGraph1 { get; private set; }
        public static PSBidirectionalGraph SimpleTestGraph2 { get; private set; }
        public static PSBidirectionalGraph SimpleTestGraph3 { get; private set; }
        public static PSBidirectionalGraph SimpleTestGraph4 { get; private set; }
        public static PSBidirectionalGraph DSMFull { get; private set; }

        static TestData()
        {
            SimpleTestGraph1 = InitializeSimpleTestGraph1();
            SimpleTestGraph2 = InitializeSimpleTestGraph2();
            SimpleTestGraph3 = InitializeSimpleTestGraph3();
            SimpleTestGraph4 = InitializeSimpleTestGraph4();
            DSMFull = InitializeSimpleTestGraph5();
        }

        
        private static PSBidirectionalGraph? InitializeSimpleTestGraph5(){
            var g = new PSBidirectionalGraph();

            g.AddVertex(new PSVertex("A"));
            g.AddVertex(new PSVertex("B"));
            g.AddVertex(new PSVertex("C"));
            g.AddVertex(new PSVertex("D"));
            g.AddVertex(new PSVertex("E"));
            g.AddVertex(new PSVertex("F"));
            g.AddVertex(new PSVertex("G"));

            g.AddEdge(new PSEdge(new PSVertex("A"), new PSVertex("C"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("A"), new PSVertex("D"), new PSEdgeTag()));

            g.AddEdge(new PSEdge(new PSVertex("B"), new PSVertex("G"), new PSEdgeTag()));
            
            g.AddEdge(new PSEdge(new PSVertex("C"), new PSVertex("A"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("C"), new PSVertex("B"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("C"), new PSVertex("F"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("C"), new PSVertex("G"), new PSEdgeTag()));

            g.AddEdge(new PSEdge(new PSVertex("D"), new PSVertex("B"), new PSEdgeTag()));
            
            g.AddEdge(new PSEdge(new PSVertex("E"), new PSVertex("C"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("E"), new PSVertex("F"), new PSEdgeTag()));

            g.AddEdge(new PSEdge(new PSVertex("G"), new PSVertex("D"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("G"), new PSVertex("F"), new PSEdgeTag()));

            return g;
        }
        private static PSBidirectionalGraph? InitializeSimpleTestGraph4()
        {
            var d = System.IO.Directory.GetCurrentDirectory();
            var testGraphPath = System.IO.Path.Combine(d, "vms.graphml");
            var graph = new PSBidirectionalGraph(false);
            using (var xmlReader = XmlReader.Create(testGraphPath))
            {
                graph.DeserializeFromGraphML<PSVertex, PSEdge, PSBidirectionalGraph>(
                    xmlReader,
                    id => new PSVertex(id),
                    (source, target, id) => new PSEdge(source, target, new PSEdgeTag()));
            }
            return graph;
        }


        private static PSBidirectionalGraph InitializeSimpleTestGraph1()
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

            return g;
        }

        private static PSBidirectionalGraph InitializeSimpleTestGraph2()
        {
            var g = new PSBidirectionalGraph();

            g.AddVertex(new PSVertex("A"));
            g.AddVertex(new PSVertex("B"));
            g.AddVertex(new PSVertex("C"));
            g.AddVertex(new PSVertex("D"));
            g.AddVertex(new PSVertex("E"));
            g.AddVertex(new PSVertex("F"));
            g.AddVertex(new PSVertex("G"));


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

            return g;
        }

        private static PSBidirectionalGraph InitializeSimpleTestGraph3()
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

            return g;
        }
    }
}
