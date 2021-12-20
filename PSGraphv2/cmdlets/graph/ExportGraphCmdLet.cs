using System;
using System.Management.Automation;
using QuikGraph;
using QuikGraph.Graphviz;
using System.Reflection;
using System.Xml;
using QuikGraph.Serialization;
using System.IO;
using System.Text;
using System.Linq;
using PSGraph.Model;
using QuikGraph.Algorithms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Miscellaneous;
using QuikGraph.MSAGL;

namespace PSGraph
{
    [Cmdlet(VerbsData.Export, "Graph")]
    public class ExportGraphCmdLet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSBidirectionalGraph Graph;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public ExportTypes Format;

        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string Path;

        protected override void ProcessRecord()
        {
            switch (Format)
            {
                case ExportTypes.Graphviz:
                    ExportGraphViz();
                    break;
                case ExportTypes.GraphML:
                    ExportGraphML();
                    break;
                case ExportTypes.MSAGL:
                    ExportMSAGL();
                    break;
                default:
                    break;
            }
        }

        private void ExportMSAGL()
        {
            var drawingGraph = Graph.ToMsaglGraph();
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

            System.IO.File.WriteAllText(Path, myStr);
        }

        private void ExportGraphML()
        {
            using (var xmlWriter = XmlWriter.Create(Path))
            {
                Graph.SerializeToGraphML<PSVertex, PSEdge, PSBidirectionalGraph>(xmlWriter,
                    v => v.Label,
                    Graph.GetEdgeIdentity());
            }
        }

        private void ExportGraphViz()
        {
            var graphviz = new GraphvizAlgorithm<PSVertex, PSEdge>(Graph);
            graphviz.FormatVertex += Graphviz_FormatVertex;
            var result = graphviz.Generate();

            if (Path != null)
            {
                File.WriteAllText(Path, result);
            }
            else
            {
                WriteObject(result);
            }
        }

        private void Graphviz_FormatVertex(object sender, FormatVertexEventArgs<PSVertex> args)
        {
            foreach (PropertyInfo p in args.Vertex.GVertexParameters.GetType().GetProperties())
            {
                var destProperty = args.VertexFormat.GetType().GetProperty(p.Name);
                if (destProperty != null)
                {
                    destProperty.SetValue(args.VertexFormat, p.GetValue(args.Vertex.GVertexParameters));
                }
            }
        }
    }
}
