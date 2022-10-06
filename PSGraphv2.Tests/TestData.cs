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
        public static PsBidirectionalGraph SimpleTestGraph1 { get; private set; }
        public static PsBidirectionalGraph SimpleTestGraph2 { get; private set; }
        public static PsBidirectionalGraph SimpleTestGraph3 { get; private set; }
        public static PsBidirectionalGraph SimpleTestGraph4 { get; private set; }
        public static PsBidirectionalGraph DSMFull { get; private set; }
        
        public static PsBidirectionalGraph SimpleTestGraph5 { get; private set; }

        static TestData()
        {
            SimpleTestGraph1 = InitializeSimpleTestGraph1();
            SimpleTestGraph2 = InitializeSimpleTestGraph2();
            SimpleTestGraph3 = InitializeSimpleTestGraph3();
            SimpleTestGraph4 = InitializeSimpleTestGraph4();
            SimpleTestGraph5 = InitializeSimpleTestGraph5();
            DSMFull = InitializeDSMFull();
        }

        private static PsBidirectionalGraph InitializeSimpleTestGraph5()
        {
            var g = new PsBidirectionalGraph();

            g.AddVertex(new PSVertex("A"));
            g.AddVertex(new PSVertex("D"));
            g.AddVertex(new PSVertex("E"));
            g.AddVertex(new PSVertex("F"));
            g.AddVertex(new PSVertex("G"));
            g.AddVertex(new PSVertex("M"));
            g.AddVertex(new PSVertex("B"));
            g.AddVertex(new PSVertex("H"));
            g.AddVertex(new PSVertex("I"));
            
            
//             Add-Edge -From A -To D -Graph $g | Out-Null
//             Add-Edge -From A -To E -Graph $g | Out-Null
//             Add-Edge -From D -to F -Graph $g | Out-Null
//             Add-Edge -From E -To F -Graph $g | Out-Null
//             Add-Edge -From G -To M -Graph $g | Out-Null
//             Add-Edge -From B -To E -Graph $g | Out-Null
//             Add-Edge -From B -To G -Graph $g | Out-Null
//             Add-Edge -From B -To H -Graph $g | Out-Null
//             Add-Edge -From H -To I -Graph $g | Out-Null
//             Add-Edge -From M -To B -Graph $g | Out-Null

            
            g.AddEdge(new PSEdge(new PSVertex("A"), new PSVertex("D"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("A"), new PSVertex("E"), new PSEdgeTag()));
            
            g.AddEdge(new PSEdge(new PSVertex("D"), new PSVertex("F"), new PSEdgeTag()));
            
            g.AddEdge(new PSEdge(new PSVertex("E"), new PSVertex("F"), new PSEdgeTag()));
            
            g.AddEdge(new PSEdge(new PSVertex("G"), new PSVertex("M"), new PSEdgeTag()));
            
            g.AddEdge(new PSEdge(new PSVertex("B"), new PSVertex("E"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("B"), new PSVertex("G"), new PSEdgeTag()));
            g.AddEdge(new PSEdge(new PSVertex("B"), new PSVertex("H"), new PSEdgeTag()));
            
            g.AddEdge(new PSEdge(new PSVertex("H"), new PSVertex("I"), new PSEdgeTag()));
            
            g.AddEdge(new PSEdge(new PSVertex("M"), new PSVertex("B"), new PSEdgeTag()));
            
            return g;
        }
        
        private static PsBidirectionalGraph? InitializeDSMFull(){
            var g = new PsBidirectionalGraph();

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
        private static PsBidirectionalGraph InitializeSimpleTestGraph4()
        {
            var d = System.IO.Directory.GetCurrentDirectory();
            var testGraphPath = System.IO.Path.Combine(d, "vms.graphml");
            var graph = new PsBidirectionalGraph(false);
            using (var xmlReader = XmlReader.Create(testGraphPath))
            {
                graph.DeserializeFromGraphML<PSVertex, PSEdge, PsBidirectionalGraph>(
                    xmlReader,
                    id => new PSVertex(id),
                    (source, target, id) => new PSEdge(source, target, new PSEdgeTag()));
            }
            return graph;
        }


        private static PsBidirectionalGraph InitializeSimpleTestGraph1()
        {
            var g = new PsBidirectionalGraph();

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

        private static PsBidirectionalGraph InitializeSimpleTestGraph2()
        {
            var g = new PsBidirectionalGraph();

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

        private static PsBidirectionalGraph InitializeSimpleTestGraph3()
        {
            var g = new PsBidirectionalGraph();

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
