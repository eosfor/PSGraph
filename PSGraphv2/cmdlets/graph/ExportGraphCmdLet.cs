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
                default:
                    break;
            }
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
