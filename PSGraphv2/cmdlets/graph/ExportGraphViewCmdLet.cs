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
using Microsoft.Msagl.Layout.MDS;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Core.Layout;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsData.Export, "Graph")]
    public class ExportGraphViewCmdLet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSBidirectionalGraph Graph;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public GraphExportTypes Format;

        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string Path;

        protected override void ProcessRecord()
        {
            switch (Format)
            {
                case GraphExportTypes.Graphviz:
                    ExportGraphViz();
                    break;
                case GraphExportTypes.GraphML:
                    ExportGraphML();
                    break;
                case GraphExportTypes.MSAGL_FASTINCREMENTAL:
                case GraphExportTypes.MSAGL_MDS:
                case GraphExportTypes.MSAGL_SUGIYAMA:
                    ExportMSAGL();
                    break;
                default:
                    ExportGraphViz();
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
                var w = 0.68 * (n.LabelText.Length * n.Label.FontSize);
                n.GeometryNode.BoundaryCurve = CurveFactory.CreateRectangleWithRoundedCorners(w, 40, 3, 2, new Point(0, 0));
            }

            AssignLabelsDimensions(drawingGraph);

            LayoutAlgorithmSettings? las = null;
            switch (Format)
            {
                case GraphExportTypes.MSAGL_MDS:
                    las = new MdsLayoutSettings();
                    break;
                case GraphExportTypes.MSAGL_SUGIYAMA:
                    las = new SugiyamaLayoutSettings();
                    break;
                case GraphExportTypes.MSAGL_FASTINCREMENTAL:
                    las = new FastIncrementalLayoutSettings();
                    break;
            }
            
            LayoutHelpers.CalculateLayout(drawingGraph.GeometryGraph, las, null);
            PrintSvgAsString(drawingGraph);
        }

        void AssignLabelsDimensions(Graph drawingGraph)
        {
            // In general, the label dimensions should depend on the viewer
            foreach (var n in drawingGraph.Nodes)
            {
                n.Label.Width = n.Width * 0.99;
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
