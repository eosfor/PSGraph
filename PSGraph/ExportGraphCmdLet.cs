using System;
using System.Management.Automation;
using QuickGraph;
using QuickGraph.Graphviz;
using System.Collections.Generic;
using System.Reflection;

namespace PSGraph
{
    [Cmdlet(VerbsData.Export, "Graph")]
    public class ExportGraphCmdLet : PSCmdlet
    {
        private ExportTypes _exportType;

        [Parameter(Mandatory = true)]
        public Object Graph { get; set; }

        [Parameter(Mandatory = true)]
        public ExportTypes Format
        {
            get { return _exportType; }
            set
            {
                if (_exportType != value)
                {
                    if (!Enum.IsDefined(typeof(ExportTypes), value))
                        throw new System.ArgumentException();
                    _exportType = value;
                }
            }
        }

        [Parameter]
        public String Path { get; set; }


        protected override void ProcessRecord()
        {
            Object graph = Graph;
            if (graph is PSObject)
                graph = ((PSObject)graph).ImmediateBaseObject;
            if (graph == null)
            {
                throw new System.ArgumentException("'Graph' mustn't be equal to null");
            }

            WriteVerbose("Add-Edge: Graph type is: " + Graph.GetType().ToString());

            var edgeList = graph as IEdgeListGraph<Object, STaggedEdge<Object, Object>>;
            if (edgeList == null)
            {
                throw new System.ArgumentException("'Graph' is an object of an unknown type");
            }

            var graphviz = new GraphvizAlgorithm<Object, STaggedEdge<Object, Object>>(edgeList);
            //graphviz.FormatVertex += new FormatVertexEventHandler<Object>((sender, vertexFormat) => { });
            graphviz.FormatVertex += FormatVertexEventHandler;
            string result = graphviz.Generate();

            String path = Path;
            if (path != null)
            {
                System.IO.File.WriteAllText(path, result);
            }
            else
            {
                WriteObject(result);
            }
        }

        public static void FormatVertexEventHandler(object sender, FormatVertexEventArgs<object> e)
        {
            PropertyInfo labelProp = e.Vertex.GetType().GetProperty("Label");

            if (labelProp != null) {
                e.VertexFormatter.Label = (string)(labelProp.GetValue(e.Vertex, null));
            }
        }

    }
}
