using System.Management.Automation;
using QuikGraph.Graphviz;
using System.Reflection;
using System.Xml;
using QuikGraph.Serialization;
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
using PSGraph.Vega.Extensions;
using PSGraph.Vega.Spec;

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsData.Export, "Graph")]
    public class ExportGraphViewCmdLet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PsBidirectionalGraph Graph;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public GraphExportTypes Format;

        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string Path;

        private VegaExportTypes _vegaExportType = VegaExportTypes.JSON;


        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (MyInvocation.BoundParameters.ContainsKey("Path"))
            {
                var extension = System.IO.Path.GetExtension(Path);

                switch (extension)
                {
                    case ".html":
                        _vegaExportType = VegaExportTypes.HTML;
                        break;
                    case ".json":
                        _vegaExportType = VegaExportTypes.JSON;
                        break;
                    case ".graphml":
                        _vegaExportType = VegaExportTypes.GRAPHML;
                        break;
                    case ".svg":
                        _vegaExportType = VegaExportTypes.SVG;
                        break;
                    case ".dot":
                        _vegaExportType = VegaExportTypes.DOT;
                        break;
                    default:
                        WriteAndAbort();
                        break;
                }
            }

        }

        protected override void ProcessRecord()
        {
            string result = string.Empty;

            switch (Format)
            {
                case GraphExportTypes.Graphviz:
                    result = ExportGraphViz();
                    break;
                case GraphExportTypes.GraphML:
                    result = ExportGraphMLAsString();
                    break;
                case GraphExportTypes.MSAGL_FASTINCREMENTAL:
                case GraphExportTypes.MSAGL_MDS:
                case GraphExportTypes.MSAGL_SUGIYAMA:
                    result = ExportMSAGL();
                    break;
                case GraphExportTypes.Vega_ForceDirected:
                    result = ExportVegaForceDirected(_vegaExportType);
                    break;
                case GraphExportTypes.Vega_AdjacencyMatrix:
                    result = ExportVegaAdjacencyMatrix(_vegaExportType);
                    break;
                case GraphExportTypes.Vega_TreeLayout:
                    result = ExportVegaTreeLayout(_vegaExportType);
                    break;
                default:
                    result = ExportGraphViz();
                    break;
            }

            if (MyInvocation.BoundParameters.ContainsKey("Path"))
            {
                File.WriteAllText(Path, result);
            }
            else
            {
                WriteObject(result);
            }
        }

        private string ExportVegaTreeLayout(VegaExportTypes exportType)
        {
            var modulePath = MyInvocation.MyCommand.Module?.ModuleBase;
            var records = Graph.ConvertToParentChildList();
            var vega = VegaHelper.GetVegaTemplateObjectFromModulePath(modulePath, "vega.tree.layout.json");
            vega.Data[0].Values = records.ToList<object>();

            switch (exportType)
            {
                case VegaExportTypes.HTML:
                    return VegaHelper.RenderHtml(vega);
                default:
                case VegaExportTypes.JSON:
                    return vega.ToJson();
            }

            //string html = VegaHelper.RenderHtml(vega);

            //return html;

            //File.WriteAllText(Path, html);
        }

        private string ExportVegaAdjacencyMatrix(VegaExportTypes exportType)
        {
            var modulePath = MyInvocation.MyCommand.Module?.ModuleBase;
            var records = Graph.ConvertToVegaNodeLink();
            var vega = VegaHelper.GetVegaTemplateObjectFromModulePath(modulePath, "vega.adj.matrix.json");

            vega.Data[0].Values = records.nodes.ToList<object>();
            vega.Data[1].Values = records.links.ToList<object>();

            switch (exportType)
            {
                case VegaExportTypes.HTML:
                    return VegaHelper.RenderHtml(vega);
                default:
                case VegaExportTypes.JSON:
                    return vega.ToJson();

            }

            //File.WriteAllText(Path, html);
        }

        private string ExportVegaForceDirected(VegaExportTypes exportType)
        {
            var modulePath = MyInvocation.MyCommand.Module?.ModuleBase;
            var records = Graph.ConvertToVegaNodeLink();
            var vega = VegaHelper.GetVegaTemplateObjectFromModulePath(modulePath, "vega.force.directed.layout.json");

            vega.Data[0].Values = records.nodes.ToList<object>();
            vega.Data[1].Values = records.links.ToList<object>();

            switch (exportType)
            {
                case VegaExportTypes.HTML:
                    return VegaHelper.RenderHtml(vega);
                default:
                case VegaExportTypes.JSON:
                    return vega.ToJson();

            }
            //File.WriteAllText(Path, html);
        }

        private string ExportMSAGL()
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
            return PrintSvgAsString(drawingGraph);
        }

        void AssignLabelsDimensions(Graph drawingGraph)
        {
            // In general, the label dimensions should depend on the viewer
            foreach (var n in drawingGraph.Nodes)
            {
                n.Label.Width = n.Width * 0.99;
                n.Label.Height = 40;
                n.Attr.FillColor = Microsoft.Msagl.Drawing.Color.Azure;
            }
        }

        string PrintSvgAsString(Graph drawingGraph)
        {
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            var svgWriter = new SvgGraphWriter(writer.BaseStream, drawingGraph);
            svgWriter.Write();
            // get the string from MemoryStream
            ms.Position = 0;
            var sr = new StreamReader(ms);
            var myStr = sr.ReadToEnd();

            return myStr;

            //System.IO.File.WriteAllText(Path, myStr);
        }

        private void ExportGraphML()
        {
            using (var xmlWriter = XmlWriter.Create(Path))
            {
                Graph.SerializeToGraphML<PSVertex, PSEdge, PsBidirectionalGraph>(xmlWriter,
                    v => v.Label,
                    Graph.GetEdgeIdentity());
            }
        }

        private string ExportGraphMLAsString()
        {
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
            {
                Graph.SerializeToGraphML<PSVertex, PSEdge, PsBidirectionalGraph>(
                    xmlWriter,
                    v => v.Label,
                    Graph.GetEdgeIdentity()
                );
                xmlWriter.Flush();
                return stringWriter.ToString();
            }
        }

        private string ExportGraphViz()
        {
            var graphviz = new GraphvizAlgorithm<PSVertex, PSEdge>(Graph);
            graphviz.FormatVertex += Graphviz_FormatVertex;
            var result = graphviz.Generate();

            return result;
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

        private void WriteAndAbort()
        {
            var errorRecord = new ErrorRecord(
                new ArgumentException("Unsupported file extension. Use .html or .json"),
                "UnsupportedExtension",
                ErrorCategory.InvalidArgument,
                Path
            );

            WriteError(errorRecord);
            ThrowTerminatingError(errorRecord);
            return; // прерывает дальнейшее выполнение ProcessRecord / BeginProcessing
        }
    }
}
