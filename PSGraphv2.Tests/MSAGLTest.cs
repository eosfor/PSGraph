using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSGraph.Model;
using QuikGraph;
using System.IO;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;
using QuikGraph.Graphviz;
using QuikGraph.MSAGL;

namespace PSGraph.Tests
{
    [TestClass]
    public class MSAGLTest
    {
		static private PowerShell _powershell;

		private static PSBidirectionalGraph _simpleTestGraph1;
		private static PSBidirectionalGraph _simpleTestGraph2;
		private static PSBidirectionalGraph _simpleTestGraph3;

		[ClassInitialize()]
		public static void InitPsGraphUnitTest(TestContext tc)
		{
			_powershell = PowerShell.Create();
			_powershell.AddCommand("Import-Module")
				.AddParameter("Assembly", System.Reflection.Assembly.GetAssembly(typeof(PSGraph.NewPsGraphCmdlet)));
			_powershell.Invoke();
			_powershell.Commands.Clear();


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

        [ClassCleanup()]
		public static void CleanupPsGraphUnitTest()
		{
			_powershell?.Dispose();
		}


		[TestMethod]
		public void TestAllGraphTypeProcessing_Success()
		{
            var drawingGraph = _simpleTestGraph2.ToMsaglGraph();
            drawingGraph.CreateGeometryGraph();

			// Now the drawing graph elements point to the corresponding geometry elements, 
			// however the node boundary curves are not set.
			// Setting the node boundaries
			foreach (var n in drawingGraph.Nodes)
			{
				// Ideally we should look at the drawing node attributes, and figure out, the required node size
				// I am not sure how to find out the size of a string rendered in SVG. Here, we just blindly assign to each node a rectangle with width 60 and height 40, and round its corners.
				n.GeometryNode.BoundaryCurve = CurveFactory.CreateRectangleWithRoundedCorners(60, 40, 3, 2, new Point(0, 0));
			}

			AssignLabelsDimensions(drawingGraph);

			LayoutHelpers.CalculateLayout(drawingGraph.GeometryGraph, new SugiyamaLayoutSettings(), null);
			PrintSvgAsString(drawingGraph);
		}

        [TestMethod]
        public void TestAllGraphTypeProcessingPowershell_Success()
        {
            var graph = _simpleTestGraph2;

            try
            {
                _powershell.AddCommand("Export-Graph");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"Format", GraphExportTypes.MSAGL_MDS}, { "Graph", graph }, { "Path", "c:\\temp\\msagl.svg" }
                    });
                Collection<PSObject> result = _powershell.Invoke();

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }

        void AssignLabelsDimensions(Graph drawingGraph)
		{
            // In general, the label dimensions should depend on the viewer
            foreach (var n in drawingGraph.Nodes)
            {
                n.Label.Width = n.Width * 0.6;
                n.Label.Height = 40;
                n.Attr.FillColor = Color.Azure;
            }
		}

		void PrintSvgAsString(Graph drawingGraph)
		{
			var ms = new MemoryStream();
			var writer = new StreamWriter(ms);
			var svgWriter = new SvgGraphWriter(writer.BaseStream, drawingGraph);
			svgWriter.Write();
			// get the string from MemoryStream
			ms.Position = 0;
			var sr = new StreamReader(ms);
			var myStr = sr.ReadToEnd();

            System.IO.File.WriteAllText("c:\\temp\\ttt.svg", myStr);
            //System.Console.WriteLine(myStr);
        }
	}
}
